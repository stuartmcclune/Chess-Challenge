using ChessChallenge.API;
using System;



public class OG1 : IChessBot
{

    int[] pieceValues = new int[] { 0, 100, 300, 320, 500, 900, 900 };
    // long count = 0; //#DEBUG
    Board board;

    //TODO: still shit at mating
    public Move Think(Board board, Timer timer)
    {
        this.board = board;
        // count = 0; //#DEBUG
        int depth = 4;
        int pieces = BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard);
        if (pieces < 18) depth += 1;
        if (pieces < 12) depth += 1;
        if (pieces < 9) depth += 1;
        if (pieces < 6) depth += 1;
        if (pieces < 4) depth += 1;
        if (board.PlyCount < 2) depth = 1;
        if (timer.MillisecondsRemaining < 15000) depth -= 1;
        if (timer.MillisecondsRemaining < 8000) depth -= 1;
        if (timer.MillisecondsRemaining < 1000) depth = 2;
        // Console.WriteLine("Depth: " + depth); //#DEBUG
        (Move m, int eval) = Search(depth, -99999999, 99999999);
        // Console.WriteLine("Eval: " + eval); //#DEBUG
        //                                     // Move m = Search(board, 4, int.MinValue, int.MaxValue).Move;
        // Console.WriteLine("Positions: " + count); //#DEBUG
        return m;
    }

    //TODO: transposition table
    //TODO: iterative deepening
    //TODO: memory efficient GetLegalMoves
    //TODO: opening
    (Move Move, int Score) Search(int depth, int alpha, int beta)
    {
        if (board.IsDraw()) return (Move.NullMove, 0);
        if (board.IsInCheckmate()) return (Move.NullMove, -99999999 + 20 - depth);
        if (depth == 0) return (Move.NullMove, CapturesSearch(alpha, beta));

        int max = -99999999;
        Move best = Move.NullMove;
        foreach (Move move in OrderMoves(board.GetLegalMoves()))
        {
            board.MakeMove(move);
            int eval = -Search(depth - 1, -beta, -alpha).Score;
            board.UndoMove(move);

            if (eval > max)
            {
                best = move;
                max = eval;
            }

            if (max > beta) break;
            alpha = Math.Max(alpha, max);
        }

        return (best, max);
    }

    //TODO: check for draw / mate, but be cogniscent that moves aren't usually forced
    int CapturesSearch(int alpha, int beta)
    {
        int max = Evaluate();

        if (max > beta) return max;
        alpha = Math.Max(alpha, max);

        foreach (Move capture in OrderMoves(board.GetLegalMoves(true)))
        {
            board.MakeMove(capture);
            max = Math.Max(-CapturesSearch(-beta, -alpha), max);
            board.UndoMove(capture);

            if (max > beta) break;
            alpha = Math.Max(alpha, max);
        }

        return max;

    }

    // From POV of player who's turn it is. Assumes quiet position.
    int Evaluate()
    {
        // count++; //#DEBUG
        return Evaluate(board.IsWhiteToMove) - Evaluate(!board.IsWhiteToMove);
    }

    int Evaluate(bool isWhite)
    {
        //TODO:
        // Centre control
        // King safety
        // King aggression in late game
        // Passed pawns
        // Promotion
        // Connected pawns / lack of isolated pawns
        // Number of controlled squares
        // Check
        // Doubled pawns
        // Connected/doubled rooks
        // Knights on the rim are dim
        // Game phases with tapered eval
        return GetMaterial(isWhite)
            + 100 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, isWhite) & 0b0000000000000000000000000001100000011000000000000000000000000000)
            + 50 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.King, isWhite) & 0b1100011100000000000000000000000000000000000000000000000011000111)
            - 30 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Knight, isWhite) & 0b1111111110000001100000011000000110000001100000011000000111111111)
            - 20 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Bishop, isWhite) & 0b1111111100000000000000000000000000000000000000000000000011111111)
            - 10 * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Queen, isWhite) & 0b1111111100000000000000000000000000000000000000000000000011111111);
    }

    int GetMaterial(bool isWhite)
    {
        return GetMaterial(PieceType.Pawn, isWhite)
            + GetMaterial(PieceType.Knight, isWhite)
            + GetMaterial(PieceType.Bishop, isWhite)
            + GetMaterial(PieceType.Rook, isWhite)
            + GetMaterial(PieceType.Queen, isWhite);
    }

    int GetMaterial(PieceType pieceType, bool isWhite)
    {
        return pieceValues[(int)pieceType] * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(pieceType, isWhite));
    }

    // int GetMaterialDifference(PieceType pieceType, Board board)
    // {
    //     return pieceValues[(int)pieceType]
    //     * (BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(pieceType, board.IsWhiteToMove))
    //         - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(pieceType, !board.IsWhiteToMove)));
    // }

    Move[] OrderMoves(Move[] moves)
    {
        //TODO: pre-initialise to max possible size
        int[] scores = new int[moves.Length];
        for (int i = 0; i < moves.Length; i++)
        {
            int score = 0;
            Move move = moves[i];

            if (move.IsCapture) score += 10 * pieceValues[(int)move.CapturePieceType] - pieceValues[(int)move.MovePieceType];
            if (move.IsPromotion) score += pieceValues[(int)move.PromotionPieceType];
            if (move.IsCastles) score += 300;
            scores[i] = score;
        }

        for (int i = 0; i < moves.Length - 1; i++)
        {
            for (int j = i + 1; j > 0; j--)
            {
                int swap = j - 1;
                if (scores[swap] < scores[j])
                {
                    (moves[j], moves[swap]) = (moves[swap], moves[j]);
                    (scores[j], scores[swap]) = (scores[swap], scores[j]);
                }
            }
        }

        return moves;
    }
}
