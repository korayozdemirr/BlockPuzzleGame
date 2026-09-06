using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Clips")]
    public AudioClip pickSound;
    public AudioClip rotateSound;
    public AudioClip snapSound;
    public AudioClip failSound;
    public AudioClip undoSound;
    public AudioClip popSound;
    public AudioClip victorySound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayPick() => PlayClip(pickSound);
    public void PlayRotate() => PlayClip(rotateSound);
    public void PlaySnap() => PlayClip(snapSound);
    public void PlayFail() => PlayClip(failSound);
    public void PlayUndo() => PlayClip(undoSound);

    public void PlayPop()
    {
        if (popSound == null || sfxSource == null) return;
        sfxSource.pitch = Random.Range(0.92f, 1.08f);
        sfxSource.PlayOneShot(popSound);
        sfxSource.pitch = 1.0f;
    }

    public void PlayVictory() => PlayClip(victorySound);

    private void PlayClip(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(clip);
        }
    }
}