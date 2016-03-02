using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {

    private int rows = 8;
    private int cols = 8;

    private Square[,] grid;


    class Square {
        public Vector3 center { get; private set; }

        public Square(Vector3 center) {
            this.center = center;
        }

    }

    // Use this for initialization
    void Awake() {
        grid = new Square[rows, cols];

        Collider collider = gameObject.GetComponent<Collider>();
        Bounds bounds = collider.bounds;
        Vector3 max = bounds.max;
        Vector3 min = bounds.min;

        float xRange = max.x - min.x;
        float zRange = max.z - min.z;

        float colWidth = xRange / cols;
        float rowWidth = zRange / rows;

        for(int row = 0; row < rows; ++row) {
            for(int col = 0; col < cols; ++col) {
                Square square = new Square(new Vector3(min.x + row * rowWidth + rowWidth / 2f, .1f, min.z + col * colWidth + colWidth / 2f));
                grid[row, col] = square;
            }
        }
    }

    void OnDrawGizmos() {
        for(int row = 0; row < rows; ++row) {
            for(int col = 0; col < cols; ++col) {
                Square square = grid[row, col];
                Gizmos.DrawSphere(square.center, .1f);
            }
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
