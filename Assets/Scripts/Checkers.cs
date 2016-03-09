using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class CheckerState {
    internal Board board;

    internal Player currentPlayer;
    internal Player otherPlayer {
        get {
            if(currentPlayer == null) return null;
            else if(currentPlayer == player1) return player2;
            else return player1;
        }
    }

    private Player player1;
    private Player player2;

    internal nPiece[,] pieceMap { get; set; }

    public CheckerState(Board board, Player currentPlayer, Player p1, Player p2, nPiece[,] pieces) {
        this.board = board;
        this.currentPlayer = currentPlayer;
        this.player1 = p1;
        this.player2 = p2;
        this.pieceMap = pieces;
    }

    private CheckerState() {
        pieceMap = new nPiece[Checkers.rows, Checkers.cols];
    }

    public void UpdatePlayers() {
        player1.DoUpdate();
        player2.DoUpdate();
    }

    public void MovePiece(nPiece piece, Square target, bool reassignOriginal) {
        pieceMap[piece.row, piece.col] = null;
        piece.row = target.row;
        piece.col = target.col;
        if(reassignOriginal)
            piece.original.square = target;
        pieceMap[piece.row, piece.col] = piece;
        if(Checkers.IsKingRow(piece.owner, piece)) {
            piece.King(reassignOriginal);
        }
    }

    public void RemovePiece(nPiece piece, bool reassignOriginal) {
        pieceMap[piece.row, piece.col] = null;
        if(reassignOriginal)
            piece.original.square = null;
        piece.row = -1;
        piece.col = -1;
    }

    internal CheckerState DeepCopy() {
        CheckerState copy = new CheckerState();
        copy.board = board;
        copy.player1 = player1;
        copy.player2 = player2;
        copy.currentPlayer = currentPlayer;
        copy.pieceMap = new nPiece[Checkers.rows, Checkers.cols];
        for(int row = 0; row < pieceMap.GetLength(0); ++row) {
            for(int col = 0; col < pieceMap.GetLength(1); ++col) {
                if(pieceMap[row, col] != null)
                    copy.pieceMap[row, col] = new nPiece(pieceMap[row, col]);
            }
        }
        return copy;
    }

}

public class nPiece {
    public Piece original { get; private set; }

    public Player owner { get { return original.owner; } set { original.owner = value; } }

    public int row { get; internal set; }
    public int col { get; internal set; }

    public bool king { get; private set; }

    public bool pendingFinishJump { get; set; }

    internal nPiece(Piece n) {
        this.original = n;
        row = n.square.row;
        col = n.square.col;
    }

    internal nPiece(nPiece n) {
        this.original = n.original;
        this.row = n.row;
        this.col = n.col;
        this.king = n.king;
        this.pendingFinishJump = n.pendingFinishJump;
    }

    internal void King(bool reassignOriginal) {
        if(reassignOriginal) original.King();
        this.king = true;
    }
}

public class Checkers : MonoBehaviour {

    public GameObject boardObject;
    public GameObject pieceObject;

    internal Piece draggedPiece;

    internal CheckerState liveState;

    public static readonly int rows = 8;
    public static readonly int cols = 8;

    [HideInInspector]
    public static Checkers instance = null;

    public enum MoveType { invalid, basic, jump };


    // Use this for initialization
    void Awake() {

        if(instance == null) {
            instance = this;
        } else if(instance != this) {
            Destroy(gameObject);
        }
        Player player1 = InitializePlayer(MainMenu.Player1Setting, 1);
        Player player2 = InitializePlayer(MainMenu.Player2Setting, -1);
        

        nPiece[,] pieceMap = new nPiece[rows, cols];
        Board board;
        board = boardObject.GetComponent<Board>();
        for(int row = 0; row < pieceMap.GetLength(0); ++row) {
            for(int col = 0; col < pieceMap.GetLength(1); ++col) {
                if((row <= 2 || row >= 5) && (row + col) % 2 == 1) {
                    GameObject piece = Instantiate(pieceObject, board.grid[row, col].GetComponent<Collider>().bounds.center, Quaternion.identity) as GameObject;
                    Piece pieceScript = piece.GetComponent<Piece>();
                    pieceScript.square = board.grid[row, col];

                    pieceMap[row, col] = new nPiece(pieceScript);
                    if(row <= 2) {
                        //player1.pieces.Add(pieceScript);
                        pieceScript.owner = player1;
                        piece.GetComponent<MeshRenderer>().material = board.squares[0];
                    } else {
                        //player2.pieces.Add(pieceScript);
                        pieceScript.owner = player2;
                        piece.GetComponent<MeshRenderer>().material = board.squares[1];
                        piece.GetComponent<MeshRenderer>().material.color = Color.gray;
                    }
                }
            }
        }
        liveState = new CheckerState(board, player1, player1, player2, pieceMap);
    }

