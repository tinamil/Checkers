using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {
	public GameObject square;
    public Material[] squares;
    public GameObject piece;

    private int rows = 8;
    private int cols = 8;
    
    class Square {
        public Vector3 center { get; private set; }

        public Square(Vector3 center) {
            this.center = center;
        }

    }

    // Use this for initialization
    void Awake() {

        Collider collider = gameObject.GetComponent<Collider>();
        Bounds bounds = collider.bounds;
        Vector3 max = bounds.max;
        Vector3 min = bounds.min;

        float xRange = max.x - min.x;
        float zRange = max.z - min.z;

        float colWidth = xRange / cols;
        float rowWidth = zRange / rows;

        int color = 0;
        for(int row = 0; row < rows; ++row) {
            color += 1;
            for(int col = 0; col < cols; ++col) {
                color += 1;
                Vector3 center = new Vector3(min.x + row * rowWidth + rowWidth / 2f, .502f, min.z + col * colWidth + colWidth / 2f);
                GameObject squareObject = Instantiate(square, center, Quaternion.identity) as GameObject;
                squareObject.GetComponent<MeshRenderer>().sharedMaterial = squares[color % squares.Length];
                squareObject.transform.SetParent(gameObject.transform, false);
				squareObject.transform.Rotate (new Vector3 (90, 0, 0));
                squareObject.transform.localScale = new Vector3(rowWidth, colWidth, 1);

                if(color % squares.Length == 0 && (row <= 2 || row >= 5)) {
                    GameObject pieceObject = Instantiate(piece, center, Quaternion.Euler(90, 0, 0)) as GameObject;
                    pieceObject.GetComponent<Piece>().square = squareObject;
                }
            }
        }
    }

    void OnDrawGizmos() {
        
    }

    // Update is called once per frame
    void Update() {

    }
}
