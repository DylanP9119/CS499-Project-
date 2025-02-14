using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineCamera CinemachineCamera;
    [SerializeField] private float fovMax = 50;
    [SerializeField] private float fovMin = 10;

    private bool DragPanActive;
    private Vector2 lastMousePos;
    private float targetFieldOfView = 50;

    private void Update()
    {
        HandleCameraMovement();

        HandleCameraRotation();

        HandleCameraZoom();

    }

    private void HandleCameraMovement()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) inputDir.y = +1f;
        if (Input.GetKey(KeyCode.S)) inputDir.y = -1f;
        if (Input.GetKey(KeyCode.A)) inputDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) inputDir.x = +1f;


        if (Input.GetMouseButtonDown(1))
        {
            DragPanActive = true;
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            DragPanActive = false;
        }

        if (DragPanActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePos;

            float dragPanSpeed = 1f;
            inputDir.x = mouseMovementDelta.x * dragPanSpeed;
            inputDir.y = mouseMovementDelta.y * dragPanSpeed;

            lastMousePos = Input.mousePosition;
        }


        Vector3 moveDir = transform.up * inputDir.y + transform.right * inputDir.x;
        float moveSpeed = 20f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

    }

    private void HandleCameraRotation()
    {
        float rotateDir = 0f;
        if (Input.GetKey(KeyCode.Q)) rotateDir = +1f;
        if (Input.GetKey(KeyCode.E)) rotateDir = -1f;

        float rotateSpeed = 100f;
        transform.eulerAngles += new Vector3(0, 0, rotateDir * rotateSpeed * Time.deltaTime);

    }

    private void HandleCameraZoom()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            targetFieldOfView -= 5;
        }

        if (Input.mouseScrollDelta.y < 0)
        {
            targetFieldOfView += 5;
        }

        targetFieldOfView = Mathf.Clamp(targetFieldOfView, fovMin, fovMax);

        float zoomSpeed = 5f;
        CinemachineCamera.Lens.OrthographicSize =
            Mathf.Lerp(CinemachineCamera.Lens.OrthographicSize, targetFieldOfView, Time.deltaTime * zoomSpeed);
    }
}
