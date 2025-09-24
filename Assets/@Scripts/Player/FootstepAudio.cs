using UnityEngine;
using FpsHorrorKit;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FootstepModule : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] footstepClips; //  /  /  /  ...
    [SerializeField] private AudioClip[] jumpClips;
    [SerializeField] private AudioClip[] landClips;

    [Header("Step Timing")]
    [SerializeField] private float walkInterval = 0.5f;
    [SerializeField] private float sprintInterval = 0.3f;

    [Header("Volumes")]
    [SerializeField] private float stepVolume = 0.6f;
    [SerializeField] private float jumpVolume = 0.7f;
    [SerializeField] private float landVolume = 0.9f;

    [Header("Pitch Variation")]
    [SerializeField] private float pitchVariation = 0.05f;

    private AudioSource audioSource;
    private CharacterController controller;
    private FpsAssetsInputs input;
    private FpsController fpsController;

    private float stepTimer;
    private bool wasGrounded = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<FpsAssetsInputs>();
        fpsController = GetComponent<FpsController>();
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D ȿ
    }

    private void Update()
    {
        HandleFootsteps();
        HandleJumpLand();
    }

    private void HandleFootsteps()
    {
        if (fpsController.isInteracting) return;

        bool isMoving = input.move.magnitude > 0.2f;
        bool isGrounded = fpsController.IsGrounded;

        if (isMoving && isGrounded)
        {
            stepTimer += Time.deltaTime;
            float interval = input.sprint ? sprintInterval : walkInterval;

            if (stepTimer >= interval)
            {
                PlayStep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void HandleJumpLand()
    {
        bool isGrounded = fpsController.IsGrounded;

        // Jump detect:  ִٰ  ư  
        if (wasGrounded && !isGrounded && input.jump)
        {
            PlayJump();
        }

        // Land detect:   
        if (!wasGrounded && isGrounded)
        {
            PlayLand();
        }

        wasGrounded = isGrounded;
    }

    private void PlayStep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        if (clip == null) return;

        audioSource.volume = stepVolume;
        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clip);
    }

    private void PlayJump()
    {
        if (jumpClips == null || jumpClips.Length == 0) return;

        AudioClip clip = jumpClips[Random.Range(0, jumpClips.Length)];
        if (clip == null) return;

        audioSource.volume = jumpVolume;
        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clip);
    }

    private void PlayLand()
    {
        if (landClips == null || landClips.Length == 0) return;

        AudioClip clip = landClips[Random.Range(0, landClips.Length)];
        if (clip == null) return;

        audioSource.volume = landVolume;
        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clip);
    }
}
