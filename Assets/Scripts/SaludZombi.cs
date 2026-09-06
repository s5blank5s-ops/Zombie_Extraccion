using UnityEngine;

public class SaludZombi : MonoBehaviour
{
    public float vidaMaxima = 50f;
    private float vidaActual;

    void Start()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDanio(float cantidad)
    {
        vidaActual -= cantidad;
        Debug.Log("Zombi golpeado. Vida restante: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        Debug.Log("¡Zombi eliminado!");
        // Aquí podremos añadir efectos de sonido, partículas de sangre o soltar loot en el futuro
        Destroy(gameObject);
    }
}