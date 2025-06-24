using UnityEngine;




[RequireComponent(typeof(AudioSource))]
public class MusicaManager : MonoBehaviour
{
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip playMusic;
    private static MusicaManager instance;
    private AudioSource musicAudioSource;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        musicAudioSource = GetComponent<AudioSource>();
        musicAudioSource.clip = menuMusic;
        musicAudioSource.Play();
    }

    public static void PlayMusic()
    {
        instance.musicAudioSource.Stop();
        instance.musicAudioSource.clip = instance.playMusic;
        instance.musicAudioSource.Play();
    }

    public static void BackToMenu()
    {
        instance.musicAudioSource.Stop();
        instance.musicAudioSource.clip = instance.menuMusic;
        instance.musicAudioSource.Play();
    }
}
