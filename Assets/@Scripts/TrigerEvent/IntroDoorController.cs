using UnityEngine;

public class IntroDoorController : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenDoor()
    {
        animator.SetTrigger("Open");
    }

    public void CloseDoor()
    {
        animator.SetTrigger("Close");
    }
}
