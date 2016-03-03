using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour {
    public LayerMask pieceLayer;

    public LayerMask boardLayer;

    private GameObject _square;

    private bool flying = false;

    public GameObject square {
        get { return _square; }
        set {
            _square = value;
            Bounds target = value.GetComponent<Renderer>().bounds;
            xWidth = target.extents.x / 8;
            zWidth = target.extents.z / 8;
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

    void FlipToTarget(Vector3 target) {
        Vector3 difference = target - transform.position;
        float angle = Mathf.Deg2Rad * 60;
        float velocity = Mathf.Sqrt(difference.magnitude * -Physics.gravity.y / (2*Mathf.Sin(angle)*Mathf.Cos(angle)));
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
    }

    IEnumerator FinishFlight(float time) {
        yield return new WaitForSeconds(time);
        flying = false;
    }

    void OnDrawGizmos() {
    }

    void OnMouseDown() {
    }

    void OnMouseUp() {
        Ray target = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        float maxDistance = 1000f;
        if(Physics.Raycast(target, out hitInfo, maxDistance, boardLayer)) {
            GameObject targetSquare = hitInfo.collider.gameObject;
            square = targetSquare;
            FlipToTarget(square.GetComponent<Collider>().bounds.center);
        }
    }
}
