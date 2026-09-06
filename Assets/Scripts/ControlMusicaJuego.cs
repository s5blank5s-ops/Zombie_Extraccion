using UnityEngine;

public class ControlMusicaJuego : MonoBehaviour
{
    [Header("Fuentes de Audio")]
    public AudioSource musicaFondo;
    public AudioSource musicaExtraccion;

    [Header("Referencia")]
    public ZonaExtraccion zonaExtraccion;

    private bool enModoExtraccion = false;

    void Start()
    {
        // 1. Cargar y aplicar los ajustes que el usuario eligió en el Menú de Opciones
        AplicarAjustesMusica();

        // 2. Al arrancar, reproducimos la música de fondo normal
        if (musicaFondo != null)
        {
            musicaFondo.loop = true;
            musicaFondo.Play();
        }

        if (musicaExtraccion != null)
        {
            musicaExtraccion.loop = true;
            musicaExtraccion.Stop();
        }
    }

    public void AplicarAjustesMusica()
    {
        // Obtener datos guardados de PlayerPrefs
        bool musicaActivada = PlayerPrefs.GetInt("MusicaActivada", 1) == 1;
        float volMusica = PlayerPrefs.GetFloat("VolumenMusica", 1.0f);

        // Aplicar a la música de fondo
        if (musicaFondo != null)
        {
            musicaFondo.mute = !musicaActivada;
            musicaFondo.volume = volMusica;
        }

        // Aplicar también a la música de extracción para cuando se active
        if (musicaExtraccion != null)
        {
            musicaExtraccion.mute = !musicaActivada;
            musicaExtraccion.volume = volMusica;
        }
    }

    void Update()
    {
        if (zonaExtraccion == null) return;

        // Detectar el momento en que se activa la extracción
        if (zonaExtraccion.ExtraccionActivada && !enModoExtraccion)
        {
            ActivarMusicaExtraccion();
        }
    }

    void ActivarMusicaExtraccion()
    {
        enModoExtraccion = true;

        // Nos aseguramos de volver a aplicar los ajustes de volumen por si acaso
        AplicarAjustesMusica();

        // Detenemos la música de ambiente normal
        if (musicaFondo != null && musicaFondo.isPlaying)
        {
            musicaFondo.Stop();
        }

        // Iniciamos la música de tensión/invasión
        if (musicaExtraccion != null)
        {
            musicaExtraccion.Play();
        }
    }
}