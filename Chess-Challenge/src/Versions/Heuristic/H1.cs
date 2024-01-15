using ChessChallenge.API;
using System;
using System.Linq;

public class H1 : IChessBot
{

    int[] pieceValues = new int[] { 0, 100, 300, 320, 500, 900, 900 };
    long count = 0; //#DEBUG
    Board board;
    Move bestMove;
    int searchDepth;

    //TODO: still shit at mating - mop up eval
    public Move Think(Board board, Timer timer)
    {
        // ulong t =        0b0000011111111100000000000000000000000000000000000000000000000000;
        // ulong mask =     0b0000000000000000000000000000000000000000000000000000000111111111;
        // ulong signMask = 0b0000000000000000000000000000000000000000000000000000001000000000;
        // ulong shifted = t >> 50;
        // long test = (int)(shifted & mask) * ((shifted & signMask) > 0 ? -1 : 1);
        // Console.WriteLine(shifted + " " + test);
        this.board = board;
        count = 0; //#DEBUG
        int taken = 32 - BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard);
        int depth = 4 + (taken - 10) / 4;
        if (taken > 28) depth += 1;
        if (timer.MillisecondsRemaining < 15000) depth -= 1;
        if (timer.MillisecondsRemaining < 8000) depth -= 1;
        if (timer.MillisecondsRemaining < 1000) depth = 2;
        if (board.PlyCount < 2) depth = 1;
        Console.WriteLine("Depth: " + depth); //#DEBUG
        searchDepth = depth;
        int eval = Search(depth, -99999999, 99999999);
        Console.WriteLine("Eval: " + eval); //#DEBUG
        // Move m = Search(board, 4, int.MinValue, int.MaxValue).Move;
        Console.WriteLine("Positions: " + count); //#DEBUG
        return bestMove;
    }

    //TODO: transposition table
    //TODO: iterative deepening
    //TODO: opening
    // //TODO: check for draw / mate, but be cogniscent that moves aren't usually forced
    // //TODO: pruning: https://www.chessprogramming.org/Quiescence_Search - Limiting Quiescence + standing pat (am I already doing this with a/b?)
    

    int Search(int depth, int alpha, int beta) {
        int max = -99999999;
        if (depth <= 0) {
            max = Evaluate();
            if (max > beta) return max;
            alpha = Math.Max(alpha, max);
        } else {
            if (board.IsDraw()) return 0;
            if (board.IsInCheckmate()) return -99999999 + 20 - depth;
        }

        foreach (Move move in OrderMoves(board.GetLegalMoves(depth <= 0))) {
            board.MakeMove(move);
            int eval = -Search(depth - 1, -beta, -alpha);
            board.UndoMove(move);

            if (eval > max)
            {
                max = eval;
                if (searchDepth == depth) bestMove = move;
            }

            if (max > beta) break;
            alpha = Math.Max(alpha, max);
        }

        return max;
    }

    // From POV of player who's turn it is. Assumes quiet position.
    int Evaluate()
    {
        count++; //#DEBUG
        return Evaluate(board.IsWhiteToMove) - Evaluate(!board.IsWhiteToMove);
    }

    //FIXME: still worse than old version
    int Evaluate(bool isWhite)
    {
        //TODO:
        // Centre control
        // King safety
        // King aggression in late game
        // Promotion
        // Number of controlled squares
        // Check
        // Connected/doubled rooks
        // Knights on the rim are dim
        // Game phases with tapered eval

        int phase = Phase();

        (int, int) pawns = Pawns(isWhite);
        (int, int) knights = Knights(isWhite);
        (int, int) bishops = Bishops(isWhite);
        (int, int) rooks = Rooks(isWhite);
        (int, int) queens = Queens(isWhite);
        (int, int) king = King(isWhite);

        int mobility = board.GetLegalMoves().Length;

        int early = pawns.Item1 + knights.Item1 + bishops.Item1 + rooks.Item1 + queens.Item1 + king.Item1 + mobility;
        int late = pawns.Item2 + knights.Item2 + bishops.Item2 + rooks.Item2 + queens.Item2 + king.Item2 + mobility;

        // early = pawns.Item1;
        // late = pawns.Item2;

        // Console.WriteLine("early: " + early + ", late: " + late + ", phase: " + phase + ", eval: " + (((50 - phase) * early + phase * late) / 50));
        return ((50 - phase) * early + phase * late) / 50;
    }
    //     return GetMaterial(isWhite)
    //         + ((50 - phase) * pawns.Item1 + phase * pawns.Item2) / 50
    //         + 5 * board.GetLegalMoves().Length
    //         + 100 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, isWhite) & 0b0000000000000000000000000001100000011000000000000000000000000000)
    //         + 50 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.King, isWhite) & 0b1100011100000000000000000000000000000000000000000000000011000111)
    //         - 30 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Knight, isWhite) & 0b1111111110000001100000011000000110000001100000011000000111111111)
    //         - 20 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Bishop, isWhite) & 0b1111111100000000000000000000000000000000000000000000000011111111)
    //         - 10 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Queen, isWhite) & 0b1111111100000000000000000000000000000000000000000000000011111111);
    // }

    // int GetMaterial(bool isWhite)
    // {
    //     return GetMaterial(PieceType.Knight, isWhite)
    //         + GetMaterial(PieceType.Bishop, isWhite)
    //         + GetMaterial(PieceType.Rook, isWhite)
    //         + GetMaterial(PieceType.Queen, isWhite);
    // }

    // int GetMaterial(PieceType pieceType, bool isWhite)
    // {
    //     return pieceValues[(int)pieceType] * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(pieceType, isWhite));
    // }

    //TODO:
    // Passed pawns
    // Connected pawns / lack of isolated pawns
    // Doubled pawns
    (int, int) Pawns(bool isWhite)
    {
        (int, int) score = (0, 0);
        PieceList pawns = board.GetPieceList(PieceType.Pawn, isWhite);
        foreach (Piece pawn in pawns)
        {
            // Console.WriteLine("" + pawn.Square + ": " + (pieceValues[1] + 15 * CentralBonus(pawn) + 5 * RelativeRank(pawn)));
            score.Item1 += pieceValues[1] + 15 * CentralBonus(pawn) + 5 * RelativeRank(pawn);
            score.Item2 += pieceValues[1] + 5 * CentralBonus(pawn) + 20 * RelativeRank(pawn);
        }
        return score;
    }

    (int, int) Knights(bool isWhite)
    {
        (int, int) score = (0, 0);
        PieceList knights = board.GetPieceList(PieceType.Knight, isWhite);
        foreach (Piece knight in knights)
        {
            score.Item1 += pieceValues[2] + 5 * CentralBonus(knight);
            score.Item2 += pieceValues[2] + 5 * CentralBonus(knight);
        }
        return score;
    }

    (int, int) Bishops(bool isWhite)
    {
        (int, int) score = (0, 0);
        PieceList bishops = board.GetPieceList(PieceType.Bishop, isWhite);
        bool pair = bishops.Count > 1;
        foreach (Piece bishop in bishops)
        {
            score.Item1 += pieceValues[3] + 2 * CentralBonus(bishop) + (pair ? 20 : 0);
            score.Item2 += pieceValues[3] + 5 * CentralBonus(bishop) + (pair ? 40 : 0);
        }
        return score;
    }

    (int, int) Rooks(bool isWhite)
    {
        (int, int) score = (0, 0);
        PieceList rooks = board.GetPieceList(PieceType.Rook, isWhite);
        foreach (Piece rook in rooks)
        {
            score.Item1 += pieceValues[4] + RelativeRank(rook) == 6 ? 90 : 0;
            score.Item2 += pieceValues[4] + 2 * CentralBonus(rook);
        }
        return score;
    }

    (int, int) Queens(bool isWhite)
    {
        (int, int) score = (0, 0);
        PieceList queens = board.GetPieceList(PieceType.Queen, isWhite);
        foreach (Piece queen in queens)
        {
            score.Item1 += pieceValues[5];
            score.Item2 += pieceValues[5] + 5 * CentralBonus(queen);
        }
        return score;
    }

    (int, int) King(bool isWhite)
    {
        (int, int) score = (10000, 10000);
        Piece king = board.GetPiece(board.GetKingSquare(isWhite));
        score.Item1 -= 50 * RelativeRank(king) + 100 * CentralBonus(king);
        score.Item2 += 20 * CentralBonus(king);
        return score;
    }

    int CentralBonus(Piece piece)
    {
        double x = piece.Square.File - 3.5;
        double y = piece.Square.Rank - 3.5;
        return 5 - (int)Math.Round(Math.Sqrt(x * x + y * y));
    }

    int RelativeRank(Piece piece)
    {
        int rank = piece.Square.Rank;
        return piece.IsWhite ? rank : 7 - rank;
    }

    //TODO: improve, probably places too much emphasis on pawns
    //0 - 50
    int Phase()
    {
        return Math.Max(0, 50 - (BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard)
            + 9 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Queen, true) | board.GetPieceBitboard(PieceType.Queen, false))));
    }

    Move[] OrderMoves(Span<Move> moves)
    {
        return moves.ToArray().OrderByDescending(move => ScoreMove(move)).ToArray();
    }

    int ScoreMove(Move move) {
        int score = 0;
        if (move.IsCapture) score += 10 * pieceValues[(int)move.CapturePieceType] - pieceValues[(int)move.MovePieceType];
        if (move.IsPromotion) score += pieceValues[(int)move.PromotionPieceType];
        if (move.IsCastles) score += 300;
        return score;
    }
}