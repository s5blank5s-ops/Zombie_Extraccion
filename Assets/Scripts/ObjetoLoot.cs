using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjetoLoot : MonoBehaviour
{
    // Añadimos 'Arma' a los tipos de loot
    public enum TipoLoot { Municion, Botiquin, SuministroMision, Arma }

    [Header("Configuración del Loot")]
    public TipoLoot tipo = TipoLoot.Municion;
    public string nombreObjeto = "Caja de Suministros";

    [Header("Ajustes por Tipo")]
    public int cantidadMunicion = 30;
    public float cantidadCuracion = 40f;

    // Identificador para saber qué arma entregamos (Ej: "Escopeta", "RifleAsalto", "Pistola")
    public string nombreIDArma = "Escopeta";

    [Header("Ajustes de Interacción y Rango")]
    public float radioInteraccion = 2.5f;
    public float tiempoParaAbrir = 3.0f;

    [Header("Efectos y Audio")]
    public GameObject efectoAlRecoger;
    public AudioClip sonidoBuscandoLoot; // Sonido mientras buscas (3s)
    public AudioClip sonidoExitoLoot;    // Sonido al terminar

    private bool abriendo = false;
    private float temporizadorApertura = 0f;
    private GameObject jugadorEncontrado;
    private AudioSource audioSourceLocal;

    void Start()
    {
        // Creamos un AudioSource local configurado para 2D limpio
        audioSourceLocal = gameObject.AddComponent<AudioSource>();
        audioSourceLocal.playOnAwake = false;
        audioSourceLocal.spatialBlend = 0f;

        // Cargar volumen de SFX configurado en Opciones
        float volSFX = PlayerPrefs.GetFloat("VolumenSFX", 1.0f);
        audioSourceLocal.volume = volSFX;
    }

    void Update()
    {
        bool jugadorEnRango = ComprobarJugadorCerca();

        if (jugadorEnRango)
        {
            if (Input.GetKeyDown(KeyCode.E) && !abriendo)
            {
                IniciarBusqueda();
            }

            if (abriendo)
            {
                // Si el jugador intenta moverse, cancelamos
                bool intentaMoverse = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

                if (intentaMoverse)
                {
                    CancelarApertura("Te has movido.");
                    return;
                }

                temporizadorApertura += Time.deltaTime;

                if (temporizadorApertura >= tiempoParaAbrir)
                {
                    CompletarBusqueda();
                }
            }
        }
        else
        {
            if (abriendo)
            {
                CancelarApertura("Te has alejado demasiado.");
            }
        }
    }

    private void IniciarBusqueda()
    {
        abriendo = true;
        temporizadorApertura = 0f;

        if (sonidoBuscandoLoot != null && audioSourceLocal != null)
        {
            audioSourceLocal.volume = PlayerPrefs.GetFloat("VolumenSFX", 1.0f);
            audioSourceLocal.Stop();
            audioSourceLocal.PlayOneShot(sonidoBuscandoLoot);
        }

        Debug.Log($"Abriendo/Recogiendo {nombreObjeto}... Quédate quieto ({tiempoParaAbrir}s)");
    }

    private void CancelarApertura(string motivo)
    {
        abriendo = false;
        temporizadorApertura = 0f;

        if (audioSourceLocal != null)
        {
            audioSourceLocal.Stop();
        }

        Debug.Log($"Interacción cancelada: {motivo}");
    }

    private void CompletarBusqueda()
    {
        if (audioSourceLocal != null)
        {
            audioSourceLocal.Stop();
        }

        if (sonidoExitoLoot != null)
        {
            Vector3 posCamara = Camera.main != null ? Camera.main.transform.position : transform.position;

            if (ControlAudio.Instancia != null)
            {
                ControlAudio.Instancia.ReproducirSonido3D(sonidoExitoLoot, posCamara);
            }
            else
            {
                float volSFX = PlayerPrefs.GetFloat("VolumenSFX", 1.0f);
                AudioSource.PlayClipAtPoint(sonidoExitoLoot, posCamara, volSFX);
            }
        }

        RecogerLoot();
    }

    private bool ComprobarJugadorCerca()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radioInteraccion);
        foreach (var col in hits)
        {
            if (col.CompareTag("Player") || col.transform.root.CompareTag("Player"))
            {
                jugadorEncontrado = col.transform.root.gameObject;
                return true;
            }
        }

        jugadorEncontrado = null;
        return false;
    }

    void RecogerLoot()
    {
        if (jugadorEncontrado == null) return;

        bool recogidoConExito = false;

        switch (tipo)
        {
            case TipoLoot.Municion:
                // Busca el arma que el jugador tiene equipada/activa en ese momento
                ArmaBase armaEquipada = jugadorEncontrado.GetComponentInChildren<ArmaBase>(false);
                if (armaEquipada != null)
                {
                    armaEquipada.municionReserva += cantidadMunicion;
                    recogidoConExito = true;
                }
                else
                {
                    Debug.LogWarning("No hay ningún arma activa en las manos del jugador para añadir munición.");
                }
                break;

            case TipoLoot.Botiquin:
                InventarioJugador inventarioBotiquin = jugadorEncontrado.GetComponentInChildren<InventarioJugador>(true);
                if (inventarioBotiquin != null)
                {
                    inventarioBotiquin.AgregarBotiquin(1);
                    recogidoConExito = true;
                }
                break;

            case TipoLoot.SuministroMision:
                InventarioJugador inventario = jugadorEncontrado.GetComponentInChildren<InventarioJugador>(true);
                if (inventario != null)
                {
                    inventario.AgregarSuministro(1);
                    recogidoConExito = true;
                }
                break;

            case TipoLoot.Arma:
                // Busca todas las armas que posee el jugador (incluso si están desactivadas)
                ArmaBase[] todasLasArmas = jugadorEncontrado.GetComponentsInChildren<ArmaBase>(true);
                bool armaEncontrada = false;

                foreach (ArmaBase arma in todasLasArmas)
                {
                    if (arma.nombreArma.Equals(nombreIDArma, System.StringComparison.OrdinalIgnoreCase) ||
                        arma.gameObject.name.Equals(nombreIDArma, System.StringComparison.OrdinalIgnoreCase))
                    {
                        armaEncontrada = true;

                        // Si el arma estaba desactivada (no la tenía desbloqueada), la activamos
                        if (!arma.gameObject.activeSelf)
                        {
                            // Desactivamos temporalmente el resto de armas para equipar esta
                            foreach (ArmaBase a in todasLasArmas) a.gameObject.SetActive(false);

                            arma.gameObject.SetActive(true);
                            Debug.Log($"¡Has recogido el arma: {arma.nombreArma}!");
                        }
                        else
                        {
                            // Si ya la tenía en las manos, le otorgamos munición extra
                            arma.municionReserva += arma.capacidadCargador * 2;
                            Debug.Log($"Ya tenías {arma.nombreArma}. Se ha añadido munición extra.");
                        }

                        recogidoConExito = true;
                        break;
                    }
                }

                if (!armaEncontrada)
                {
                    Debug.LogError($"No se encontró ningún objeto de arma en el jugador con el nombre: {nombreIDArma}");
                }
                break;
        }

        if (recogidoConExito)
        {
            if (efectoAlRecoger != null)
            {
                Instantiate(efectoAlRecoger, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}