    Player InitializePlayer(int setting, int direction) {
        Player player = null; 
        switch(setting) {
            case 0:
                player = new Player();
                break;
            case 1:
            case 2:
            case 3:
                player = new AIPlayer((int)Mathf.Floor(setting * 1.5f));
                break;
            default:
                Debug.Assert(false);
                break;
        }
        player.unitDirection = direction;
        return player;
    }

    void Update() {
        liveState.UpdatePlayers();
    }

    static bool IsValidMove(nPiece piece, Square target, CheckerState currentState) {
        MoveType ignore;
        return IsValidMove(piece, target, currentState, out ignore);
    }

    static public bool IsMovablePiece(nPiece piece, CheckerState currentState) {
        foreach(Square s in GetSquares(piece.row, piece.col, currentState, 1)) {
            if(IsValidMove(piece, s, currentState)) return true;
        }
        foreach(Square s in GetSquares(piece.row, piece.col, currentState, 2)) {
            if(IsValidMove(piece, s, currentState)) return true;
        }
        return false;
    }

    static internal bool IsValidMove(nPiece piece, Square target, CheckerState currentState, out MoveType type) {
        type = MoveType.invalid;
        if(piece.owner != currentState.currentPlayer) return false;
        if(IsSquareOccupied(target, currentState)) return false;
        if(IsPendingJumpChoice(currentState.currentPlayer, currentState) && piece.pendingFinishJump == false) return false;
        if(IsJump(piece, target, currentState)) {
            type = MoveType.jump;
            return true;
        }
        if(IsJumpAvailable(currentState)) return false;
        if(!IsMoveAdjacent(piece, target)) return false;
        if(piece.king == false && !IsMoveForward(currentState.currentPlayer, piece, target)) return false;
        type = MoveType.basic;
        return true;
    }

    //bool IsCurrentPlayerPiece(nPiece piece) {
    //    return piece.owner == liveState.currentPlayer;
    //}

    static internal bool IsPendingJumpChoice(Player player, CheckerState currentState) {
        foreach(nPiece piece in currentState.pieceMap) {
            if(piece != null && piece.owner == player && piece.pendingFinishJump == true) return true;
        }
        return false;
    }



    static public IList<Square> GetSquares(int targetRow, int targetCol, CheckerState currentState, params int[] distances) {
        IList<Square> squareList = new List<Square>();
        foreach(int distance in distances) {
            int min = -distance;
            int max = distance;
            int skip = 2 * distance;
            for(int row = min; row <= max; row += skip) {
                int currentRow = targetRow + row;
                if(currentRow >= 0 && currentRow < rows) {
                    for(int col = min; col <= max; col += skip) {
                        int currentCol = targetCol + col;
                        if(currentCol >= 0 && currentCol < cols) {
                            squareList.Add(currentState.board.grid[currentRow, currentCol]);
                        }
                    }
                }
            }
        }
        return squareList;
    }

    public void MovePiece(Piece piece, Square square) {
        float delay;
        nPiece pieceToMove = liveState.pieceMap[piece.square.row, piece.square.col];
        if(MovePiece(pieceToMove, square, liveState, out delay)) {
            StartCoroutine(SwitchPlayer(liveState.otherPlayer, delay + .1f));
            liveState.currentPlayer = null;
        }
    }

    IEnumerator SwitchPlayer(Player otherPlayer, float delay) {
        bool continueGame = false;
        foreach(nPiece p in liveState.pieceMap) {
            if(p != null && p.owner == otherPlayer) {
                continueGame = true;
                break;
            }
        }
        if(continueGame) {
            yield return new WaitForSeconds(delay);
            liveState.currentPlayer = otherPlayer;
        }
    }

    static bool IsJumpAvailable(CheckerState currentState) {
        foreach(nPiece piece in currentState.pieceMap) {
            if(piece == null || piece.owner != currentState.currentPlayer) continue;
            IEnumerable<Square> adjacentSquares = GetSquares(piece.row, piece.col, currentState, 2);
            foreach(Square adj in adjacentSquares) {
                if(IsJump(piece, adj, currentState) && (IsMoveForward(currentState.currentPlayer, piece, adj) || piece.king)) return true;
            }
        }
        return false;
    }

