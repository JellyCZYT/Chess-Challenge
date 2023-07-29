using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        public Move Think(Board board, Timer timer)
        {
            return GetBestMove(board);
        }

        int[] pieceTables = { 0, 100, 300, 400, 500, 1000, 0 };

        float evaluate(Board board, Move move)
        {
            // Evaluate INIT
            PieceList[] pieces = board.GetAllPieceLists();
            float eval = 0f;

            for (int i = 0; i < pieces.Length; i++)
            {
                if (i <= 5)
                {
                    eval += pieceTables[i % 6] * pieces[i].Count;
                }
                else
                {
                    eval -= pieceTables[i % 6] * pieces[i].Count;
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
}