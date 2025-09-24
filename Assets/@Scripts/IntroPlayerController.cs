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
    [SerializeField] float sitHeight = 0.5f;   // �ɾ� ���� �� ī�޶� y ��ġ
    [SerializeField] float standHeight = 1.4f; // �Ͼ ���� �� ī�޶� y ��ġ
    [SerializeField] float transitionSpeed = 2f; // ���� �ӵ�

    private float yaw;
    private float pitch;
    private bool isStanding = false; // ó������ �ɾ� ����
    private Transform camTransform;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = yawOffset;
        pitch = 0f;

        camTransform = GetComponentInChildren<CinemachineCamera>().transform;
        SetCameraHeight(sitHeight); // ������ ���� ����
    }

    void Update()
    {
        // --- ���콺 ȸ�� ---
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;

        yaw += mouseX;
        yaw = Mathf.Clamp(yaw, -maxYaw + yawOffset, maxYaw + yawOffset);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        // --- �ɱ�/�Ͼ�� ��ȯ ---
        float targetHeight = isStanding ? standHeight : sitHeight;
        Vector3 pos = camTransform.localPosition;
        pos.y = Mathf.Lerp(pos.y, targetHeight, Time.deltaTime * transitionSpeed);
        camTransform.localPosition = pos;
    }

    public void StandUp()  // IntroFlow���� ȣ���� �޼���
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
    public float GetCurrentYaw() => yaw - yawOffset; // Offset ������ ���� Yaw
}