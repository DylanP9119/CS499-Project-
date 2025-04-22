using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] private float fovMax = 50;
    [SerializeField] private float fovMin = 10;

    private bool DragPanActive;
    private Vector2 lastMousePos;
    private float targetFieldOfView = 50;

    public Material bigMaterial;
    public Material medMaterial;
    public Material smallMaterial;

    public UISimSettings uiSimSettings;

    private void Update()
    {
        HandleCameraMovement();

        HandleCameraRotation();

        HandleCameraZoom();

    }

    private void HandleCameraMovement()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W) && (transform.position.z < 100) == true) inputDir.y = +1f;
        if (Input.GetKey(KeyCode.S) && (transform.position.z > 0) == true) inputDir.y = -1f;
        if (Input.GetKey(KeyCode.A) && (transform.position.x > 0) == true) inputDir.x = -1f;
        if (Input.GetKey(KeyCode.D) && (transform.position.x < 400) == true) inputDir.x = +1f;


        /* if (Input.GetMouseButtonDown(1))
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

             float dragPanSpeed = 0.25f;
             inputDir.x = -mouseMovementDelta.x * dragPanSpeed;
             inputDir.y = -mouseMovementDelta.y * dragPanSpeed;

             lastMousePos = Input.mousePosition;
         }*/


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

        if (targetFieldOfView > 35) {
            uiSimSettings.DeleteGrid();
            uiSimSettings.UpdateGrid(bigMaterial);
        }
        else if (targetFieldOfView > 10) {
            uiSimSettings.DeleteGrid();
            uiSimSettings.UpdateGrid(medMaterial);
        }
        else {
            uiSimSettings.DeleteGrid();
            uiSimSettings.UpdateGrid(smallMaterial);
        }

        float zoomSpeed = 5f;
        cinemachineVirtualCamera.m_Lens.OrthographicSize =
            Mathf.Lerp(cinemachineVirtualCamera.m_Lens.OrthographicSize, targetFieldOfView, Time.deltaTime * zoomSpeed);
    }
}
