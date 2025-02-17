﻿using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{

    //zobrist, depth, eval, flag, move
    //flags: 1 = exact, 2 = beta (lower bound), 3 = alpha (upper bound), 0 implies null
    (ulong, byte, int, byte, Move)[] TT = new (ulong, byte, int, byte, Move)[1_000_000];

    // 0000_King_Queen_Rook_Bishop_Knight_Pawn
    ulong[] mg_pst_enc = {
        0b0000_1_001000001_1_000011100_0_000100000_1_000011101_1_010100111_0_000000000, // -65 _-28 _ 32 _-29 _-167_ 0  
        0b0000_0_000010111_0_000000000_0_000101010_0_000000100_1_001011001_0_000000000, //  23 _ 0  _ 42 _ 4  _-89 _ 0  
        0b0000_0_000010000_0_000011101_0_000100000_1_001010010_1_000100010_0_000000000, //  16 _ 29 _ 32 _-82 _-34 _ 0  
        0b0000_1_000001111_0_000001100_0_000110011_1_000100101_1_000110001_0_000000000, // -15 _ 12 _ 51 _-37 _-49 _ 0  
        0b0000_1_000111000_0_000111011_0_000111111_1_000011001_0_000111101_0_000000000, // -56 _ 59 _ 63 _-25 _ 61 _ 0  
        0b0000_1_000100010_0_000101100_0_000001001_1_000101010_1_001100001_0_000000000, // -34 _ 44 _ 9  _-42 _-97 _ 0  
        0b0000_0_000000010_0_000101011_0_000011111_0_000000111_1_000001111_0_000000000, //  2  _ 43 _ 31 _ 7  _-15 _ 0  
        0b0000_0_000001101_0_000101101_0_000101011_1_000001000_1_001101011_0_000000000, //  13 _ 45 _ 43 _-8  _-107_ 0  

        0b0000_0_000011101_1_000011000_0_000011011_1_000011010_1_001001001_0_001100010, //  29 _-24 _ 27 _-26 _-73 _ 98 
        0b0000_1_000000001_1_000100111_0_000100000_0_000010000_1_000101001_0_010000110, // -1  _-39 _ 32 _ 16 _-41 _ 134
        0b0000_1_000010100_1_000000101_0_000111010_1_000010010_0_001001000_0_000111101, // -20 _-5  _ 58 _-18 _ 72 _ 61 
        0b0000_1_000000111_0_000000001_0_000111110_1_000001101_0_000100100_0_001011111, // -7  _ 1  _ 62 _-13 _ 36 _ 95 
        0b0000_1_000001000_1_000010000_0_001010000_0_000011110_0_000010111_0_001000100, // -8  _-16 _ 80 _ 30 _ 23 _ 68 
        0b0000_1_000000100_0_000111001_0_001000011_0_000111011_0_000111110_0_001111110, // -4  _ 57 _ 67 _ 59 _ 62 _ 126
        0b0000_1_000100110_0_000011100_0_000011010_0_000010010_0_000000111_0_000100010, // -38 _ 28 _ 26 _ 18 _ 7  _ 34 
        0b0000_1_000011101_0_000110110_0_000101100_1_000101111_1_000010001_1_000001011, // -29 _ 54 _ 44 _-47 _-17 _-11 

        0b0000_1_000001001_1_000001101_1_000000101_1_000010000_1_000101111_1_000000110, // -9  _-13 _-5  _-16 _-47 _-6  
        0b0000_0_000011000_1_000010001_0_000010011_0_000100101_0_000111100_0_000000111, //  24 _-17 _ 19 _ 37 _ 60 _ 7  
        0b0000_0_000000010_0_000000111_0_000011010_0_000101011_0_000100101_0_000011010, //  2  _ 7  _ 26 _ 43 _ 37 _ 26 
        0b0000_1_000010000_0_000001000_0_000100100_0_000101000_0_001000001_0_000011111, // -16 _ 8  _ 36 _ 40 _ 65 _ 31 
        0b0000_1_000010100_0_000011101_0_000010001_0_000100011_0_001010100_0_001000001, // -20 _ 29 _ 17 _ 35 _ 84 _ 65 
        0b0000_0_000000110_0_000111000_0_000101101_0_000110010_0_010000001_0_000111000, //  6  _ 56 _ 45 _ 50 _ 129_ 56 
        0b0000_0_000010110_0_000101111_0_000111101_0_000100101_0_001001001_0_000011001, //  22 _ 47 _ 61 _ 37 _ 73 _ 25 
        0b0000_1_000010110_0_000111001_0_000010000_1_000000010_0_000101100_1_000010100, // -22 _ 57 _ 16 _-2  _ 44 _-20 

        0b0000_1_000010001_1_000011011_1_000011000_1_000000100_1_000001001_1_000001110, // -17 _-27 _-24 _-4  _-9  _-14 
        0b0000_1_000010100_1_000011011_1_000001011_0_000000101_0_000010001_0_000001101, // -20 _-27 _-11 _ 5  _ 17 _ 13 
        0b0000_1_000001100_1_000010000_0_000000111_0_000010011_0_000010011_0_000000110, // -12 _-16 _ 7  _ 19 _ 19 _ 6  
        0b0000_1_000011011_1_000010000_0_000011010_0_000110010_0_000110101_0_000010101, // -27 _-16 _ 26 _ 50 _ 53 _ 21 
        0b0000_1_000011110_1_000000001_0_000011000_0_000100101_0_000100101_0_000010111, // -30 _-1  _ 24 _ 37 _ 37 _ 23 
        0b0000_1_000011001_0_000010001_0_000100011_0_000100101_0_001000101_0_000001100, // -25 _ 17 _ 35 _ 37 _ 69 _ 12 
        0b0000_1_000001110_1_000000010_1_000001000_0_000000111_0_000010010_0_000010001, // -14 _-2  _-8  _ 7  _ 18 _ 17 
        0b0000_1_000100100_0_000000001_1_000010100_1_000000010_0_000010110_1_000010111, // -36 _ 1  _-20 _-2  _ 22 _-23 

        0b0000_1_000110001_1_000001001_1_000100100_1_000000110_1_000001101_1_000011011, // -49 _-9  _-36 _-6  _-13 _-27 
        0b0000_1_000000001_1_000011010_1_000011010_0_000001101_0_000000100_1_000000010, // -1  _-26 _-26 _ 13 _ 4  _-2  
        0b0000_1_000011011_1_000001001_1_000001100_0_000001101_0_000010000_1_000000101, // -27 _-9  _-12 _ 13 _ 16 _-5  
        0b0000_1_000100111_1_000001010_1_000000001_0_000011010_0_000001101_0_000001100, // -39 _-10 _-1  _ 26 _ 13 _ 12 
        0b0000_1_000101110_1_000000010_0_000001001_0_000100010_0_000011100_0_000010001, // -46 _-2  _ 9  _ 34 _ 28 _ 17 
        0b0000_1_000101100_1_000000100_1_000000111_0_000001100_0_000010011_0_000000110, // -44 _-4  _-7  _ 12 _ 19 _ 6  
        0b0000_1_000100001_0_000000011_0_000000110_0_000001010_0_000010101_0_000001010, // -33 _ 3  _ 6  _ 10 _ 21 _ 10 
        0b0000_1_000110011_1_000000011_1_000010111_0_000000100_1_000001000_1_000011001, // -51 _-3  _-23 _ 4  _-8  _-25 

        0b0000_1_000001110_1_000001110_1_000101101_0_000000000_1_000010111_1_000011010, // -14 _-14 _-45 _ 0  _-23 _-26 
        0b0000_1_000001110_0_000000010_1_000011001_0_000001111_1_000001001_1_000000100, // -14 _ 2  _-25 _ 15 _-9  _-4  
        0b0000_1_000010110_1_000001011_1_000010000_0_000001111_0_000001100_1_000000100, // -22 _-11 _-16 _ 15 _ 12 _-4  
        0b0000_1_000101110_1_000000010_1_000010001_0_000001111_0_000001010_1_000001010, // -46 _-2  _-17 _ 15 _ 10 _-10 
        0b0000_1_000101100_1_000000101_0_000000011_0_000001110_0_000010011_0_000000011, // -44 _-5  _ 3  _ 14 _ 19 _ 3  
        0b0000_1_000011110_0_000000010_0_000000000_0_000011011_0_000010001_0_000000011, // -30 _ 2  _ 0  _ 27 _ 17 _ 3  
        0b0000_1_000001111_0_000001110_1_000000101_0_000010010_0_000011001_0_000100001, // -15 _ 14 _-5  _ 18 _ 25 _ 33 
        0b0000_1_000011011_0_000000101_1_000100001_0_000001010_1_000010000_1_000001100, // -27 _ 5  _-33 _ 10 _-16 _-12 

        0b0000_0_000000001_1_000100011_1_000101100_0_000000100_1_000011101_1_000100011, //  1  _-35 _-44 _ 4  _-29 _-35 
        0b0000_0_000000111_1_000001000_1_000010000_0_000001111_1_000110101_1_000000001, //  7  _-8  _-16 _ 15 _-53 _-1  
        0b0000_1_000001000_0_000001011_1_000010100_0_000010000_1_000001100_1_000010100, // -8  _ 11 _-20 _ 16 _-12 _-20 
        0b0000_1_001000000_0_000000010_1_000001001_0_000000000_1_000000011_1_000010111, // -64 _ 2  _-9  _ 0  _-3  _-23 
        0b0000_1_000101011_0_000001000_1_000000001_0_000000111_1_000000001_1_000001111, // -43 _ 8  _-1  _ 7  _-1  _-15 
        0b0000_1_000010000_0_000001111_0_000001011_0_000010101_0_000010010_0_000011000, // -16 _ 15 _ 11 _ 21 _ 18 _ 24 
        0b0000_0_000001001_1_000000011_1_000000110_0_000100001_1_000001110_0_000100110, //  9  _-3  _-6  _ 33 _-14 _ 38 
        0b0000_0_000001000_0_000000001_1_001000111_0_000000001_1_000010011_1_000010110, //  8  _ 1  _-71 _ 1  _-19 _-22 

        0b0000_1_000001111_1_000000001_1_000010011_1_000100001_1_001101001_0_000000000, // -15 _-1  _-19 _-33 _-105_ 0  
        0b0000_0_000100100_1_000010010_1_000001101_1_000000011_1_000010101_0_000000000, //  36 _-18 _-13 _-3  _-21 _ 0  
        0b0000_0_000001100_1_000001001_0_000000001_1_000001110_1_000111010_0_000000000, //  12 _-9  _ 1  _-14 _-58 _ 0  
        0b0000_1_000110110_0_000001010_0_000010001_1_000010101_1_000100001_0_000000000, // -54 _ 10 _ 17 _-21 _-33 _ 0  
        0b0000_0_000001000_1_000001111_0_000010000_1_000001101_1_000010001_0_000000000, //  8  _-15 _ 16 _-13 _-17 _ 0  
        0b0000_1_000011100_1_000011001_0_000000111_1_000001100_1_000011100_0_000000000, // -28 _-25 _ 7  _-12 _-28 _ 0  
        0b0000_0_000011000_1_000011111_1_000100101_1_000100111_1_000010011_0_000000000, //  24 _-31 _-37 _-39 _-19 _ 0  
        0b0000_0_000001110_1_000110010_1_000011010_1_000010101_1_000010111_0_000000000, //  14 _-50 _-26 _-21 _-23 _ 0  
    };

    ulong[] eg_pst_enc = {
        0b0000_1_001001010_1_000001001_0_000001101_1_000001110_1_001001010_0_000000000, // -74 _-9  _ 13 _-14 _-74 _ 0  
        0b0000_1_000100011_0_000010110_0_000001010_1_000010101_1_000100011_0_000000000, // -35 _ 22 _ 10 _-21 _-35 _ 0  
        0b0000_1_000010010_0_000010110_0_000010010_1_000001011_1_000010010_0_000000000, // -18 _ 22 _ 18 _-11 _-18 _ 0  
        0b0000_1_000010010_0_000011011_0_000001111_1_000001000_1_000010010_0_000000000, // -18 _ 27 _ 15 _-8  _-18 _ 0  
        0b0000_1_000001011_0_000011011_0_000001100_1_000000111_1_000001011_0_000000000, // -11 _ 27 _ 12 _-7  _-11 _ 0  
        0b0000_0_000001111_0_000010011_0_000001100_1_000001001_0_000001111_0_000000000, //  15 _ 19 _ 12 _-9  _ 15 _ 0  
        0b0000_0_000000100_0_000001010_0_000001000_1_000010001_0_000000100_0_000000000, //  4  _ 10 _ 8  _-17 _ 4  _ 0  
        0b0000_1_000010001_0_000010100_0_000000101_1_000011000_1_000010001_0_000000000, // -17 _ 20 _ 5  _-24 _-17 _ 0  

        0b0000_1_000001100_1_000010001_0_000001011_1_000001000_1_000001100_0_010110010, // -12 _-17 _ 11 _-8  _-12 _ 178
        0b0000_0_000010001_0_000010100_0_000001101_1_000000100_0_000010001_0_010101101, //  17 _ 20 _ 13 _-4  _ 17 _ 173
        0b0000_0_000001110_0_000100000_0_000001101_0_000000111_0_000001110_0_010011110, //  14 _ 32 _ 13 _ 7  _ 14 _ 158
        0b0000_0_000010001_0_000101001_0_000001011_1_000001100_0_000010001_0_010000110, //  17 _ 41 _ 11 _-12 _ 17 _ 134
        0b0000_0_000010001_0_000111010_1_000000011_1_000000011_0_000010001_0_010010011, //  17 _ 58 _-3  _-3  _ 17 _ 147
        0b0000_0_000100110_0_000011001_0_000000011_1_000001101_0_000100110_0_010000100, //  38 _ 25 _ 3  _-13 _ 38 _ 132
        0b0000_0_000010111_0_000011110_0_000001000_1_000000100_0_000010111_0_010100101, //  23 _ 30 _ 8  _-4  _ 23 _ 165
        0b0000_0_000001011_0_000000000_0_000000011_1_000001110_0_000001011_0_010111011, //  11 _ 0  _ 3  _-14 _ 11 _ 187

        0b0000_0_000001010_1_000010100_0_000000111_0_000000010_0_000001010_0_001011110, //  10 _-20 _ 7  _ 2  _ 10 _ 94 
        0b0000_0_000010001_0_000000110_0_000000111_1_000001000_0_000010001_0_001100100, //  17 _ 6  _ 7  _-8  _ 17 _ 100
        0b0000_0_000010111_0_000001001_0_000000111_0_000000000_0_000010111_0_001010101, //  23 _ 9  _ 7  _ 0  _ 23 _ 85 
        0b0000_0_000001111_0_000110001_0_000000101_1_000000001_0_000001111_0_001000011, //  15 _ 49 _ 5  _-1  _ 15 _ 67 
        0b0000_0_000010100_0_000101111_0_000000100_1_000000010_0_000010100_0_000111000, //  20 _ 47 _ 4  _-2  _ 20 _ 56 
        0b0000_0_000101101_0_000100011_1_000000011_0_000000110_0_000101101_0_000110101, //  45 _ 35 _-3  _ 6  _ 45 _ 53 
        0b0000_0_000101100_0_000010011_1_000000101_0_000000000_0_000101100_0_001010010, //  44 _ 19 _-5  _ 0  _ 44 _ 82 
        0b0000_0_000001101_0_000001001_1_000000011_0_000000100_0_000001101_0_001010100, //  13 _ 9  _-3  _ 4  _ 13 _ 84 

        0b0000_1_000001000_0_000000011_0_000000100_1_000000011_1_000001000_0_000100000, // -8  _ 3  _ 4  _-3  _-8  _ 32 
        0b0000_0_000010110_0_000010110_0_000000011_0_000001001_0_000010110_0_000011000, //  22 _ 22 _ 3  _ 9  _ 22 _ 24 
        0b0000_0_000011000_0_000011000_0_000001101_0_000001100_0_000011000_0_000001101, //  24 _ 24 _ 13 _ 12 _ 24 _ 13 
        0b0000_0_000011011_0_000101101_0_000000001_0_000001001_0_000011011_0_000000101, //  27 _ 45 _ 1  _ 9  _ 27 _ 5  
        0b0000_0_000011010_0_000111001_0_000000010_0_000001110_0_000011010_1_000000010, //  26 _ 57 _ 2  _ 14 _ 26 _-2  
        0b0000_0_000100001_0_000101000_0_000000001_0_000001010_0_000100001_0_000000100, //  33 _ 40 _ 1  _ 10 _ 33 _ 4  
        0b0000_0_000011010_0_000111001_1_000000001_0_000000011_0_000011010_0_000010001, //  26 _ 57 _-1  _ 3  _ 26 _ 17 
        0b0000_0_000000011_0_000100100_0_000000010_0_000000010_0_000000011_0_000010001, //  3  _ 36 _ 2  _ 2  _ 3  _ 17 

        0b0000_1_000010010_1_000010010_0_000000011_1_000000110_1_000010010_0_000001101, // -18 _-18 _ 3  _-6  _-18 _ 13 
        0b0000_1_000000100_0_000011100_0_000000101_0_000000011_1_000000100_0_000001001, // -4  _ 28 _ 5  _ 3  _-4  _ 9  
        0b0000_0_000010101_0_000010011_0_000001000_0_000001101_0_000010101_1_000000011, //  21 _ 19 _ 8  _ 13 _ 21 _-3  
        0b0000_0_000011000_0_000101111_0_000000100_0_000010011_0_000011000_1_000000111, //  24 _ 47 _ 4  _ 19 _ 24 _-7  
        0b0000_0_000011011_0_000011111_1_000000101_0_000000111_0_000011011_1_000000111, //  27 _ 31 _-5  _ 7  _ 27 _-7  
        0b0000_0_000010111_0_000100010_1_000000110_0_000001010_0_000010111_1_000001000, //  23 _ 34 _-6  _ 10 _ 23 _-8  
        0b0000_0_000001001_0_000100111_1_000001000_1_000000011_0_000001001_0_000000011, //  9  _ 39 _-8  _-3  _ 9  _ 3  
        0b0000_1_000001011_0_000010111_1_000001011_1_000001001_1_000001011_1_000000001, // -11 _ 23 _-11 _-9  _-11 _-1  

        0b0000_1_000010011_1_000010000_1_000000100_1_000001100_1_000010011_0_000000100, // -19 _-16 _-4  _-12 _-19 _ 4  
        0b0000_1_000000011_1_000011011_0_000000000_1_000000011_1_000000011_0_000000111, // -3  _-27 _ 0  _-3  _-3  _ 7  
        0b0000_0_000001011_0_000001111_1_000000101_0_000001000_0_000001011_1_000000110, //  11 _ 15 _-5  _ 8  _ 11 _-6  
        0b0000_0_000010101_0_000000110_1_000000001_0_000001010_0_000010101_0_000000001, //  21 _ 6  _-1  _ 10 _ 21 _ 1  
        0b0000_0_000010111_0_000001001_1_000000111_0_000001101_0_000010111_0_000000000, //  23 _ 9  _-7  _ 13 _ 23 _ 0  
        0b0000_0_000010000_0_000010001_1_000001100_0_000000011_0_000010000_1_000000101, //  16 _ 17 _-12 _ 3  _ 16 _-5  
        0b0000_0_000000111_0_000001010_1_000001000_1_000000111_0_000000111_1_000000001, //  7  _ 10 _-8  _-7  _ 7  _-1  
        0b0000_1_000001001_0_000000101_1_000010000_1_000001111_1_000001001_1_000001000, // -9  _ 5  _-16 _-15 _-9  _-8  

        0b0000_1_000011011_1_000010110_1_000000110_1_000001110_1_000011011_0_000001101, // -27 _-22 _-6  _-14 _-27 _ 13 
        0b0000_1_000001011_1_000010111_1_000000110_1_000010010_1_000001011_0_000001000, // -11 _-23 _-6  _-18 _-11 _ 8  
        0b0000_0_000000100_1_000011110_0_000000000_1_000000111_0_000000100_0_000001000, //  4  _-30 _ 0  _-7  _ 4  _ 8  
        0b0000_0_000001101_1_000010000_0_000000010_1_000000001_0_000001101_0_000001010, //  13 _-16 _ 2  _-1  _ 13 _ 10 
        0b0000_0_000001110_1_000010000_1_000001001_0_000000100_0_000001110_0_000001101, //  14 _-16 _-9  _ 4  _ 14 _ 13 
        0b0000_0_000000100_1_000010111_1_000001001_1_000001001_0_000000100_0_000000000, //  4  _-23 _-9  _-9  _ 4  _ 0  
        0b0000_1_000000101_1_000100100_1_000001011_1_000001111_1_000000101_0_000000010, // -5  _-36 _-11 _-15 _-5  _ 2  
        0b0000_1_000010001_1_000100000_1_000000011_1_000011011_1_000010001_1_000000111, // -17 _-32 _-3  _-27 _-17 _-7  

        0b0000_1_000110101_1_000100001_1_000001001_1_000010111_1_000110101_0_000000000, // -53 _-33 _-9  _-23 _-53 _ 0  
        0b0000_1_000100010_1_000011100_0_000000010_1_000001001_1_000100010_0_000000000, // -34 _-28 _ 2  _-9  _-34 _ 0  
        0b0000_1_000010101_1_000010110_0_000000011_1_000010111_1_000010101_0_000000000, // -21 _-22 _ 3  _-23 _-21 _ 0  
        0b0000_1_000001011_1_000101011_1_000000001_1_000000101_1_000001011_0_000000000, // -11 _-43 _-1  _-5  _-11 _ 0  
        0b0000_1_000011100_1_000000101_1_000000101_1_000001001_1_000011100_0_000000000, // -28 _-5  _-5  _-9  _-28 _ 0  
        0b0000_1_000001110_1_000100000_1_000001101_1_000010000_1_000001110_0_000000000, // -14 _-32 _-13 _-16 _-14 _ 0  
        0b0000_1_000011000_1_000010100_0_000000100_1_000000101_1_000011000_0_000000000, // -24 _-20 _ 4  _-5  _-24 _ 0  
        0b0000_1_000101011_1_000101001_1_000010100_1_000010001_1_000101011_0_000000000, // -43 _-41 _-20 _-17 _-43 _ 0 
    };

    int[] mg_base_piece_values = { 0, 82, 337, 365, 477, 1025, 0 };
    int[] eg_base_piece_values = { 0, 94, 281, 297, 512, 936, 0 };

    int[,,] mg_pst = new int[2, 7, 64];
    int[,,] eg_pst = new int[2, 7, 64];

    int[] phase_increment = { 0, 0, 1, 1, 2, 4, 0 };

    Board Board;
    Move BestMoveThisIteration;
    Move BestMove;
    int SearchDepth;
    //TODO: is this needed for iterative deepening?
    //int BestEval;
    bool SearchAborted;

    public MyBot()
    {
        //init PSTs
        for (int pt = 1; pt < 7; pt++)
        {
            for (int sq = 0; sq < 64; sq++)
            {
                //White
                mg_pst[0, pt, sq] = mg_base_piece_values[pt] + DecodePstVal(mg_pst_enc[sq ^ 56], pt);
                eg_pst[0, pt, sq] = eg_base_piece_values[pt] + DecodePstVal(eg_pst_enc[sq ^ 56], pt);
                //Black
                mg_pst[1, pt, sq] = mg_base_piece_values[pt] + DecodePstVal(mg_pst_enc[sq], pt);
                eg_pst[1, pt, sq] = eg_base_piece_values[pt] + DecodePstVal(eg_pst_enc[sq], pt);
            }
        }

    }

    //TODO: aspiration windows
    public Move Think(Board board, Timer timer)
    {
        Board = board;
        //TODO: abort search
        SearchAborted = false;

        // DrawBoard(); //#DEBUG
        //FIXME: the idea is to cancel the search and allow much greater depths
        for (int idDepth = 1; idDepth < 6; idDepth++)
        {
            SearchDepth = idDepth;
            Search(SearchDepth, -99999999, 99999999);
            BestMove = BestMoveThisIteration;
            BestMoveThisIteration = Move.NullMove;
            //TODO: if we track best eval, we can detect mate and exit at a lower depth
        }

        return BestMove;
    }

    // Negamax + quiesence in 1
    //TODO: LMR (requiring history)
    private int Search(int depth, int alpha, int beta)
    {

        ulong zobrist = Board.ZobristKey;
        //TODO: potentially this implementation takes more tokens than required. Look up a negamax impl. Possibly also want fail soft?
        int eval = -99999999;
        if (depth < 1)
        {
            eval = Evaluate();
            if (eval >= beta) return beta;
            alpha = Math.Max(alpha, eval);
        }
        else
        {
            if (Board.IsDraw()) return 0;
            if (Board.IsInCheckmate()) return -99999900 - depth;

            (ulong, byte, int, byte, Move) ttEntry = TT[zobrist % 1_000_000];
            byte ttFlag = ttEntry.Item4;
            if (ttFlag != 0 && ttEntry.Item1 == zobrist && ttEntry.Item2 >= depth)
            {
                int ttEval = ttEntry.Item3;

                //Note this is fail-soft AB, but main negamax algo is fail-hard
                if (ttFlag == 1 || ttFlag == 2 && ttEval >= beta || ttFlag == 3 && ttEval <= alpha)
                {
                    if (SearchDepth == depth) BestMoveThisIteration = ttEntry.Item5; //Note: could be null
                    return ttEval;
                }
            }
        }

        byte flag = 3;
        Move bestMoveInPos = Move.NullMove;

        foreach (Move move in Board.GetLegalMoves(depth < 1)
            .OrderByDescending(m =>
            {
                //TODO: move ordering - hash move / prev iteration, good captures, promotions, killers, bad captures, castles, history 
                int score = 0;
                if (m.IsCapture) score += 10 * mg_base_piece_values[(int)m.CapturePieceType] - mg_base_piece_values[(int)m.MovePieceType];
                if (m.IsPromotion) score += mg_base_piece_values[(int)m.PromotionPieceType];
                if (m.IsCastles) score += 300;
                return score;
            }))
        {
            Board.MakeMove(move);
            eval = -Search(depth - 1, -beta, -alpha);
            Board.UndoMove(move);

            if (eval >= beta)
            {
                TT[zobrist % 1_000_000] = (zobrist, (byte)depth, beta, 2, move);
                return beta;
            }
            if (eval > alpha)
            {
                //New best move in position
                //store an exact in tt
                flag = 1;
                bestMoveInPos = move;

                alpha = eval;

                if (SearchDepth == depth) BestMoveThisIteration = move;
            }
        }

        TT[zobrist % 1_000_000] = (zobrist, (byte)depth, eval, flag, bestMoveInPos);

        return alpha;
    }

    //TODO: called once so more token efficient to inline this
    private int Evaluate()
    {
        int phase = 0;
        int[] mg = new int[2];
        int[] eg = new int[2];

        for (int sq = 0; sq < 64; sq++)
        {
            Piece piece = Board.GetPiece(new(sq));
            if (!piece.IsNull)
            {
                int colour = piece.IsWhite ? 0 : 1;
                int pt = (int)piece.PieceType;
                mg[colour] += mg_pst[colour, pt, sq];
                eg[colour] += eg_pst[colour, pt, sq];
                phase += phase_increment[pt];
            }
        }

        phase = Math.Min(phase, 24);

        int sideToMove = Board.IsWhiteToMove ? 0 : 1;

        return ((mg[sideToMove] - mg[sideToMove ^ 1]) * phase + (eg[sideToMove] - eg[sideToMove ^ 1]) * (24 - phase)) / 24;
    }

    private int DecodePstVal(ulong pstVal, int pt)
    {
        ulong shiftedPstVal = pstVal >> (10 * pt - 10);
        return (int)(shiftedPstVal & 0b0000_0000000000_0000000000_0000000000_0000000000_0000000000_0111111111)
            * ((shiftedPstVal & 0b0000_0000000000_0000000000_0000000000_0000000000_0000000000_1000000000) > 0 ? -1 : 1);
    }

    private void DrawBoard() //#DEBUG
    { //#DEBUG
        for (int sq = 0; sq < 64; sq++) //#DEBUG
        { //#DEBUG
            Square square = new(sq); //#DEBUG
            Piece piece = Board.GetPiece(square); //#DEBUG

            bool isNull = piece.IsNull; //#DEBUG
            string pieceDesc = piece.ToString(); //#DEBUG
            if (!isNull) pieceDesc += ", pstVal: " + (mg_pst[piece.IsWhite ? 0 : 1, (int)piece.PieceType, sq] - mg_base_piece_values[(int)piece.PieceType]); //#DEBUG

            Console.WriteLine(square.Name + ": " + pieceDesc); //#DEBUG
        } //#DEBUG
    } //#DEBUG
}