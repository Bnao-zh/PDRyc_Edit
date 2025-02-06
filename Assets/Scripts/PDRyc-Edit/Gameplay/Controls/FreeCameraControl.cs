using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraControl : MonoBehaviour
{
    public Transform cameraTransform;
    public float moveSpeed = 10.0f; // 摄像头移动的速度
    public float lookSensitivity = 2.0f; // 用鼠标旋转的灵敏度

    private Vector2 currentMouseLook = new Vector2();
    private Vector2 mouseLook = new Vector2();
    void Start()
    {
        cameraTransform = transform;
    }
    void Update()
    {
        if (EditManager.Instance.IsFreeCamera)
        {
            // WASD键控制摄像头的移动
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal) * moveSpeed * Time.deltaTime;
            transform.position += moveDirection;
            // 用锁定的鼠标移动量旋转摄像头
            mouseLook = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            currentMouseLook += mouseLook * lookSensitivity;
            currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, -90f, 90f);
            transform.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
        }
    }
}
