using System;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot {

  Move BestMove;

  public Move Think(Board board, Timer timer) {
    long maxDepth = 19999999998;

    long NegaMax(long alpha, long beta, long depth, long eval) {
      long bestFound = depth;

      var moves = board.GetLegalMoves();
      if (depth == maxDepth)
        return eval - moves.Length + board.FiftyMoveCounter;

      foreach (var move in moves.OrderByDescending(t => t.CapturePieceType)) {
        board.MakeMove(move);
        long subEval = board.IsDraw() ? 
                         0 :
                        -NegaMax(-beta, -alpha, depth - 1,
                                 -eval + (move.IsCastles ? 10000 : 
                                          move.IsPromotion ? 1610612736 : 
                                          move.IsCapture ? 1078477616 << (int)move.CapturePieceType * 1306960869 : 0));
        board.UndoMove(move);

        if (subEval < bestFound) {
          bestFound = subEval;
          if (depth == 20000000000)
            BestMove = move;

          alpha = Math.Min(alpha, bestFound);
          if (alpha <= beta)
            break;
        }
      }

      return bestFound;
    }

    for (; timer.MillisecondsElapsedThisTurn < timer.MillisecondsRemaining / 1000f; maxDepth -= 2)
      NegaMax(30000000000, -30000000000, 20000000000, 0);

    return BestMove;
  }
}