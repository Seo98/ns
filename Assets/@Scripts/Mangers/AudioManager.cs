using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource introSource;
    [SerializeField] private AudioSource informationSource;

    [SerializeField] private AudioClip[] bgmCrip;
    [SerializeField] private AudioClip[] sfxCrip;
    [SerializeField] private AudioClip[] introCrip;
    [SerializeField] private AudioClip[] informationCrip;
}
