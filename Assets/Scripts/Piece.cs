using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour {

    bool pickedUp = false;

    public LayerMask boardLayer;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        
	}

   void OnDrawGizmos() {
        if(pickedUp) {
            Ray target = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            float maxDistance = 1000f;
            if(Physics.Raycast(target, out hitInfo, maxDistance, boardLayer)) {
                Gizmos.DrawCube(hitInfo.point, new Vector3(.2f, .2f, .2f));
            }
        }
    }

    void OnMouseDown() {
        Debug.Log("Mouse Down");
        pickedUp = true;
    }

    void OnMouseUp() {
        Debug.Log("Mouse Up");
        pickedUp = false;
    }
}
