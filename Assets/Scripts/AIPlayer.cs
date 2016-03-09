using UnityEngine;
using System.ComponentModel;
using System.Collections.Generic;

public class AIPlayer : Player {

    AlphaBetaTree tree = null;
    public int treeDepth = 3;
    
    new public bool mouseControlled = false;

    // Update is called once per frame
    override internal void DoUpdate() {
        //if(Checkers.instance.currentPlayer == this && tree != null && !tree.Update()) {
        //    return;
        //} else if(Checkers.instance.currentPlayer == this && tree != null && tree.Update()) {
         //   Checkers.instance.MovePiece(tree.bestMove.Value.piece.original, tree.bestMove.Value.target);
        //    tree = null;
        /*} else*/ if(Checkers.instance.currentPlayer == this) {
            tree = new AlphaBetaTree(treeDepth, this, Checkers.instance.OtherPlayer(this), Checkers.instance.pieceMap);
            //tree.Start();
            tree.ThreadFunction();
            Checkers.instance.MovePiece(tree.bestMove.Value.piece.original, tree.bestMove.Value.target);
        }
    }


    private class AlphaBetaTree : ThreadedJob {
        int maxDepth;
        Player aiPlayer;
        Player otherPlayer;
        nPiece[,] startingState;
        internal Move? bestMove;

        
        internal struct Move {
            public nPiece piece { get; private set; }
            public Square target { get; private set; }

            public Move(nPiece p, Square t) {
                this.piece = p;
                this.target = t;
            }
        }

        internal AlphaBetaTree(int depth, Player aiPlayer, Player otherPlayer, nPiece[,] startingState) {
            this.maxDepth = depth;
            this.aiPlayer = aiPlayer;
            this.otherPlayer = otherPlayer;
            this.startingState = startingState;
        }

        internal override void ThreadFunction() {
            nPiece[,] newBoard = new nPiece[Checkers.rows, Checkers.cols];
            foreach(nPiece p in startingState) {
                if(p != null) {
                    nPiece newPiece = new nPiece(p);
                    newBoard[p.square.row, p.square.col] = newPiece;
                }
            }
            int value = FindOptimalChoice(0, aiPlayer, otherPlayer, newBoard, out bestMove);
            Debug.Log("Best move was value " + value + " moving from [" + bestMove.Value.piece.original.square.row + ", " + bestMove.Value.piece.original.square.col + "] to [" + bestMove.Value.target.row + ", " + bestMove.Value.target.col + "]");
        }

        private bool MoveTheoretically(Player currentPlayer, Player otherPlayer, nPiece piece, Square target, nPiece[,] currentState) {
            Checkers.MoveType type;
            if(Checkers.instance.IsValidMove(currentPlayer, piece, target, currentState, out type)) {
                switch(type) {

                    case Checkers.MoveType.basic:
                        currentState[piece.square.row, piece.square.col] = null;
                        piece.square = target;
                        currentState[piece.square.row, piece.square.col] = piece;
                        if(Checkers.instance.IsKingRow(currentPlayer, piece)) {
                            piece.king = true;
                        }
                        return true;

                    case Checkers.MoveType.jump:
                        return JumpTheoretically(currentPlayer, otherPlayer, piece, target, currentState);

                }
            }
            return false;
        }

        private bool JumpTheoretically(Player currentPlayer, Player otherPlayer, nPiece piece, Square target, nPiece[,] currentState) {
            piece.pendingFinishJump = false;
            nPiece jumpedPiece = Checkers.instance.GetPlayerPiece(otherPlayer, Checkers.instance.GetJumpedSquare(piece, target), currentState);
            //otherPlayer.pieces.Remove(jumpedPiece);
            currentState[jumpedPiece.square.row, jumpedPiece.square.col] = null;
            jumpedPiece.square = null;
            currentState[piece.square.row, piece.square.col] = null;
            piece.square = target;
            currentState[piece.square.row, piece.square.col] = piece;
            List<Square> potentialFollowJump = new List<Square>();
            foreach(Square adjacentJump in Checkers.instance.GetSquares(target, 2)) {
                if(Checkers.instance.IsJump(currentPlayer, piece, adjacentJump, currentState)) {
                    potentialFollowJump.Add(adjacentJump);
                }
            }
            if(potentialFollowJump.Count == 1) {
                return JumpTheoretically(currentPlayer, otherPlayer, piece, potentialFollowJump[0], currentState);
            } else if(potentialFollowJump.Count == 0) {
                if(Checkers.instance.IsKingRow(currentPlayer, piece)) {
                    piece.king = true;
                }
                return true;
            } else {
                piece.pendingFinishJump = true; 
                return false;
            }
        }

        private int FindOptimalChoice(int depth, Player player, Player otherPlayer, nPiece[,] grid, out Move? bestMove) {
            bestMove = null;
            if(depth >= this.maxDepth) {
                return EvaluateState(grid);
            }
            int bestValue = int.MinValue;
            List<Move> bestMoves = new List<Move>();
            foreach(nPiece targetPiece in grid) {
                if(targetPiece == null) continue;
                foreach(Square targetSquare in Checkers.instance.GetSquares(targetPiece.square, 1, 2)) {
                    nPiece[,] newBoard = new nPiece[Checkers.rows, Checkers.cols];
                    foreach(nPiece p in grid) {
                        if(p != null) newBoard[p.square.row, p.square.col] = new nPiece(p);
                    }
                    
                    if(MoveTheoretically(player, otherPlayer, targetPiece, targetSquare, newBoard)) {
                        Move? ignored;
                        int value;
                        if(Checkers.instance.IsPendingJumpChoice(player, newBoard)) {
                            value = FindOptimalChoice(depth, player, otherPlayer, newBoard, out ignored);
                        } else { 
                            value = FindOptimalChoice(depth + 1, otherPlayer, player, newBoard, out ignored);
                        }
                        if(value > bestValue) {
                            bestMoves.Clear();
                        }
                        if(value >= bestValue) {
                            bestValue = value;
                            bestMoves.Add(new Move(targetPiece, targetSquare));
                        }
                    }
                }
            }
            if(bestMoves.Count > 0) {
                bestMove = bestMoves[Random.Range(0, bestMoves.Count)];
            }
            
            return bestValue;
        }
        
        int EvaluateState(nPiece[,] pieceGrid) {
            int score = 0;
            foreach(nPiece p in pieceGrid) {
                if(p == null) continue;
                else if(p.owner == aiPlayer) score += 1;
                else score -= 1;
            }
            if(score == pieceGrid.Length) score = int.MaxValue;
            if(score == -pieceGrid.Length) score = int.MinValue;
            return score;
        }

    }

}
