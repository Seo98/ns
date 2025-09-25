using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class TimelineInputBlocker : MonoBehaviour
{
    public PlayerInput playerInput;


    public void DisableInput() => playerInput.DeactivateInput();
    public void EnableInput() => playerInput.ActivateInput();

}
