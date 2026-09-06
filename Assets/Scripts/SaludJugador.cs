using UnityEngine;
using UnityEngine.SceneManagement;

public class SaludJugador : MonoBehaviour
{
    [Header("Vida del Jugador")]
    public float vidaMaxima = 100f;
    public float vidaActual;

    private bool estaMuerto = false;

    void Start()
    {
        vidaActual = vidaMaxima;
    }

    void Update()
    {
        // Si el jugador ha muerto y pulsa R, se reinicia el nivel
        if (estaMuerto && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f; // Restaurar la velocidad del juego
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void RecibirDanio(float cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;
        Debug.Log("¡Jugador atacado! Vida restante: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        estaMuerto = true;
        Debug.Log("¡HAS MUERTO! Presiona [R] para reiniciar.");

        // Desactivamos el control y movimiento para que no siga rotando/moviéndose
        ControlJugador control = GetComponent<ControlJugador>();
        if (control != null) control.enabled = false;

        // Opcional: Pausar la física
        Time.timeScale = 0.2f; // Efecto cámara lenta al morir
    }

    public bool EstaMuerto()
    {
        return estaMuerto;
    }
}