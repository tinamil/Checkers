using UnityEngine;
using System.Collections;
using System;

public class Piece : MonoBehaviour {
    public LayerMask pieceLayer;

    public LayerMask boardLayer;

    public AudioClip hit1;

    public Mesh king;

    private Square _square;

    private bool flying = false;

    public Player owner { get; set; }

    public Square square {
        get { return _square; }
        set {
            _square = value;
            if(value != null) {
                Bounds target = value.GetComponent<Renderer>().bounds;
                xWidth = target.extents.x / 8;
                zWidth = target.extents.z / 8;
            }
        }
    }

    private float xWidth, zWidth;
    
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    void FixedUpdate() {
        if(square != null && !flying) {
            Vector3 difference = square.transform.position - transform.position;
            if(Mathf.Abs(difference.x) < xWidth && Mathf.Abs(difference.z) < zWidth) {
                //Do nothing, close enough to the target
            } else {
                Rigidbody pieceRB = GetComponent<Rigidbody>();
                Vector3 direction = difference.normalized;
                direction.y = 0;
                pieceRB.AddForce(direction * 15);
            }
        }
    }

    public float FlipToTarget(Vector3 target) {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        Vector3 difference = target - transform.position;
        float angle = Mathf.Deg2Rad * 60;
        float velocity = Mathf.Sqrt(difference.magnitude * -Physics.gravity.y / (2 * Mathf.Sin(angle) * Mathf.Cos(angle)));
        float vVel = velocity * Mathf.Sin(angle);
        float hVel = velocity * Mathf.Cos(angle);

        float hAngle = Mathf.Atan2(difference.z, difference.x);
        float xComponent = Mathf.Cos(hAngle) * hVel;
        float zComponent = Mathf.Sin(hAngle) * hVel;

        Vector3 launch = new Vector3(xComponent, vVel, zComponent);

        Rigidbody pieceRB = GetComponent<Rigidbody>();
        pieceRB.AddForce(launch, ForceMode.VelocityChange);
        flying = true;

        float timeOfFlight = 2 * vVel / -Physics.gravity.y;
        StartCoroutine(FinishFlight(timeOfFlight));
        return timeOfFlight;
    }

    public static float CalculateFlightTime(Vector3 start, Vector3 target) {
        Vector3 difference = target - start;
        float angle = Mathf.Deg2Rad * 60;
        float velocity = Mathf.Sqrt(difference.magnitude * -Physics.gravity.y / (2 * Mathf.Sin(angle) * Mathf.Cos(angle)));
        float vVel = velocity * Mathf.Sin(angle);

        float timeOfFlight = 2 * vVel / -Physics.gravity.y;
        return timeOfFlight;
    }

    static public float CalculateFlightTime(Square start, Square target) {
        return CalculateFlightTime(start.GetComponent<Collider>().bounds.center, target.GetComponent<Collider>().bounds.center);
    }

    IEnumerator FinishFlight(float time) {
        yield return new WaitForSeconds(time);
        flying = false;
    }

    void OnDrawGizmos() {
    }
    
    void OnMouseDown() {
        if(owner.mouseControlled && Checkers.IsMovablePiece(Checkers.instance.liveState.pieceMap[square.row, square.col], Checkers.instance.liveState)) {
            square.Highlight(GetComponent<MeshRenderer>().material.color);
            Checkers.instance.draggedPiece = this;
        }
    }

    void OnMouseUp() {
        if(!owner.mouseControlled) return;
        Checkers.instance.draggedPiece = null;
        square.ClearHighlight();
        Ray target = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        float maxDistance = 1000f;
        if(Physics.Raycast(target, out hitInfo, maxDistance, boardLayer)) {
            GameObject targetSquare = hitInfo.collider.gameObject;
            Checkers.instance.MovePiece(this, targetSquare.GetComponent<Square>());
        }
    }

    void OnMouseEnter() {
        if(owner.mouseControlled && square != null && Checkers.instance.liveState.pieceMap[square.row, square.col] != null 
            && Checkers.IsMovablePiece(Checkers.instance.liveState.pieceMap[square.row, square.col], Checkers.instance.liveState)) {
            square.Highlight(GetComponent<MeshRenderer>().material.color);
        }
    }

    void OnMouseExit() {
        if(owner.mouseControlled && Checkers.instance.draggedPiece != this && square != null) {
            square.ClearHighlight();
        }
    }


    public float FlipToTarget(Square target) {
        float time = FlipToTarget(target.GetComponent<Collider>().bounds.center);
        //StartCoroutine(PlayHit(time));
        return time;
    }

    /*IEnumerator PlayHit(float time) {
        //yield return new WaitForSeconds(time);
        //SoundManager.instance.RandomizeSfx(hit1);
        yield return;
    }*/

    public IEnumerator FlipToTarget(Square target, float timeDelay) {
        flying = true;
        yield return new WaitForSeconds(timeDelay);
        FlipToTarget(target);
    }

    void OnCollisionEnter(Collision collision) {
        if(collision.collider.tag == "BoardTag")   
            SoundManager.instance.RandomizeSfx(hit1);
    }

    public void King() {
        GetComponent<MeshFilter>().mesh = king;
    }
}
