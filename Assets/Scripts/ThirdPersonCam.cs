using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    public GameObject thirdPersonCam;
    public GameObject topDownCam;
    public GameObject topDownCamOrto;

    Camera mainCamera;

    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        Topdown,
        TopdownOrto,
        SideScroller
    }

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        switch (currentStyle)
        {
            case CameraStyle.Basic:
                BasicCameraStyle();
                mainCamera.orthographic = false;
                SwitchCameraStyle(CameraStyle.Basic);
                break;
            case CameraStyle.Topdown:
                BasicCameraStyle();
                mainCamera.orthographic = false;
                SwitchCameraStyle(CameraStyle.Topdown);
                break;
            case CameraStyle.TopdownOrto:
                BasicCameraStyle();
                SwitchCameraStyle(CameraStyle.TopdownOrto);
                mainCamera.orthographic = true;
                break;
            case CameraStyle.SideScroller:
                SideScrollingCameraStyle();
                SwitchCameraStyle(CameraStyle.Topdown);
                break;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.GameStarted)
        {
            // switch styles
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchCameraStyle(CameraStyle.Basic);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchCameraStyle(CameraStyle.Topdown);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchCameraStyle(CameraStyle.TopdownOrto);

            // rotate orientation
            Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
            orientation.forward = viewDir.normalized;
        }
    }
    private void FixedUpdate()
    {
        if (GameManager.Instance.GameStarted)
        {
            // roate player object
            if (currentStyle == CameraStyle.Basic || currentStyle == CameraStyle.Topdown)
            {
                BasicCameraStyle();
                mainCamera.orthographic = false;
            }

            if (currentStyle == CameraStyle.TopdownOrto)
            {
                BasicCameraStyle();
                mainCamera.orthographic = true;
            }

            if (currentStyle == CameraStyle.SideScroller)
            {
                SideScrollingCameraStyle();
            }
        }
    }

    void BasicCameraStyle()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
    }

    void SideScrollingCameraStyle()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector3 inputDir = orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        mainCamera.orthographic = false;
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        topDownCamOrto.SetActive(false);
        thirdPersonCam.SetActive(false);
        topDownCam.SetActive(false);

        if (newStyle == CameraStyle.Basic) thirdPersonCam.SetActive(true);
        if (newStyle == CameraStyle.Topdown) topDownCam.SetActive(true);
        if (newStyle == CameraStyle.TopdownOrto) topDownCamOrto.SetActive(true);

        if (newStyle == CameraStyle.SideScroller) topDownCam.SetActive(true);

        currentStyle = newStyle;
    }
}
