using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {
	public GameObject square;
    public Material[] squares;

    public GameObject[,] grid;


    // Use this for initialization
    void Awake() {
        grid = new GameObject[Checkers.rows, Checkers.cols];

        Collider collider = gameObject.GetComponent<Collider>();
        Bounds bounds = collider.bounds;
        Vector3 max = bounds.max;
        Vector3 min = bounds.min;

        float xRange = max.x - min.x;
        float zRange = max.z - min.z;

        float colWidth = xRange / Checkers.cols;
        float rowWidth = zRange / Checkers.rows;

        int color = 0;
        for(int row = 0; row < Checkers.rows; ++row) {
            color += 1;
            for(int col = 0; col < Checkers.cols; ++col) {
                color += 1;
                Vector3 center = new Vector3(min.x + row * rowWidth + rowWidth / 2f, .52f, min.z + col * colWidth + colWidth / 2f);
                GameObject squareObject = Instantiate(square, center, Quaternion.identity) as GameObject;
                squareObject.GetComponent<MeshRenderer>().sharedMaterial = squares[color % squares.Length];
                squareObject.transform.SetParent(gameObject.transform, false);
				squareObject.transform.Rotate (new Vector3 (90, 0, 0));
                squareObject.transform.localScale = new Vector3(rowWidth, colWidth, 1);
                grid[row, col] = squareObject;
                squareObject.GetComponent<Square>().setLocation(row, col);
            }
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
