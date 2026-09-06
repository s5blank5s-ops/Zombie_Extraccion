using System.Collections;
using UnityEngine;

public class MusicaAmbienteBucle : MonoBehaviour
{
    [Header("Configuración de Fuentes")]
    public AudioSource audioSourceA;
    public AudioSource audioSourceB;
    public AudioClip clipAmbiente;

    [Header("Ajustes de Bucle")]
    public float tiempoTransicion = 3.0f; // Segundos que tarda en mezclarse el final con el principio

    private AudioSource fuenteActiva;
    private AudioSource fuenteInactiva;
    private bool reproduciendo = false;

    void Start()
    {
        if (clipAmbiente == null) return;

        // Configurar los dos AudioSources
        audioSourceA.clip = clipAmbiente;
        audioSourceB.clip = clipAmbiente;
        audioSourceA.loop = false;
        audioSourceB.loop = false;

        fuenteActiva = audioSourceA;
        fuenteInactiva = audioSourceB;

        // Cargar volumen guardado de los ajustes
        float vol = PlayerPrefs.GetFloat("VolumenMusica", 1.0f);
        bool activa = PlayerPrefs.GetInt("MusicaActivada", 1) == 1;

        fuenteActiva.volume = activa ? vol : 0;
        fuenteActiva.Play();
        reproduciendo = true;

        StartCoroutine(GestionarBucleInfinita());
    }

    IEnumerator GestionarBucleInfinita()
    {
        while (reproduciendo)
        {
            // Esperar hasta que quede poco tiempo para que termine la pista
            float tiempoRestante = clipAmbiente.length - fuenteActiva.time;

            if (tiempoRestante <= tiempoTransicion)
            {
                // Iniciar la transición suave hacia la otra fuente antes de que termine
                StartCoroutine(HacerCrossfade());
                yield return new WaitForSeconds(clipAmbiente.length - tiempoTransicion);
            }

            yield return null;
        }
    }

    IEnumerator HacerCrossfade()
    {
        float volObjetivo = PlayerPrefs.GetInt("MusicaActivada", 1) == 1 ? PlayerPrefs.GetFloat("VolumenMusica", 1.0f) : 0;
        float tiempo = 0;

        fuenteInactiva.time = 0;
        fuenteInactiva.volume = 0;
        fuenteInactiva.Play();

        while (tiempo < tiempoTransicion)
        {
            tiempo += Time.deltaTime;
            float factor = tiempo / tiempoTransicion;

            // Transición progresiva
            fuenteActiva.volume = Mathf.Lerp(volObjetivo, 0, factor);
            fuenteInactiva.volume = Mathf.Lerp(0, volObjetivo, factor);

            yield return null;
        }

        fuenteActiva.Stop();

        // Intercambiar las referencias
        AudioSource temp = fuenteActiva;
        fuenteActiva = fuenteInactiva;
        fuenteInactiva = temp;
    }
}