using FpsHorrorKit;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ControllerTransitionManager : MonoBehaviour
{
    [Header("Controllers")]
    public IntroPlayerController introController;
    public FpsController fpsController;

    [Header("Transition Settings")]
    public float transitionDuration = 1f;

    private bool isTransitioning = false;

    public void StartTransition()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToFpsController());
        }
    }

    private IEnumerator TransitionToFpsController()
    {
        isTransitioning = true;

        // 1. IntroController���� ���� ȸ���� ��������
        float currentPitch = introController.GetCurrentPitch();
        float currentYaw = introController.GetCurrentYaw();

        // 2. FpsController Ȱ��ȭ (������ �Է��� ���Ƶ�)
        fpsController.enabled = true;
        fpsController.isInteracting = true; // �Է� ����

        // 3. FpsController�� �ʱ� ȸ���� ����
        SetFpsControllerRotation(currentPitch, currentYaw);

        // 4. �ε巯�� ȸ�� ����
        yield return StartCoroutine(SmoothRotationCorrection());

        // 5. IntroController ��Ȱ��ȭ
        introController.enabled = false;

        // 6. FpsController �Է� Ȱ��ȭ
        fpsController.isInteracting = false;

        isTransitioning = false;
    }

    private IEnumerator SmoothRotationCorrection()
    {
        float elapsedTime = 0f;

        // ���� ȸ����
        Quaternion startPlayerRotation = fpsController.transform.rotation;
        Quaternion startCameraRotation = fpsController.virtualCamera.transform.localRotation;

        // ��ǥ ȸ���� (����ȭ)
        Quaternion targetPlayerRotation = Quaternion.Euler(0, fpsController.transform.eulerAngles.y, 0);
        Quaternion targetCameraRotation = Quaternion.Euler(introController.GetCurrentPitch(), 0, 0);

        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // �ε巯�� �

            // Player (Yaw) ȸ�� ����
            fpsController.transform.rotation = Quaternion.Slerp(startPlayerRotation, targetPlayerRotation, t);

            // Camera (Pitch) ȸ�� ����
            fpsController.virtualCamera.transform.localRotation = Quaternion.Slerp(startCameraRotation, targetCameraRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ���� �� Ȯ���� ����
        fpsController.transform.rotation = targetPlayerRotation;
        fpsController.virtualCamera.transform.localRotation = targetCameraRotation;
    }

    private void SetFpsControllerRotation(float pitch, float yaw)
    {
        // Player Y�� ȸ�� (Yaw)
        fpsController.transform.rotation = Quaternion.Euler(0, yaw, 0);

        // Camera X�� ȸ�� (Pitch) - FpsController ���� ������ ����
        fpsController.virtualCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);

        // FpsController�� ���� cameraPitch ���� ����ȭ (���÷��� ���)
        var field = typeof(FpsController).GetField("cameraPitch",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(fpsController, pitch);
        }
    }
}