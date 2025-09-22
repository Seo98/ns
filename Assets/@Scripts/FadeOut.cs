using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FadeOut : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign a full-screen UI Image (usually black). If null, the script will create one as a child of this Canvas.")]
    public Image fadeImage;


    [Header("Look & Timing")]
    public Color fadeColor = Color.black;
    [Tooltip("How quickly the eyes close (seconds)")]
    public float closeDuration = 0.12f;
    [Tooltip("How long eyes stay closed (seconds)")]
    public float holdDuration = 0.06f;
    [Tooltip("How quickly the eyes open (seconds)")]
    public float openDuration = 0.12f;


    [Header("Smoothing")]
    [Tooltip("Curve controlling ease-in/out of the fade (0..1 -> 0..1)")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    [Header("Behavior")]
    [Tooltip("Start the scene fully closed (alpha = 1)")]
    public bool startClosed = false;
    [Tooltip("If no Image assigned, create one automatically")]
    public bool createImageIfMissing = true;


    [Header("Events")]
    public UnityEvent onBlinkCompleted;


    Coroutine running;


    void Awake()
    {
        EnsureImage();
        SetAlpha(startClosed ? 1f : 0f);
    }


    void EnsureImage()
    {
        if (fadeImage != null) return;
        if (!createImageIfMissing) return;


        // Create a GameObject with Image and stretch it to full screen
        GameObject go = new GameObject("EyeBlink_FadeImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);


        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;


        fadeImage = go.GetComponent<Image>();
        fadeImage.raycastTarget = false;
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);


        // Make sure this Canvas is on top (optional) - keep it simple and let user control sorting
    }


    /// <summary>
    /// Trigger a single blink (close -> hold -> open).
    /// </summary>
    public void Blink()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(BlinkRoutine());
    }


    /// <summary>
    /// Same as Blink but forces immediate close and then open (useful for cutscenes).
    /// </summary>
    public void BlinkForced()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(BlinkRoutine(forceStartClosed: true));
    }


    IEnumerator BlinkRoutine(bool forceStartClosed = false)
    {
        // Close
        float start = forceStartClosed ? 0f : GetAlpha();
        yield return Fade(start, 1f, closeDuration);


        // Hold closed
        if (holdDuration > 0f)
            yield return new WaitForSecondsRealtime(holdDuration);


        // Open
        yield return Fade(1f, 0f, openDuration);


        running = null;
        onBlinkCompleted?.Invoke();
    }
    IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null)
        {
            yield break; // nothing to fade
        }


        if (duration <= 0f)
        {
            SetAlpha(to);
            yield break;
        }


        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // unscaled so it still works during slow-mo or paused UI
            float p = Mathf.Clamp01(t / duration);
            float eased = easeCurve.Evaluate(p);
            float a = Mathf.Lerp(from, to, eased);
            SetAlpha(a);
            yield return null;
        }


        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        var c = fadeImage.color;
        c.r = fadeColor.r; c.g = fadeColor.g; c.b = fadeColor.b; c.a = Mathf.Clamp01(a);
        fadeImage.color = c;
    }


    float GetAlpha()
    {
        if (fadeImage == null) return 0f;
        return fadeImage.color.a;
    }


#if UNITY_EDITOR
    // Editor helper to preview a blink from context menu
    [ContextMenu("Preview Blink")]
    void EditorPreviewBlink() { Blink(); }
#endif
}
