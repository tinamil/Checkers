﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class nPiece
{
    internal Piece original;

    internal Player owner { get { return original.owner; } set { original.owner = value; } }

    internal Square square;

    internal bool king = false;

    internal bool pendingFinishJump = false;

    internal nPiece(Piece n)
    {
        this.original = n;
        this.square = n.square;
        this.king = n.king;
        this.pendingFinishJump = n.pendingFinishJump;
    }

    internal nPiece(nPiece n)
    {
        this.original = n.original;
        this.square = n.square;
        this.king = n.king;
        this.pendingFinishJump = n.pendingFinishJump;
    }
}

public class Checkers : MonoBehaviour {

    public GameObject boardObject;
    public GameObject pieceObject;

    internal Piece draggedPiece;

    internal Board _board;

    internal Player currentPlayer;

    private Player player1;
    private Player player2;

    public static readonly int rows = 8;
    public static readonly int cols = 8;

    [HideInInspector]
    public static Checkers instance = null;

    public enum MoveType { invalid, basic, jump };

    internal nPiece[,] pieceMap = new nPiece[rows, cols];

    // Use this for initialization
    void Awake() {

        if(instance == null) {
            instance = this;
        } else if(instance != this) {
            Destroy(gameObject);
        }

        player1 = new Player();
        player1.unitDirection = 1;
        player2 = new AIPlayer();
        player2.unitDirection = -1;

        currentPlayer = player1;

        _board = boardObject.GetComponent<Board>();
        for(int row = 0; row < rows; ++row) {
            for(int col = 0; col < cols; ++col) {
                if((row <= 2 || row >= 5) && (row + col) % 2 == 1) {
                    GameObject piece = Instantiate(pieceObject, _board.grid[row, col].GetComponent<Collider>().bounds.center, Quaternion.identity) as GameObject;
                    Piece pieceScript = piece.GetComponent<Piece>();
                    pieceScript.square = _board.grid[row, col];

                    pieceMap[row, col] = new nPiece(pieceScript);
                    if(row <= 2) {
                        //player1.pieces.Add(pieceScript);
                        pieceScript.owner = player1;
                        piece.GetComponent<MeshRenderer>().material = _board.squares[0];
                    } else {
                        //player2.pieces.Add(pieceScript);
                        pieceScript.owner = player2;
                        piece.GetComponent<MeshRenderer>().material = _board.squares[1];
                        piece.GetComponent<MeshRenderer>().material.color = Color.gray;
                    }
                }
            }
        }
    }


    void Update() {
        player1.DoUpdate();
        player2.DoUpdate();
    }

    public bool IsValidMove(nPiece piece, Square target, nPiece[,] currentState) {
        MoveType ignore;
        return IsValidMove(currentPlayer, new nPiece(piece), target, currentState, out ignore);
    }

    public bool IsMovablePiece(nPiece piece, nPiece[,] currentState) {
        if(piece.square == null) return false;
        foreach(Square s in GetSquares(piece.square, 1)){ 
            if(IsValidMove(piece, s, currentState)) return true;
        }
        foreach(Square s in GetSquares(piece.square, 2)){
            if(IsValidMove(piece, s, currentState)) return true;
        }
        return false;
    }

    internal bool IsValidMove(Player player, nPiece piece, Square target, nPiece[,] currentState, out MoveType type) {
        type = MoveType.invalid;
        if(piece.owner != player) return false;
        if(IsSquareOccupied(target, currentState)) return false;
        if(IsPendingJumpChoice(player, currentState) && piece.pendingFinishJump == false) return false;
        if(IsJump(player, piece, target, currentState)) {
            type = MoveType.jump;
            return true;
        }
        if(IsJumpAvailable(player, currentState)) return false;
        if(!IsMoveAdjacent(piece, target)) return false;
        if(piece.king == false && !IsMoveForward(player, piece, target)) return false;
        type = MoveType.basic;
        return true;
    }

    bool IsCurrentPlayerPiece(nPiece piece) {
        return piece.owner == currentPlayer;
    }

    internal bool IsPendingJumpChoice(Player player, nPiece[,] currentState) {
        foreach(nPiece piece in currentState) {
            if(piece != null && piece.owner == player && piece.pendingFinishJump == true) return true;
        }
        return false;
    }

    public IList<Square> GetSquares(Square square, params int[] distances) {
        IList<Square> squareList = new List<Square>();
        foreach(int distance in distances) {
            int min = -distance;
            int max = distance;
            int skip = 2 * distance;
            for(int row = min; row <= max; row += skip) {
                int currentRow = square.row + row;
                if(currentRow >= 0 && currentRow < rows) {
                    for(int col = min; col <= max; col += skip) {
                        int currentCol = square.col + col;
                        if(currentCol >= 0 && currentCol < cols) {
                            squareList.Add(_board.grid[currentRow, currentCol]);
                        }
                    }
                }
            }
        }
        return squareList;
    }

    public void MovePiece(Piece piece, Square square) {
        float delay;
        if(MovePiece(currentPlayer, OtherPlayer(currentPlayer), new nPiece(piece), square, pieceMap, out delay)) {
            StartCoroutine(SwitchPlayer(OtherPlayer(currentPlayer), delay));
            currentPlayer = null;
        }
    }

