using UnityEngine;

public class PhoneController : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;

    // 실행 전 인스펙터에서 보던 기본 오프셋
    [SerializeField] private Vector3 positionOffset = new Vector3(0.2f, -0.2f, 0.4f);

    // 회전 오프셋 (폰 모델 방향 맞추기용)
    [SerializeField] private Vector3 rotationOffset = new Vector3(0f, 180f, 180f);

    [SerializeField] private float followSpeed = 15f;

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // --- 위치 ---
        Vector3 targetPos = playerCamera.transform.position 
                          + playerCamera.transform.TransformDirection(positionOffset);

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // --- 회전 ---
        Quaternion targetRot = playerCamera.transform.rotation * Quaternion.Euler(rotationOffset);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, followSpeed * Time.deltaTime);
    }
}