    internal static bool IsJump(nPiece piece, Square target, CheckerState currentState) {
        if(IsSquareOccupied(target, currentState)) return false;

        if(Mathf.Abs(target.row - piece.row) != 2 || Mathf.Abs(target.col - piece.col) != 2) return false;

        return IsPlayerPiecePresent(currentState.otherPlayer, GetJumpedSquare(piece, target, currentState), currentState) && (IsMoveForward(currentState.currentPlayer, piece, GetJumpedSquare(piece, target, currentState)) || piece.king);
    }

    internal static Square GetJumpedSquare(nPiece piece, Square final, CheckerState currentState) {
        return currentState.board.grid[(final.row + piece.row) / 2, (final.col + piece.col) / 2];
    }

    static bool IsSquareOccupied(Square target, CheckerState currentState) {
        return (IsPlayerPiecePresent(currentState.currentPlayer, target, currentState) || IsPlayerPiecePresent(currentState.otherPlayer, target, currentState));
    }

    static bool IsMoveForward(Player player, nPiece piece, Square target) {
        return Mathf.Sign(target.row - piece.row) == Mathf.Sign(player.unitDirection);
    }

    static bool IsMoveAdjacent(nPiece piece, Square target) {
        return Mathf.Abs(target.row - piece.row) == 1 && Mathf.Abs(target.col - piece.col) == 1;
    }

    static bool IsPlayerPiecePresent(Player player, Square target, CheckerState currentState) {
        return (GetPlayerPiece(player, target, currentState) != null);
    }

    static internal nPiece GetPlayerPiece(Player player, Square target, CheckerState currentState) {
        nPiece piece = currentState.pieceMap[target.row, target.col];
        if(piece == null) return null;
        if(player == piece.owner) return piece;
        return null;
    }

    bool JumpPiece(nPiece piece, Square target, CheckerState currentState, ref float delay) {
        piece.pendingFinishJump = false;
        nPiece jumpedPiece = GetPlayerPiece(currentState.otherPlayer, GetJumpedSquare(piece, target, currentState), currentState);
        Vector3 offBoard = new Vector3(currentState.currentPlayer.unitDirection, 0);
        float hangtime = Piece.CalculateFlightTime(piece.original.GetComponent<Collider>().bounds.center, target.GetComponent<Collider>().bounds.center);
        float otherHangtime = Piece.CalculateFlightTime(jumpedPiece.original.GetComponent<Collider>().bounds.center, offBoard);
        StartCoroutine(DelayedFlipDestroy(jumpedPiece.original, offBoard, delay + hangtime));
        StartCoroutine(piece.original.FlipToTarget(target, delay));
        delay = delay + hangtime + otherHangtime;
        currentState.RemovePiece(jumpedPiece, true);
        currentState.MovePiece(piece, target, true);
        List<Square> potentialFollowJump = new List<Square>();
        foreach(Square adjacentJump in GetSquares(piece.row, piece.col, currentState, 2)) {
            if(IsJump(piece, adjacentJump, currentState)) {
                potentialFollowJump.Add(adjacentJump);
            }
        }
        if(potentialFollowJump.Count == 1) {
            return JumpPiece(piece, potentialFollowJump[0], currentState, ref delay);
        } else if(potentialFollowJump.Count == 0) {
            return true;
        } else {
            piece.pendingFinishJump = true;
            return false;
        }
    }

    bool MovePiece(nPiece piece, Square target, CheckerState currentState, out float delay) {
        MoveType type;
        if(IsValidMove(piece, target, currentState, out type)) {
            switch(type) {

                case MoveType.basic:
                    currentState.MovePiece(piece, target, true);
                    delay = piece.original.FlipToTarget(target);
                    return true;

                case MoveType.jump:
                    delay = 0f;
                    return JumpPiece(piece, target, currentState, ref delay);

                default:
                    Debug.Log("Reached unknown move type: " + type);
                    delay = 0f;
                    return false;
            }
        } else {
            delay = 0f;
            return false;
        }
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

    internal static bool IsKingRow(Player player, nPiece piece) {
        int targetRow = (player.unitDirection == 1 ? rows - 1 : 0);
        if(piece.row == targetRow) {
            return true;
        } else {
            return false;
        }
    }
}
