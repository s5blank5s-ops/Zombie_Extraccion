using UnityEngine;

public class ControlLinterna : MonoBehaviour
{
    [Header("Referencias")]
    public Light linterna; // Arrastra aquí la Spot Light

    [Header("Configuración")]
    public KeyCode teclaLinterna = KeyCode.F;
    public bool empiezaEncendida = false;

    private bool encendida = false;

    void Start()
    {
        if (linterna == null)
        {
            linterna = GetComponentInChildren<Light>();
        }

        if (linterna != null)
        {
            encendida = empiezaEncendida;
            linterna.enabled = encendida;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaLinterna) && linterna != null)
        {
            ToggleLinterna();
        }
    }

    void ToggleLinterna()
    {
        encendida = !encendida;
        linterna.enabled = encendida;

        // Efecto visual: si encendemos, subimos un poco el ruido momentáneamente
        // O podrías notificar a los zombis si te ven la luz (más avanzado)
        Debug.Log("Linterna: " + (encendida ? "ENCENDIDA" : "APAGADA"));
    }

    public bool EstaEncendida()
    {
        return encendida;
    }
}