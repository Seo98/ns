using TMPro;
using UnityEngine;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public CanvasGroup canvasGroup;
    public float charDelay = 0.05f;

    private Coroutine routine;

    public void ShowText(string message)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(TypeRoutine(message));
    }

    private IEnumerator TypeRoutine(string message)
    {
        canvasGroup.alpha = 1;
        textUI.text = "";

        foreach (char c in message)
        {
            textUI.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        // 2~3초 후 자동 사라지기
        yield return new WaitForSeconds(2f);
        yield return FadeOut();
    }

    private IEnumerator FadeOut()
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * 2;
            yield return null;
        }
        textUI.text = "";
    }
}
