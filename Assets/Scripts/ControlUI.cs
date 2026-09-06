using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ControlUI : MonoBehaviour
{
    [Header("Referencias UI - Vida")]
    public Image rellenoBarraVida;
    private float anchoMaximoBarra;

    [Header("Referencias UI - Munición")]
    public TextMeshProUGUI textoMunicion;

    [Header("Referencias UI - Inventario")]
    public TextMeshProUGUI textoBotiquines;
    public TextMeshProUGUI textoSuministros;

    [Header("Referencias UI - Mensajes Centrales")]
    public TextMeshProUGUI textoEstado;

    [Header("Referencias UI - Menú Fin de Partida")]
    public GameObject panelFinPartida;
    public TextMeshProUGUI textoTituloFinPartida;

    [Header("Referencias Entidades")]
    public SaludJugador saludJugador;
    public ZonaExtraccion zonaExtraccion;
    public InventarioJugador inventarioJugador;

    private ArmaBase armaEquipada;
    private bool partidaFinalizada = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (rellenoBarraVida != null)
        {
            anchoMaximoBarra = rellenoBarraVida.rectTransform.sizeDelta.x;
        }

        if (panelFinPartida != null)
        {
            panelFinPartida.SetActive(false);
        }

        // Cambio aquí: Usa FindAnyObjectByType en lugar de FindObjectOfType
        if (saludJugador == null)
        {
            saludJugador = Object.FindAnyObjectByType<SaludJugador>();
        }

        if (inventarioJugador == null && saludJugador != null)
        {
            inventarioJugador = saludJugador.GetComponent<InventarioJugador>();
        }
    }

    void Update()
    {
        ActualizarBarraVida();
        ActualizarTextoMunicion();
        ActualizarInventarioUI();

        if (!partidaFinalizada)
        {
            ComprobarEstadoPartida();
        }
    }

    void ActualizarBarraVida()
    {
        if (saludJugador == null || rellenoBarraVida == null) return;

        float porcentajeVida = Mathf.Clamp01(saludJugador.vidaActual / saludJugador.vidaMaxima);
        float nuevoAncho = anchoMaximoBarra * porcentajeVida;
        rellenoBarraVida.rectTransform.sizeDelta = new Vector2(nuevoAncho, rellenoBarraVida.rectTransform.sizeDelta.y);

        if (porcentajeVida < 0.3f)
            rellenoBarraVida.color = new Color(0.9f, 0.2f, 0.2f);
        else
            rellenoBarraVida.color = new Color(0.2f, 0.8f, 0.2f);
    }

    void ActualizarTextoMunicion()
    {
        if (textoMunicion == null) return;

        if (armaEquipada == null || !armaEquipada.gameObject.activeInHierarchy)
        {
            if (saludJugador != null)
            {
                armaEquipada = saludJugador.GetComponentInChildren<ArmaBase>(false);
            }
            else
            {
                // Cambio aquí: Usa FindAnyObjectByType
                armaEquipada = Object.FindAnyObjectByType<ArmaBase>();
            }
        }

        if (armaEquipada != null && armaEquipada.gameObject.activeInHierarchy)
        {
            textoMunicion.text = "<b>" + armaEquipada.municionCargador + "</b> / " + armaEquipada.municionReserva;
        }
        else
        {
            textoMunicion.text = "-- / --";
        }
    }

    void ActualizarInventarioUI()
    {
        if (inventarioJugador == null) return;

        if (textoBotiquines != null)
        {
            textoBotiquines.text = $"Botiquines [1]: <b>{inventarioJugador.botiquines}</b>";
        }

        if (textoSuministros != null)
        {
            textoSuministros.text = $"Suministros: <b>{inventarioJugador.suministrosMision}</b>";
        }
    }

    void ComprobarEstadoPartida()
    {
        if (saludJugador != null && saludJugador.EstaMuerto())
        {
            MostrarFinPartida("<color=red>¡HAS MUERTO!</color>");
            return;
        }

        if (zonaExtraccion == null) return;

        if (zonaExtraccion.TiempoRestante <= 0f && !zonaExtraccion.ExtraccionActivada)
        {
            MostrarFinPartida("<color=green>¡EXTRACCIÓN EXITOSA!</color>");
            return;
        }

        if (zonaExtraccion.ExtraccionActivada)
        {
            if (textoEstado != null)
            {
                textoEstado.gameObject.SetActive(true);
                int segundos = Mathf.CeilToInt(zonaExtraccion.TiempoRestante);
                textoEstado.text = $"<color=yellow><b>EVACUANDO...</b>\nResiste {segundos}s</color>";
            }
            return;
        }

        if (zonaExtraccion.JugadorEnZona)
        {
            if (textoEstado != null)
            {
                textoEstado.gameObject.SetActive(true);

                InventarioJugador inv = saludJugador != null ? saludJugador.GetComponent<InventarioJugador>() : null;
                if (inv != null && !inv.TieneSuministrosSuficientes())
                {
                    textoEstado.text = "<color=orange><b>NECESITAS BOTÍN</b>\nRecoge al menos 1 caja de suministros primero.</color>";
                }
                else
                {
                    textoEstado.text = "Presiona <b>[E]</b> para solicitar la extracción";
                }
            }
            return;
        }

        if (textoEstado != null)
        {
            textoEstado.gameObject.SetActive(false);
        }
    }

    void MostrarFinPartida(string mensaje)
    {
        partidaFinalizada = true;

        if (textoEstado != null)
            textoEstado.gameObject.SetActive(false);

        if (panelFinPartida != null)
        {
            panelFinPartida.SetActive(true);

            if (textoTituloFinPartida != null)
            {
                textoTituloFinPartida.text = mensaje;
            }
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }
}