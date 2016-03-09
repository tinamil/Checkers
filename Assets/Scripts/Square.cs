using UnityEngine;
using System.Collections;

public class Square : MonoBehaviour {

    public int row { get; set; }
    public int col { get; set; }

    public int number { get { return (col + (row * Checkers.rows)); } }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void setLocation(int _row, int _col) {
        row = _row;
        col = _col;
    }
    
    public void Highlight(Color color) {
        GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
    }

    public void ClearHighlight() {
        GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
    }

    void OnMouseEnter() {
        Piece dragged = Checkers.instance.draggedPiece;
        Checkers.MoveType type;
        if(dragged != null && Checkers.IsValidMove(Checkers.instance.liveState.pieceMap[dragged.square.row, dragged.square.col], this, Checkers.instance.liveState, out type)) {
            Highlight(dragged.GetComponent<MeshRenderer>().material.color);
        }
    }

    void OnMouseExit() {
        Piece dragged = Checkers.instance.draggedPiece;
        if(dragged == null || dragged.square != gameObject) ClearHighlight();
    }
}
