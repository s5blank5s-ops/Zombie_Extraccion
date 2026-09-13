using UnityEngine;

public class AtaqueSigilo : MonoBehaviour
{
    [Header("Configuración de Sigilo")]
    public float distanciaAsesinato = 2f;    // Distancia máxima para ejecutar
    public float anguloEspalda = 60f;        // Ángulo para considerar que estás detrás
    public KeyCode teclaAsesinar = KeyCode.F; // Tecla para ejecutar

    [Header("Referencias")]
    public LayerMask capaEnemigos;
    public AudioClip sonidoAsesinato;

    private ControlJugador scriptJugador;

    void Start()
    {
        scriptJugador = GetComponent<ControlJugador>();
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaAsesinar))
        {
            IntentarAsesinar();
        }
    }

    void IntentarAsesinar()
    {
        // Detectar enemigos en un radio cercano alrededor del jugador
        Collider[] enemigos = Physics.OverlapSphere(transform.position, distanciaAsesinato, capaEnemigos);

        foreach (var col in enemigos)
        {
            ZombiIA zombi = col.GetComponent<ZombiIA>();
            SaludZombi saludZombi = col.GetComponentInParent<SaludZombi>();

            if (zombi != null && saludZombi != null)
            {
                // 1. Comprobar que el zombi NO esté alertado
                if (zombi.estaAlertado) continue;

                // 2. Comprobar si estamos detrás del zombi
                Vector3 direccionHaciaZombi = (zombi.transform.position - transform.position).normalized;
                float angulo = Vector3.Angle(zombi.transform.forward, direccionHaciaZombi);

                // Si el ángulo es pequeño, significa que ambos miran en la misma dirección (estamos detrás)
                if (angulo < anguloEspalda)
                {
                    EjecutarAsesinato(zombi, saludZombi);
                    break; // Solo ejecutamos a uno a la vez
                }
            }
        }
    }

    void EjecutarAsesinato(ZombiIA zombi, SaludZombi salud)
    {
        // Reproducir sonido de puñalada/ejecución
        if (ControlAudio.Instancia != null && sonidoAsesinato != null)
        {
            float volSFX = PlayerPrefs.GetFloat("VolumenSFX", 1.0f);
            ControlAudio.Instancia.ReproducirSonido(sonidoAsesinato, volSFX);
        }

        // Infligir daño masivo para eliminar al zombi al instante sin alertar a otros por ruido
        salud.RecibirDanio(9999f);

        Debug.Log("¡Asesinato sigiloso completado!");
    }

    // Dibujar el radio de acción en el editor de Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAsesinato);
    }
}