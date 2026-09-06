using UnityEngine;
using System.Collections; // <-- Esta es la directiva que te falta

public class FarolaParpadeante : MonoBehaviour
{
    [Header("Referencias")]
    public Light luzFarola;

    [Header("Ajustes del Parpadeo")]
    public float intensidadMinima = 0.2f;
    public float intensidadMaxima = 4.0f;
    public float velocidadParpadeoMin = 0.05f;
    public float velocidadParpadeoMax = 0.4f;

    private float intensidadOriginal;

    void Start()
    {
        if (luzFarola == null)
        {
            luzFarola = GetComponentInChildren<Light>();
        }

        if (luzFarola != null)
        {
            intensidadOriginal = luzFarola.intensity;
            StartCoroutine(RutinaParpadeo());
        }
    }

    IEnumerator RutinaParpadeo()
    {
        while (true)
        {
            // Cambiar intensidad de forma aleatoria
            luzFarola.intensity = Random.Range(intensidadMinima, intensidadMaxima);

            // Apagar del todo a veces para un efecto más tétrico
            if (Random.value > 0.8f)
            {
                luzFarola.enabled = false;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
                luzFarola.enabled = true;
            }

            // Esperar un tiempo aleatorio antes del siguiente parpadeo
            yield return new WaitForSeconds(Random.Range(velocidadParpadeoMin, velocidadParpadeoMax));
        }
    }
}
