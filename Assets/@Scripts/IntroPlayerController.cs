using Unity.Cinemachine;
using UnityEngine;

public class IntroPlayerController : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] float sensitivityX = 200f;
    [SerializeField] float sensitivityY = 200f;

    [Header("Clamp Angles")]
    [SerializeField] float maxYaw = 90f;
    [SerializeField] float maxPitch = 60f;

    [Header("Offset")]
    [SerializeField] float yawOffset = -90f;

    [Header("Sit/Stand Settings")]
    [SerializeField] float sitHeight = 0.5f;   // 앉아 있을 때 카메라 y 위치
    [SerializeField] float standHeight = 1.4f; // 일어서 있을 때 카메라 y 위치
    [SerializeField] float transitionSpeed = 2f; // 보간 속도

    private float yaw;
    private float pitch;
    private bool isStanding = false; // 처음에는 앉아 있음
    private Transform camTransform;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = yawOffset;
        pitch = 0f;

        camTransform = GetComponentInChildren<CinemachineCamera>().transform;
        SetCameraHeight(sitHeight); // 시작은 앉은 상태
    }

    void Update()
    {
        // --- 마우스 회전 ---
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;

        yaw += mouseX;
        yaw = Mathf.Clamp(yaw, -maxYaw + yawOffset, maxYaw + yawOffset);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        // --- 앉기/일어서기 전환 ---
        float targetHeight = isStanding ? standHeight : sitHeight;
        Vector3 pos = camTransform.localPosition;
        pos.y = Mathf.Lerp(pos.y, targetHeight, Time.deltaTime * transitionSpeed);
        camTransform.localPosition = pos;
    }

    public void StandUp()  // IntroFlow에서 호출할 메서드
    {
        isStanding = true;
    }

    private void SetCameraHeight(float h)
    {
        Vector3 pos = camTransform.localPosition;
        pos.y = h;
        camTransform.localPosition = pos;
    }



    public float GetCurrentPitch() => pitch;
    public float GetCurrentYaw() => yaw - yawOffset; // Offset 제거한 실제 Yaw
}