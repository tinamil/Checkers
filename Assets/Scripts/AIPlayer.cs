using UnityEngine;
using System.ComponentModel;
using System.Collections.Generic;

public class AIPlayer : Player {

    AlphaBetaTree tree = null;
    private int treeDepth;

    new public bool mouseControlled = false;

    public AIPlayer(int depth) {
        this.treeDepth = depth;
    }

    // Update is called once per frame
    override internal void DoUpdate() {
        if(Checkers.instance.liveState.currentPlayer == this && tree != null && !tree.Update()) {
            return;
        } else if(Checkers.instance.liveState.currentPlayer == this && tree != null && tree.Update()) {
            Debug.Assert(tree.bestMove.HasValue);
            Checkers.instance.MovePiece(tree.bestMove.Value.piece, tree.bestMove.Value.target);
            tree = null;
        } else if(Checkers.instance.liveState.currentPlayer == this && tree == null) {
            tree = new AlphaBetaTree(treeDepth, Checkers.instance.liveState);
            tree.Start();
            //tree.ThreadFunction();
        }
    }


    private class AlphaBetaTree : ThreadedJob {
        int maxDepth;
        Player aiPlayer;
        CheckerState startingState;
        internal Move? bestMove;


        internal struct Move {
            public Piece piece { get; private set; }
            public Square target { get; private set; }

            public Move(Piece p, Square t) {
                this.piece = p;
                this.target = t;
            }
        }

        internal AlphaBetaTree(int depth, CheckerState startingState) {
            this.maxDepth = depth;
            this.aiPlayer = startingState.currentPlayer;
            this.startingState = startingState;
        }

        internal override void ThreadFunction() {
            FindOptimalChoice(0, startingState, out bestMove);
            //Debug.Log("Best move was value " + value + " moving from [" + bestMove.Value.piece.square.row + ", " + bestMove.Value.piece.square.col + "] to [" + bestMove.Value.target.row + ", " + bestMove.Value.target.col + "]");
        }

        static private bool MoveTheoretically(nPiece piece, Square target, CheckerState currentState, out bool completedTurn) {
            Checkers.MoveType type;
            completedTurn = false;
            if(Checkers.IsValidMove(piece, target, currentState, out type)) {
                switch(type) {

                    case Checkers.MoveType.basic:
                        currentState.MovePiece(piece, target, false);
                        completedTurn = true;
                        break;

                    case Checkers.MoveType.jump:
                        JumpTheoretically(piece, target, currentState, out completedTurn);
                        break;

                }
                return true;
            }
            return false;
        }

        static private void JumpTheoretically(nPiece piece, Square target, CheckerState currentState, out bool completedTurn) {
            piece.pendingFinishJump = false;
            nPiece jumpedPiece = Checkers.GetPlayerPiece(currentState.otherPlayer, Checkers.GetJumpedSquare(piece, target, currentState), currentState);
            //otherPlayer.pieces.Remove(jumpedPiece);
            currentState.MovePiece(piece, target, false);
            currentState.RemovePiece(jumpedPiece, false);
            List<Square> potentialFollowJump = new List<Square>();
            foreach(Square adjacentJump in Checkers.GetSquares(target.row, target.col, currentState, 2)) {
                if(Checkers.IsJump(piece, adjacentJump, currentState)) {
                    potentialFollowJump.Add(adjacentJump);
                }
            }
            if(potentialFollowJump.Count == 1) {
                JumpTheoretically(piece, potentialFollowJump[0], currentState, out completedTurn);
            } else if(potentialFollowJump.Count == 0) {
                completedTurn = true;
            } else {
                piece.pendingFinishJump = true;
                completedTurn = false;
            }
        }

        private int FindOptimalChoice(int depth, CheckerState state, out Move? bestMove) {
            bestMove = null;
            if(depth > this.maxDepth) {
                return EvaluateState(state);
            }
            int bestValue = int.MinValue;
            List<Move> bestMoves = new List<Move>();
            foreach(nPiece targetPiece in state.pieceMap) {
                if(targetPiece == null || targetPiece.owner != state.currentPlayer) continue;
                foreach(Square targetSquare in Checkers.GetSquares(targetPiece.row, targetPiece.col, state, 1, 2)) {
                    CheckerState duplicateState = state.DeepCopy();
                    bool done;
                    if(MoveTheoretically(duplicateState.pieceMap[targetPiece.row, targetPiece.col], duplicateState.board.grid[targetSquare.row, targetSquare.col], duplicateState, out done)) {
                        Move? ignored;
                        int value;
                        if(!done) {
                            value = FindOptimalChoice(depth, duplicateState, out ignored);
                        } else {
                            duplicateState.currentPlayer = duplicateState.otherPlayer;
                            value = FindOptimalChoice(depth + 1, duplicateState, out ignored);
                        }
                        if(value > bestValue) {
                            bestMoves.Clear();
                        }
                        if(value >= bestValue) {
                            bestValue = value;
                            bestMoves.Add(new Move(targetPiece.original, targetSquare));
                        }
                    }
                }
            }
            if(bestMoves.Count > 0) {
                bestMove = bestMoves[new System.Random().Next(0, bestMoves.Count)];
            }

            return bestValue;
        }

        int EvaluateState(CheckerState state) {
            int score = 0;
            int count = 0;
            foreach(nPiece p in state.pieceMap) {
                if(p == null) continue;
                if(p.owner == aiPlayer) score += 1;
                else score -= 1;
                count += 1;
            }
            if(score == count) score = int.MaxValue;
            if(score == -count) score = int.MinValue;
            return score;
        }

    }

}
