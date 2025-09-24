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

        // 1. IntroController에서 현재 회전값 가져오기
        float currentPitch = introController.GetCurrentPitch();
        float currentYaw = introController.GetCurrentYaw();

        // 2. FpsController 활성화 (하지만 입력은 막아둠)
        fpsController.enabled = true;
        fpsController.isInteracting = true; // 입력 차단

        // 3. FpsController의 초기 회전값 설정
        SetFpsControllerRotation(currentPitch, currentYaw);

        // 4. 부드러운 회전 보정
        yield return StartCoroutine(SmoothRotationCorrection());

        // 5. IntroController 비활성화
        introController.enabled = false;

        // 6. FpsController 입력 활성화
        fpsController.isInteracting = false;

        isTransitioning = false;
    }

    private IEnumerator SmoothRotationCorrection()
    {
        float elapsedTime = 0f;

        // 시작 회전값
        Quaternion startPlayerRotation = fpsController.transform.rotation;
        Quaternion startCameraRotation = fpsController.virtualCamera.transform.localRotation;

        // 목표 회전값 (정상화)
        Quaternion targetPlayerRotation = Quaternion.Euler(0, fpsController.transform.eulerAngles.y, 0);
        Quaternion targetCameraRotation = Quaternion.Euler(introController.GetCurrentPitch(), 0, 0);

        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // 부드러운 곡선

            // Player (Yaw) 회전 보간
            fpsController.transform.rotation = Quaternion.Slerp(startPlayerRotation, targetPlayerRotation, t);

            // Camera (Pitch) 회전 보간
            fpsController.virtualCamera.transform.localRotation = Quaternion.Slerp(startCameraRotation, targetCameraRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종 값 확실히 설정
        fpsController.transform.rotation = targetPlayerRotation;
        fpsController.virtualCamera.transform.localRotation = targetCameraRotation;
    }

    private void SetFpsControllerRotation(float pitch, float yaw)
    {
        // Player Y축 회전 (Yaw)
        fpsController.transform.rotation = Quaternion.Euler(0, yaw, 0);

        // Camera X축 회전 (Pitch) - FpsController 내부 변수도 설정
        fpsController.virtualCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);

        // FpsController의 내부 cameraPitch 변수 동기화 (리플렉션 사용)
        var field = typeof(FpsController).GetField("cameraPitch",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(fpsController, pitch);
        }
    }
}