    IEnumerator SwitchPlayer(Player otherPlayer, float delay) {
        bool continueGame = false;
        foreach(nPiece p in pieceMap) {
            if(p != null && p.owner == otherPlayer) {
                continueGame = true;
                break;
            }
        }
        if(continueGame) {
            yield return new WaitForSeconds(delay);
            currentPlayer = otherPlayer;
        }
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

    internal bool IsJump(Player player, nPiece piece, Square target, nPiece[,] currentState) {
        if(IsSquareOccupied(target, currentState)) return false;

        if(Mathf.Abs(target.row - piece.square.row) != 2 || Mathf.Abs(target.col - piece.square.col) != 2) return false;

        Player otherPlayer = OtherPlayer(player);

        return IsPlayerPiecePresent(otherPlayer, GetJumpedSquare(piece, target), currentState) && (IsMoveForward(player, piece, GetJumpedSquare(piece, target)) || piece.king);
    }

    internal Square GetJumpedSquare(nPiece piece, Square final) {
        return _board.grid[(final.row + piece.square.row) / 2, (final.col + piece.square.col) / 2];
    }

    bool IsSquareOccupied(Square target, nPiece[,] currentState) {
        return (IsPlayerPiecePresent(player1, target, currentState) || IsPlayerPiecePresent(player2, target, currentState));
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

    internal nPiece GetPlayerPiece(Player player, Square target, nPiece[,] currentState) {
        nPiece piece = currentState[target.row, target.col];
        if(piece == null) return null;
        if(player == piece.owner) return piece;
        return null;
    }

    bool JumpPiece(Player player, Player otherPlayer, nPiece piece, Square target, nPiece[,] currentState, ref float delay) {
        piece.pendingFinishJump = false;
        nPiece jumpedPiece = GetPlayerPiece(OtherPlayer(player), GetJumpedSquare(piece, target), currentState);
        Vector3 offBoard = new Vector3(player.unitDirection, 0);
        float hangtime = Piece.CalculateFlightTime(piece.original.GetComponent<Collider>().bounds.center, target.GetComponent<Collider>().bounds.center);
        float otherHangtime = Piece.CalculateFlightTime(jumpedPiece.original.GetComponent<Collider>().bounds.center, offBoard);
        StartCoroutine(DelayedFlipDestroy(jumpedPiece.original, offBoard, delay + hangtime));
        StartCoroutine(piece.original.FlipToTarget(target, delay));
        delay = delay + hangtime + otherHangtime;
        currentState[jumpedPiece.square.row, jumpedPiece.square.col] = null;
        jumpedPiece.original.square = null;
        jumpedPiece.square = null;
        currentState[piece.square.row, piece.square.col] = null;
        piece.square = target;
        piece.original.square = target;
        currentState[piece.square.row, piece.square.col] = piece;
        List<Square> potentialFollowJump = new List<Square>();
        foreach(Square adjacentJump in GetSquares(target, 2)) {
            if(IsJump(player, piece, adjacentJump, currentState)) {
                potentialFollowJump.Add(adjacentJump);
            }
        }
        if(potentialFollowJump.Count == 1) {
            return JumpPiece(player, otherPlayer, piece, potentialFollowJump[0], currentState, ref delay);
        } else if(potentialFollowJump.Count == 0) {
            if(IsKingRow(player, piece)) {
                piece.king = true;
            }
            return true;
        } else {
            piece.pendingFinishJump = true;
            return false;
        }
    }

    bool MovePiece(Player player, Player otherPlayer, nPiece piece, Square target, nPiece[,] currentState, out float delay) {
        MoveType type;
        if(IsValidMove(player, piece, target, currentState, out type)) {
            switch(type) {

                case MoveType.basic:
                    currentState[piece.square.row, piece.square.col] = null;
                    delay = piece.original.FlipToTarget(target);
                    piece.square = target;
                    piece.original.square = target;
                    currentState[piece.square.row, piece.square.col] = piece;
                    if(IsKingRow(player, piece)) {
                        piece.original.king = true;
                        piece.king = true;
                    }
                    return true;

                case MoveType.jump:
                    delay = 0f;
                    return JumpPiece(player, otherPlayer, piece, target, currentState, ref delay);
                    
            }
        }
        delay = 0f;
        return false;
    }

    IEnumerator DelayedFlipDestroy(Piece piece, Vector3 target, float delay) {
        yield return new WaitForSeconds(delay);
        float hangtime = piece.FlipToTarget(target);
        //piece.GetComponent<Collider>().isTrigger = true;
        StartCoroutine(DestroyPiece(piece, hangtime));
    }

    IEnumerator DestroyPiece(Piece piece, float delay) {
        yield return new WaitForSeconds(delay);
        //Destroy(piece.gameObject);
    }

    internal bool IsKingRow(Player player, nPiece piece) {
        int targetRow = (player.unitDirection == 1 ? rows - 1 : 0);
        if(piece.square.row == targetRow) {
            return true;
        } else {
            return false;
        }
    }

    public Player OtherPlayer(Player player) {
        if(player == player1) return player2;
        else return player1;
    }
}
