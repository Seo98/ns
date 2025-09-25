using UnityEngine;

public class DialogueSignalReceiver : MonoBehaviour
{
    public TypewriterEffect typewriter;

    public void ShowLine(string line)
    {
        if (typewriter != null)
            typewriter.ShowText(line);
    }
}
