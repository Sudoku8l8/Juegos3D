using UnityEngine;

/// <summary>
/// Sistema centralizado de Audio (Música de fondo y Efectos de Sonido).
/// Maneja la reproducción de audio 2D y genera tonos de respaldo si no hay clips asignados.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public enum SoundType
    {
        Jump,
        Collect,
        Damage,
        Checkpoint,
        PortalActive,
        Victory,
        GameOver,
        ButtonClick
    }

    [Header("Canales de Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips de Música")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Clips de Efectos de Sonido (SFX)")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip collectClip;
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip checkpointClip;
    [SerializeField] private AudioClip portalActiveClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip buttonClickClip;

    [Header("Ajustes")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Configurar fuentes de audio si no fueron asignadas
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        ApplyVolumes();
    }

    private void Start()
    {
        PlayMusic();
    }

    public void ApplyVolumes()
    {
        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public void PlayMusic()
    {
        if (musicSource == null) return;

        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PlaySFX(SoundType type)
    {
        if (sfxSource == null) return;

        AudioClip clipToPlay = GetClipForType(type);

        if (clipToPlay != null)
        {
            sfxSource.PlayOneShot(clipToPlay, sfxVolume);
        }
        else
        {
            // Respaldo procedural sutil si no hay audio asignado aún en el inspector
            PlayProceduralBeep(type);
        }
    }

    private AudioClip GetClipForType(SoundType type)
    {
        switch (type)
        {
            case SoundType.Jump: return jumpClip;
            case SoundType.Collect: return collectClip;
            case SoundType.Damage: return damageClip;
            case SoundType.Checkpoint: return checkpointClip;
            case SoundType.PortalActive: return portalActiveClip;
            case SoundType.Victory: return victoryClip;
            case SoundType.GameOver: return gameOverClip;
            case SoundType.ButtonClick: return buttonClickClip;
            default: return null;
        }
    }

    /// <summary>
    /// Genera un tono simple sintético para no fallar si el alumno no ha importado archivos de audio.
    /// </summary>
    private void PlayProceduralBeep(SoundType type)
    {
        float frequency = 440f;
        float duration = 0.15f;

        switch (type)
        {
            case SoundType.Jump: frequency = 320f; duration = 0.12f; break;
            case SoundType.Collect: frequency = 780f; duration = 0.18f; break;
            case SoundType.Damage: frequency = 180f; duration = 0.25f; break;
            case SoundType.Victory: frequency = 880f; duration = 0.5f; break;
            case SoundType.GameOver: frequency = 140f; duration = 0.6f; break;
            default: frequency = 440f; break;
        }

        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration); // fade out
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope * 0.3f;
        }

        AudioClip synthClip = AudioClip.Create("SynthBeep", sampleCount, 1, sampleRate, false);
        synthClip.SetData(samples, 0);
        sfxSource.PlayOneShot(synthClip, sfxVolume);
    }
}
