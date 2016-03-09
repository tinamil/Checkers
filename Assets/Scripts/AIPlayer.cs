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
            tree = new AlphaBetaTree(treeDepth, this, Checkers.instance.OtherPlayer(this), Checkers.instance.pieceMap, Checkers.instance._board);
            //tree.Start();
            tree.ThreadFunction();
            Checkers.instance.MovePiece(tree.bestMove.Value.piece.original, tree.bestMove.Value.target);
        }
    }


    private class AlphaBetaTree : ThreadedJob {
        int maxDepth;
        Player aiPlayer;
        Player otherPlayer;
        Piece[,] startingState;
        Board board;
        internal Move? bestMove;

        internal class nPiece {
            internal Piece original;

            internal Player owner { get; set; }

            internal Square square;

            internal bool king = false;

            internal bool pendingFinishJump = false;

            internal nPiece(Piece n) {
                this.owner = n.owner;
                this.original = n;
                this.square = n.square;
                this.king = n.king;
                this.pendingFinishJump = n.pendingFinishJump;
            }

            internal nPiece(nPiece n) {
                this.owner = n.owner;
                this.original = n.original;
                this.square = n.square;
                this.king = n.king;
                this.pendingFinishJump = n.pendingFinishJump;
            }
        }

        internal struct Move {
            public nPiece piece { get; private set; }
            public Square target { get; private set; }

            public Move(nPiece p, Square t) {
                this.piece = p;
                this.target = t;
            }
        }

        internal AlphaBetaTree(int depth, Player aiPlayer, Player otherPlayer, Piece[,] startingState, Board board) {
            this.maxDepth = depth;
            this.aiPlayer = aiPlayer;
            this.otherPlayer = otherPlayer;
            this.startingState = startingState;
            this.board = board;
        }

        internal override void ThreadFunction() {
            nPiece[,] newBoard = new nPiece[Checkers.rows, Checkers.cols];
            foreach(Piece p in startingState) {
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
            if(IsValidMove(currentPlayer, piece, target, currentState, out type)) {
                switch(type) {

                    case Checkers.MoveType.basic:
                        currentState[piece.square.row, piece.square.col] = null;
                        piece.square = target;
                        currentState[piece.square.row, piece.square.col] = piece;
                        if(IsKingRow(currentPlayer, piece)) {
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
            nPiece jumpedPiece = GetPlayerPiece(otherPlayer, GetJumpedSquare(piece, target), currentState);
            //otherPlayer.pieces.Remove(jumpedPiece);
            currentState[jumpedPiece.square.row, jumpedPiece.square.col] = null;
            jumpedPiece.square = null;
            currentState[piece.square.row, piece.square.col] = null;
            piece.square = target;
            currentState[piece.square.row, piece.square.col] = piece;
            List<Square> potentialFollowJump = new List<Square>();
            foreach(Square adjacentJump in GetSquares(target, 2)) {
                if(IsJump(currentPlayer, piece, adjacentJump, currentState)) {
                    potentialFollowJump.Add(adjacentJump);
                }
            }
            if(potentialFollowJump.Count == 1) {
                return JumpTheoretically(currentPlayer, otherPlayer, piece, potentialFollowJump[0], currentState);
            } else if(potentialFollowJump.Count == 0) {
                if(IsKingRow(currentPlayer, piece)) {
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
                foreach(Square targetSquare in GetSquares(targetPiece.square, 1, 2)) {
                    nPiece[,] newBoard = new nPiece[Checkers.rows, Checkers.cols];
                    foreach(nPiece p in grid) {
                        if(p != null) newBoard[p.square.row, p.square.col] = new nPiece(p);
                    }
                    
                    if(MoveTheoretically(player, otherPlayer, targetPiece, targetSquare, newBoard)) {
                        Move? ignored;
                        int value;
                        if(IsPendingJumpChoice(player, newBoard)) {
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

        bool IsValidMove(Player player, nPiece piece, Square target, nPiece[,] currentState, out Checkers.MoveType type) {
            type = Checkers.MoveType.invalid;
            if(piece.owner != player) return false;
            if(IsSquareOccupied(target, currentState)) return false;
            if(IsPendingJumpChoice(player, currentState) && piece.pendingFinishJump == false) return false;
            if(IsJump(player, piece, target, currentState)) {
                type = Checkers.MoveType.jump;
                return true;
            }
            if(IsJumpAvailable(player, currentState)) return false;
            if(!IsMoveAdjacent(piece, target)) return false;
            if(piece.king == false && !IsMoveForward(player, piece, target)) return false;
            type = Checkers.MoveType.basic;
            return true;
        }

        bool IsPendingJumpChoice(Player player, nPiece[,] currentState) {
            foreach(nPiece piece in currentState) {
                if(piece != null && piece.owner == player && piece.pendingFinishJump == true) return true;
            }
            return false;
        }

        IList<Square> GetSquares(Square square, params int[] distances) {
            IList<Square> squareList = new List<Square>();
            foreach(int distance in distances) {
                int min = -distance;
                int max = distance;
                int skip = 2 * distance;
                for(int row = min; row <= max; row += skip) {
                    int currentRow = square.row + row;
                    if(currentRow >= 0 && currentRow < Checkers.rows) {
                        for(int col = min; col <= max; col += skip) {
                            int currentCol = square.col + col;
                            if(currentCol >= 0 && currentCol < Checkers.cols) {
                                squareList.Add(board.grid[currentRow, currentCol]);
                            }
                        }
                    }
                }
            }
            return squareList;
        }

        bool IsJumpAvailable(Player player, nPiece[,] currentState) {
            foreach(nPiece piece in currentState) {
                if(piece == null || piece.owner != player) continue;
                IEnumerable<Square> adjacentSquares = GetSquares(piece.square, 2);
                foreach(Square adj in adjacentSquares) {
                    if(IsJump(player, piece, adj, currentState) && (IsMoveForward(player, piece, adj) || piece.king)) return true;
                }
            }
            return false;
        }

        bool IsJump(Player player, nPiece piece, Square target, nPiece[,] currentState) {
            if(IsSquareOccupied(target, currentState)) return false;

            if(Mathf.Abs(target.row - piece.square.row) != 2 || Mathf.Abs(target.col - piece.square.col) != 2) return false;

            Player otherPlayer = OtherPlayer(player);

            return IsPlayerPiecePresent(otherPlayer, GetJumpedSquare(piece, target), currentState) && (IsMoveForward(player, piece, GetJumpedSquare(piece, target)) || piece.king);
        }

        Square GetJumpedSquare(nPiece piece, Square final) {
            return board.grid[(final.row + piece.square.row) / 2, (final.col + piece.square.col) / 2];
        }

        bool IsSquareOccupied(Square target, nPiece[,] currentState) {
            return (IsPlayerPiecePresent(aiPlayer, target, currentState) || IsPlayerPiecePresent(otherPlayer, target, currentState));
        }

        bool IsMoveForward(Player player, nPiece piece, Square target) {
            return Mathf.Sign(target.row - piece.square.row) == Mathf.Sign(player.unitDirection);
        }

        bool IsMoveAdjacent(nPiece piece, Square target) {
            return Mathf.Abs(target.row - piece.square.row) == 1 && Mathf.Abs(target.col - piece.square.col) == 1;
        }

        bool IsPlayerPiecePresent(Player player, Square target, nPiece[,] currentState) {
            return (GetPlayerPiece(player, target, currentState) != null);
        }

        nPiece GetPlayerPiece(Player player, Square target, nPiece[,] currentState) {
            nPiece piece = currentState[target.row, target.col];
            if(piece == null) return null;
            if(player == piece.owner) return piece;
            return null;
        }

        bool IsKingRow(Player player, nPiece piece) {
            int targetRow = (player.unitDirection == 1 ? Checkers.rows - 1 : 0);
            if(piece.square.row == targetRow) {
                return true;
            } else {
                return false;
            }
        }

        public Player OtherPlayer(Player player) {
            if(player == aiPlayer) return otherPlayer;
            else return aiPlayer;
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
