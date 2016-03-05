using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Checkers : MonoBehaviour {

    public GameObject boardObject;
    public GameObject pieceObject;
    public GameObject playerObject;

    internal Piece draggedPiece;

    private Board _board;

    private Player currentPlayer;

    private Player player1;
    private Player player2;

    public static readonly int rows = 8;
    public static readonly int cols = 8;

    [HideInInspector]
    public static Checkers instance = null;

    enum MoveType { invalid, basic, jump };

    // Use this for initialization
    void Awake() {

        if(instance == null) {
            instance = this;
        } else if(instance != this) {
            Destroy(gameObject);
        }

        player1 = (Instantiate(playerObject) as GameObject).GetComponent<Player>();
        player1.unitDirection = 1;
        player2 = (Instantiate(playerObject) as GameObject).GetComponent<Player>();
        player2.unitDirection = -1;

        currentPlayer = player1;

        _board = boardObject.GetComponent<Board>();
        for(int row = 0; row < rows; ++row) {
            for(int col = 0; col < cols; ++col) {
                if((row <= 2 || row >= 5) && (row + col) % 2 == 1) {
                    GameObject piece = Instantiate(pieceObject, _board.grid[row, col].GetComponent<Collider>().bounds.center, Quaternion.identity) as GameObject;
                    Piece pieceScript = piece.GetComponent<Piece>();
                    pieceScript.square = _board.grid[row, col];

                    if(row <= 2) {
                        player1.pieces.Add(pieceScript);
                        piece.GetComponent<MeshRenderer>().material = _board.squares[0];
                    } else {
                        player2.pieces.Add(pieceScript);
                        piece.GetComponent<MeshRenderer>().material = _board.squares[1];
                        piece.GetComponent<MeshRenderer>().material.color = Color.gray;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {

    }

    public bool IsValidMove(Piece piece, Square target) {
        MoveType ignore;
        return IsValidMove(currentPlayer, piece, target, out ignore);
    }

    public bool IsMovablePiece(Piece piece) {
        if(piece.square == null) return false;
        Square currentSquare = piece.square.GetComponent<Square>();
        foreach(Square s in GetSquares(currentSquare, 1)){ 
            if(IsValidMove(piece, s)) return true;
        }
        foreach(Square s in GetSquares(currentSquare, 2)){
            if(IsValidMove(piece, s)) return true;
        }
        return false;
    }

    bool IsValidMove(Player player, Piece piece, Square target, out MoveType type) {
        type = MoveType.invalid;
        if(!player.pieces.Contains(piece)) return false;
        if(IsSquareOccupied(target)) return false;
        if(IsPendingJumpChoice(player) && piece.pendingFinishJump == false) return false;
        if(IsJump(player, piece, target)) {
            type = MoveType.jump;
            return true;
        }
        if(IsJumpAvailable(player)) return false;
        if(!IsMoveAdjacent(piece, target)) return false;
        if(piece.king == false && !IsMoveForward(player, piece, target)) return false;
        type = MoveType.basic;
        return true;
    }

    public bool IsCurrentPlayerPiece(Piece piece) {
        return (currentPlayer.pieces.Contains(piece));
    }

    private bool IsPendingJumpChoice(Player player) {
        foreach(Piece piece in player.pieces) {
            if(piece.pendingFinishJump == true) return true;
        }
        return false;
    }

    IEnumerable<Square> GetSquares(Square square, int distance) {
        IList<Square> squareList = new List<Square>();
        int min = -distance;
        int max = distance;
        int skip = 2 * distance;
        for(int row = min; row <= max; row += skip) {
            int currentRow = square.row + row;
            if(currentRow >= 0 && currentRow < rows) {
                for(int col = min; col <= max; col += skip) {
                    int currentCol = square.col + col;
                    if(currentCol >= 0 && currentCol < cols) {
                        squareList.Add(_board.grid[currentRow, currentCol].GetComponent<Square>());
                    }
                }
            }
        }
        return squareList;
    }

    public void MovePiece(Piece piece, Square square) {
        if(MovePiece(currentPlayer, piece, square))
            currentPlayer = OtherPlayer(currentPlayer);
    }

    bool IsJumpAvailable(Player player) {
        foreach(Piece piece in player.pieces) {
            IEnumerable<Square> adjacentSquares = GetSquares(piece.square.GetComponent<Square>(), 2);
            foreach(Square adj in adjacentSquares) {
                if(IsJump(player, piece, adj) && (IsMoveForward(player, piece, adj) || piece.king)) return true;
            }
        }
        return false;
    }

    bool IsJump(Player player, Piece piece, Square target) {
        Square current = piece.square.GetComponent<Square>();
        if(IsSquareOccupied(target)) return false;

        if(Mathf.Abs(target.row - current.row) != 2 || Mathf.Abs(target.col - current.col) != 2) return false;

        Player otherPlayer = OtherPlayer(player);

        return IsPlayerPiecePresent(otherPlayer, GetJumpedSquare(piece, target)) && (IsMoveForward(player, piece, GetJumpedSquare(piece, target)) || piece.king);
    }

    Square GetJumpedSquare(Piece piece, Square final) {
        Square current = piece.square.GetComponent<Square>();
        return _board.grid[(final.row + current.row) / 2, (final.col + current.col) / 2].GetComponent<Square>();
    }

    bool IsSquareOccupied(Square target) {
        return (IsPlayerPiecePresent(player1, target) || IsPlayerPiecePresent(player2, target));
    }

    bool IsMoveForward(Player player, Piece piece, Square target) {
        return Mathf.Sign(target.row - piece.square.GetComponent<Square>().row) == Mathf.Sign(player.unitDirection);
    }

    bool IsMoveAdjacent(Piece piece, Square target) {
        Square current = piece.square.GetComponent<Square>();
        return Mathf.Abs(target.row - current.row) == 1 && Mathf.Abs(target.col - current.col) == 1;
    }

    bool IsPlayerPiecePresent(Player player, Square target) {
        return (GetPlayerPiece(player, target) != null);
    }

    Piece GetPlayerPiece(Player player, Square target) {
        foreach(Piece playerPiece in player.pieces) {
            if(playerPiece.square.GetComponent<Square>().number == target.number) {
                return playerPiece;
            }
        }
        return null;
    }

    bool JumpPiece(Player player, Piece piece, Square target, float delay = 0f) {
        piece.pendingFinishJump = false;
        Piece jumpedPiece = GetPlayerPiece(OtherPlayer(player), GetJumpedSquare(piece, target));
        jumpedPiece.square = null;
        Vector3 offBoard = new Vector3(player.unitDirection, 0);
        float hangtime = Piece.CalculateFlightTime(piece.GetComponent<Collider>().bounds.center, target.GetComponent<Collider>().bounds.center);
        StartCoroutine(DelayedFlipDestroy(jumpedPiece, offBoard, delay + hangtime));

        player1.pieces.Remove(jumpedPiece);
        player2.pieces.Remove(jumpedPiece);
        StartCoroutine(piece.FlipToTarget(target, delay));
        piece.square = target.gameObject;
        List<Square> potentialFollowJump = new List<Square>();
        foreach(Square adjacentJump in GetSquares(target, 2)) {
            if(IsJump(player, piece, adjacentJump)) {
                potentialFollowJump.Add(adjacentJump);
            }
        }
        if(potentialFollowJump.Count == 1) {
            return JumpPiece(player, piece, potentialFollowJump[0], hangtime + .1f);
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

    bool MovePiece(Player player, Piece piece, Square target) {
        MoveType type;
        if(IsValidMove(player, piece, target, out type)) {
            switch(type) {

                case MoveType.basic:
                    piece.FlipToTarget(target);
                    piece.square = target.gameObject;
                    if(IsKingRow(player, piece)) {
                        piece.king = true;
                    }
                    return true;

                case MoveType.jump:
                    return JumpPiece(player, piece, target);
                    
            }
        }
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

    bool IsKingRow(Player player, Piece piece) {
        int targetRow = (player.unitDirection == 1 ? rows - 1 : 0);
        if(piece.square.GetComponent<Square>().row == targetRow) {
            return true;
        } else {
            return false;
        }
    }

    Player OtherPlayer(Player player) {
        if(player == player1) return player2;
        else return player1;
    }
}
