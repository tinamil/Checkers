using UnityEngine;
using System.Collections;
using System;

public class CameraScript : MonoBehaviour {

    public Transform board;

    public float smoothing = 1f;

    private float distance = 2f;
    private float angle = 0f;
    private float yAngle = 0;

    private Vector3 lastMousePos;
    private bool mouseDown = false;

	// Use this for initialization
	void Start () {
        move(0f, 0f);
        transform.LookAt(board);
    }
	
	// Update is called once per frame
	void Update () {
        mouseMove();
        keyboardMove();
        transform.LookAt(board);
	}

    private void keyboardMove() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        horizontal = Mathf.Clamp(horizontal, -.01f, .01f);
        float vertical = Input.GetAxisRaw("Vertical");
        vertical = Mathf.Clamp(vertical, -.01f, .01f);
        move(horizontal, vertical);
    }

    private void mouseMove() {
        float wheel = Input.GetAxisRaw("Mouse ScrollWheel");
        distance -= wheel*10;
        distance = Mathf.Clamp(distance, .4f, 40f);
        if(Input.GetMouseButton(1)) {
            if(mouseDown) {
                Vector2 mouseMovement = Input.mousePosition - lastMousePos;
                mouseMovement.x *= 5f/Screen.width;
                mouseMovement.y *= 5f/Screen.height;
                move(mouseMovement.x, mouseMovement.y);
                lastMousePos = Input.mousePosition;
            } else {
                mouseDown = true;
                lastMousePos = Input.mousePosition;
            }
        } else {
            mouseDown = false;
        }
    }

    private void move(float horizontal, float vertical) {
        Vector3 target = transform.position;
        angle += horizontal;
       
        target.x = Mathf.Sin(angle);
        target.z = Mathf.Cos(angle);

        yAngle += vertical;
        yAngle = Mathf.Clamp(yAngle, 0, Mathf.PI * 0.5f);

        target.y = Mathf.Sin(yAngle) * distance;

        float ratio = Mathf.Cos(yAngle) * distance;
        target.x *= ratio;
        target.z *= ratio;
          
        transform.position = Vector3.Slerp(transform.position, target, smoothing * Time.deltaTime);
    }
}
