using UnityEngine;

public class PhoneController : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;

    // ���� �� �ν����Ϳ��� ���� �⺻ ������
    [SerializeField] private Vector3 positionOffset = new Vector3(0.2f, -0.2f, 0.4f);

    // ȸ�� ������ (�� �� ���� ���߱��)
    [SerializeField] private Vector3 rotationOffset = new Vector3(0f, 180f, 180f);

    [SerializeField] private float followSpeed = 15f;

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // --- ��ġ ---
        Vector3 targetPos = playerCamera.transform.position 
                          + playerCamera.transform.TransformDirection(positionOffset);

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // --- ȸ�� ---
        Quaternion targetRot = playerCamera.transform.rotation * Quaternion.Euler(rotationOffset);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, followSpeed * Time.deltaTime);
    }
}
