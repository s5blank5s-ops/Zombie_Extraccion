using UnityEngine;

public class ZonaExtraccion : MonoBehaviour
{
    [Header("Tiempos y Estado")]
    public float tiempoExtraccion = 30f;
    private float tiempoRestante;
    private bool jugadorEnZona = false;
    private bool extraccionActivada = false;

    [Header("Referencias de Luces y FX")]
    public Light luzZona; // Asigna aquí la Point Light de la zona
    public Color colorEspera = Color.green; // O Amarillo
    public Color colorAlerta = Color.red;

    private GeneradorZombis generador;

    // Propiedades públicas para la UI
    public bool JugadorEnZona => jugadorEnZona;
    public bool ExtraccionActivada => extraccionActivada;
    public float TiempoRestante => tiempoRestante;

    void Start()
    {
        tiempoRestante = tiempoExtraccion;
        generador = FindAnyObjectByType<GeneradorZombis>();

        // Configurar color inicial de la luz
        if (luzZona != null)
        {
            luzZona.color = colorEspera;
        }
    }

    void Update()
    {
        if (jugadorEnZona && !extraccionActivada && Input.GetKeyDown(KeyCode.E))
        {
            // Comprobar si el jugador tiene los suministros requeridos
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                InventarioJugador inventario = jugador.GetComponent<InventarioJugador>();
                if (inventario != null && !inventario.TieneSuministrosSuficientes())
                {
                    Debug.Log("¡Necesitas recoger suministros antes de extraer!");
                    return;
                }
            }

            IniciarExtraccion();
        }

        if (extraccionActivada)
        {
            tiempoRestante -= Time.deltaTime;

            // Efecto visual: Luz parpadeante durante la cuenta atrás
            if (luzZona != null)
            {
                luzZona.intensity = Mathf.PingPong(Time.time * 4f, 15f) + 5f;
            }

            if (tiempoRestante <= 0f)
            {
                CompletarExtraccion();
            }
        }
    }

    void IniciarExtraccion()
    {
        if (ControlAudio.Instancia != null)
        {
            ControlAudio.Instancia.ReproducirAlarmaBucle(true);
        }
        if (ControlAudio.Instancia != null)
        {
            ControlAudio.Instancia.ReproducirSonido(ControlAudio.Instancia.audioAlarmaExtraccion);
        }
        
        extraccionActivada = true;

        // Cambiar luz a Alerta/Rojo
        if (luzZona != null)
        {
            luzZona.color = colorAlerta;
        }

        // Iniciar el spawn de la horda
        if (generador != null)
        {
            generador.IniciarGeneracion();
        }
    }

    void CompletarExtraccion()
    {
        extraccionActivada = false;

        if (ControlAudio.Instancia != null)
        {
            ControlAudio.Instancia.ReproducirAlarmaBucle(false);
        }

        if (generador != null)
        {
            generador.DetenerGeneracion();
        }

        Debug.Log("¡Extracción Completada!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnZona = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnZona = false;
        }
    }
}