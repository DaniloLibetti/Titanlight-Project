using UnityEngine;


public enum SoundType
{
    BUTTON,
    DOOR,
    HACKING,
    OVERHEAT,
    HEATLASER,
    LASERSHOTGUN,
    DASH,
    BULLETHITENEMY,
    FALL,
    PLAYERHIT,
    SPININGBLADEENEMY,
    WARNING,
    ENEMYEXPLODE
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList;
    private static SoundManager instance;
    private AudioSource sFXAudioSource;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        sFXAudioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        instance.sFXAudioSource.PlayOneShot(instance.soundList[(int)sound], volume);
    }
}
