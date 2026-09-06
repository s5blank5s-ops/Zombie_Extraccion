using UnityEngine;

public class InventarioJugador : MonoBehaviour
{
    [Header("Inventario")]
    public int botiquines = 1;
    public int suministrosMision = 0;
    public int suministrosRequeridos = 1; // Para la extracción
    public float curacionPorBotiquin = 40f;

    private SaludJugador saludJugador;

    void Start()
    {
        saludJugador = GetComponent<SaludJugador>();
    }

    void Update()
    {
        // Pulsar '1' para curarse
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            UsarBotiquin();
        }
    }

    // --- MÉTODOS QUE LLAMA OBJETOLOOT Y CONTROLUI ---

    public void AgregarBotiquin(int cantidad)
    {
        botiquines += cantidad;
        Debug.Log($"Botiquines actualizados: {botiquines}");
    }

    public void AgregarSuministro(int cantidad)
    {
        suministrosMision += cantidad;
        Debug.Log($"Suministros recolectados: {suministrosMision}");
    }

    public bool TieneSuministrosSuficientes()
    {
        return suministrosMision >= suministrosRequeridos;
    }

    public void UsarBotiquin()
    {
        if (saludJugador == null) return;

        if (botiquines > 0 && saludJugador.vidaActual < saludJugador.vidaMaxima)
        {
            botiquines--;
            saludJugador.vidaActual = Mathf.Min(saludJugador.vidaActual + curacionPorBotiquin, saludJugador.vidaMaxima);

            if (ControlAudio.Instancia != null && ControlAudio.Instancia.audioLoot != null)
            {
                ControlAudio.Instancia.ReproducirSonido(ControlAudio.Instancia.audioLoot);
            }

            Debug.Log($"Botiquín usado. Vida: {saludJugador.vidaActual}. Restantes: {botiquines}");
        }
    }
}