using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        return GetBestMove(board);
    }


    int[] pieceTables = { 0, 100, 300, 400, 500, 1000, 0 };

    int[,] pawnPST = new int[8, 8]
        {
        {0, 0, 0, 0, 0, 0, 0, 0},
        {50, 50, 50, 50, 50, 50, 50, 50},
        {10, 10, 20, 30, 30, 20, 10, 10},
        {5, 5, 10, 25, 25, 10, 5, 5},
        {0, 0, 0, 20, 20, 0, 0, 0},
        {5, -5, -10, 0, 0, -10, -5, 5},
        {5, 10, 10, -20, -20, 10, 10, 5},
        {0, 0, 0, 0, 0, 0, 0, 0}
        };

    int[,] knightPST = new int[8, 8]
    {
        {-50, -40, -30, -30, -30, -30, -40, -50},
        {-40, -20, 0, 0, 0, 0, -20, -40},
        {-30, 0, 10, 15, 15, 10, 0, -30},
        {-30, 5, 15, 20, 20, 15, 5, -30},
        {-30, 0, 15, 20, 20, 15, 0, -30},
        {-30, 5, 10, 15, 15, 10, 5, -30},
        {-40, -20, 0, 5, 5, 0, -20, -40},
        {-50, -40, -30, -30, -30, -30, -40, -50}
    };

    float evaluate(Board board, Move move)
    {
        // Define PSTs for other piece types (bishop, rook, queen, and king) similarly
        // Evaluate INIT
        PieceList[] pieces = board.GetAllPieceLists();
        float eval = 0f;

        for (int i = 0; i < pieces.Length; i++)
        {
            int pieceValue = pieceTables[i % 6];
            foreach (Piece piece in pieces[i])
            {
                Square square = piece.Square;
                float pst = pieceValue + getPST(square.Rank, square.File, i,piece.IsWhite);
                if (!piece.IsWhite) pst = -pst;
                eval += pst;
            }
        }


        if (board.IsWhiteToMove)
        {
            if (board.IsDraw())
            {
                return 0;
            }
            if (board.IsInCheckmate())
            {
                eval -= 999999;
            }
            if (move.IsCapture || move.IsCastles || move.IsPromotion)
            {
                eval -= 50;
            }
            return eval;
        }
        else
        {
            if (board.TrySkipTurn())
            {
                if (board.IsInCheckmate())
                {
                    eval += 999999;
                }
                if (move.IsCapture || move.IsCastles || move.IsPromotion)
                {
                    eval -= 50;
                }
                board.UndoSkipTurn();
            }
            return eval;
        }
    }

    private float getPST(int rank, int file, int i,bool white)
    {
        if (!white) rank = 7 - rank;
        if(i <= 5) return pawnPST[rank, file];
        else return knightPST[rank, file];
    }

    Move GetBestMove(Board board)
    {
        float bestScore = float.NegativeInfinity;
        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];

        foreach (Move move in moves)
        {
            board.MakeMove(move); // Make the move on the board
            float score = Minimax(board, move, 3, float.NegativeInfinity, float.PositiveInfinity, false);
            board.UndoMove(move); // Undo the move after evaluation

            if (board.IsWhiteToMove)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
        }

        return bestMove;
    }
    float Minimax(Board board, Move move, int depth, float alpha, float beta, bool isMaximizing)
    {
        if (depth == 0)
            return evaluate(board, move);

        Move[] moves = board.GetLegalMoves();

        if (isMaximizing)
        {
            float maxScore = float.NegativeInfinity;
            foreach (Move childMove in moves)
            {
                board.MakeMove(childMove); // Make the move on the board
                float childScore = Minimax(board, childMove, depth - 1, alpha, beta, false);
                board.UndoMove(childMove); // Undo the move after evaluation

                maxScore = Math.Max(maxScore, childScore);
                alpha = Math.Max(alpha, childScore);
                if (beta <= alpha)
                    break;
            }
            return maxScore;
        }
        else
        {
            float minScore = float.PositiveInfinity;
            foreach (Move childMove in moves)
            {
                board.MakeMove(childMove); // Make the move on the board
                float childScore = Minimax(board, childMove, depth - 1, alpha, beta, true);
                board.UndoMove(childMove); // Undo the move after evaluation

                minScore = Math.Min(minScore, childScore);
                beta = Math.Min(beta, childScore);
                if (beta <= alpha)
                    break;
            }
            return minScore;
        }
    }
}