using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class robloxCamera : MonoBehaviour
{
    public Transform player;
    public Transform cameraTransform;
    public Vector3 thirdPersonOffset = new Vector3(0, 2, -5);
    public float sensitivity = 5f;
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 10f;
    public float minYAngle = -30f;
    public float maxYAngle = 60f;
    public bool isShiftLock = false;

    private float rotationY = 0f;
    private float rotationX = 0f;
    private bool isRotating = false;

    private void Start()
    {
        rotationX = transform.eulerAngles.y;
        rotationY = transform.eulerAngles.x;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isShiftLock = !isShiftLock;

            Cursor.lockState = isShiftLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isShiftLock;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;

            if (!isShiftLock)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        if (isRotating || isShiftLock)
        {
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            rotationX += mouseX;
            rotationY -= mouseY;

            rotationY = Mathf.Clamp(rotationY, minYAngle, maxYAngle);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            thirdPersonOffset.z += scroll * zoomSpeed;
            thirdPersonOffset.z = Mathf.Clamp(thirdPersonOffset.z, -maxZoom, -minZoom);
        }

        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 desiredPosition = player.position + rotation * thirdPersonOffset;

        // Проверка на столкновение
        RaycastHit hit;
        if (Physics.Raycast(player.position, rotation * Vector3.back, out hit, thirdPersonOffset.magnitude))
        {
            cameraTransform.position = hit.point + hit.normal * 0.5f; // Немного отодвинуть камеру от поверхности
        }
        else
        {
            cameraTransform.position = desiredPosition;
        }

        cameraTransform.LookAt(player);
        Vector3 lookDirection = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z);
        player.forward = lookDirection;
    }
}
