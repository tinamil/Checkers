using UnityEngine;
using System.Collections;
using System;

public class CameraScript : MonoBehaviour {

    public Transform board;
    public float smoothing = 1f;
    public float minDistance;
    public float maxDistance;

    private float distance;
    private float angle = 0f;
    private float yAngle = 0;

    private Vector3 lastMousePos;
    private bool mouseDown = false;

    void Awake() {
        distance = (maxDistance + minDistance) / 2f;
    }

    // Use this for initialization
    void Start() {
        transform.position = CalculateTarget(Mathf.Deg2Rad * -90.1f, Mathf.Deg2Rad * 45.1f);
        transform.LookAt(board);
        Smaa.QualityPreset preset = Smaa.QualityPreset.High;
        int presetNumber = PlayerPrefs.GetInt("SMAAQuality", 2);
        switch(presetNumber) {
            case 0:
                preset = Smaa.QualityPreset.Low;
                break;
            case 1:
                preset = Smaa.QualityPreset.Medium;
                break;
            case 2:
                preset = Smaa.QualityPreset.High;
                break;
            case 3:
                preset = Smaa.QualityPreset.Ultra;
                break;
            default:
                Debug.Assert(false, "Invalid SMAA Quality preset from player prefs: " + presetNumber);
                break;
        }
        GetComponent<Smaa.SMAA>().Quality = preset;
    }

    // Update is called once per frame
    void Update() {
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
        distance -= wheel * distance;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        if(Input.GetMouseButton(1)) {
            if(mouseDown) {
                Vector2 mouseMovement = Input.mousePosition - lastMousePos;
                mouseMovement.x *= 5f / Screen.width;
                mouseMovement.y *= 5f / Screen.height;
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

    private Vector3 CalculateTarget(float horizontal, float vertical) {
        Vector3 target = transform.position;
        angle += horizontal;

        target.x = Mathf.Sin(angle);
        target.z = Mathf.Cos(angle);

        yAngle += vertical;
        yAngle = Mathf.Clamp(yAngle, 0.001f, Mathf.PI * 0.499f);

        target.y = Mathf.Sin(yAngle) * distance;

        float ratio = Mathf.Cos(yAngle) * distance;
        target.x *= ratio;
        target.z *= ratio;

        return target;
    }

    private void move(float horizontal, float vertical) {
        transform.position = Vector3.Slerp(transform.position, CalculateTarget(horizontal, vertical), smoothing * Time.deltaTime);
    }
}
