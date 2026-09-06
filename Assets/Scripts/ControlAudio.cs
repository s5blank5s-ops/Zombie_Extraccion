using UnityEngine;

public class ControlAudio : MonoBehaviour
{
    public static ControlAudio Instancia;

    [Header("Sonidos del Jugador")]
    public AudioClip audioPasos;
    public AudioClip audioDisparo;
    public AudioClip audioSinBalas;
    public AudioClip audioRecargar;
    public AudioClip audioCurar;

    [Header("Sonidos de Zombis")]
    public AudioClip audioAlertaZombi;
    public AudioClip audioAtaqueZombi;

    [Header("Sonidos de la Extracción")]
    public AudioClip audioAlarmaExtraccion;
    public AudioClip audioLoot;

    private AudioSource audioSourceGlobal;

    void Awake()
    {
        // Patron Singleton para acceder facilmente desde cualquier script
        if (Instancia == null)
        {
            Instancia = this;
            audioSourceGlobal = GetComponent<AudioSource>();
            if (audioSourceGlobal == null)
            {
                audioSourceGlobal = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ReproducirAlarmaBucle(bool activar)
    {
        if (audioSourceGlobal == null || audioAlarmaExtraccion == null) return;

        if (activar)
        {
            float volSFX = PlayerPrefs.GetFloat("VolumenSFX", 1.0f);
            audioSourceGlobal.volume = volSFX;
            audioSourceGlobal.clip = audioAlarmaExtraccion;
            audioSourceGlobal.loop = true;
            audioSourceGlobal.Play();
        }
        else
        {
            audioSourceGlobal.loop = false;
            audioSourceGlobal.Stop();
        }
    }

    // Reproduce un sonido 2D (interfaz, disparo local, etc.)
    public void ReproducirSonido(AudioClip clip, float volumen = 1.0f)
    {
        if (clip != null && audioSourceGlobal != null)
        {
            // Multiplicamos el volumen enviado por el SFX guardado en Opciones
            float volFinal = volumen * PlayerPrefs.GetFloat("VolumenSFX", 1.0f);
            audioSourceGlobal.PlayOneShot(clip, volFinal);
        }
    }

    // Reproduce un sonido en una posicion 3D especifica (ej. donde esta un zombi o una caja)
    public void ReproducirSonido3D(AudioClip clip, Vector3 posicion, float volumen = 1.0f)
    {
        if (clip != null)
        {
            // Multiplicamos el volumen enviado por el SFX guardado en Opciones
            float volFinal = volumen * PlayerPrefs.GetFloat("VolumenSFX", 1.0f);
            AudioSource.PlayClipAtPoint(clip, posicion, volFinal);
        }
    }
}