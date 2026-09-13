using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InventarioMunicion;

public class ArmaBase : MonoBehaviour
{
    public enum TipoModoDisparo { Semiautomatico, Automatico, Escopeta }
    public enum TipoRanuraArma { Principal, Secundaria } // <-- AÑADIDO: Clasificación de ranuras

    [Header("Estado del Arma")]
    public bool enElSuelo = true; // Si es true, ignora las entradas de disparo

    [Header("Tipo de Arma")]
    public string nombreArma = "Arma";
    public TipoModoDisparo modoDisparo = TipoModoDisparo.Semiautomatico;
    public TipoRanuraArma ranuraArma = TipoRanuraArma.Principal; // <-- AÑADIDO: Definición explícita
    public InventarioMunicion.TipoMunicion tipoMunicion = InventarioMunicion.TipoMunicion.Pistola;

    [Header("Configuración de Disparo")]
    public float dañoPorBala = 25f;
    public float alcance = 20f;
    public float cadenciaDisparo = 0.3f; // Tiempo entre disparos
    public int perdigonesPorDisparo = 8;  // Solo se usa si es modo Escopeta
    public float dispersionEscopeta = 0.1f; // Apertura de la perdigonada

    [Header("Munición y Recarga")]
    public int municionCargador = 12;
    public int capacidadCargador = 12;
    public int municionReserva = 36;
    public float tiempoRecarga = 1.5f;

    [Header("Referencias y Efectos")]
    public Transform puntoDisparo;
    public LayerMask capasImpacto;
    public TrailRenderer rastroBalaPrefab;

    [Header("Efectos de Audio")]
    public AudioClip sonidoDisparoCustom; // Opcional si quieres sonidos únicos por arma

    [Header("Dispersión y Apuntado")]
    public float dispersionCadera = 0.15f;    // Dispersión sin apuntar (cadera)
    public float dispersionApuntando = 0.02f; // Dispersión casi nula al apuntar (precisión)

    private float tiempoSiguienteDisparo = 0f;
    private bool estaRecargando = false;
    private ControlJugador scriptJugador;

    void Start()
    {
        BuscarReferenciaJugador();
    }

    void Update()
    {
        // Si el arma está en el suelo, no procesamos ni disparo ni recarga desde este script
        if (enElSuelo) return;

        // Comprobación constante por si cambia de padre al equiparse
        if (scriptJugador == null)
        {
            BuscarReferenciaJugador();
        }

        // Recarga automática al pulsar R cuando está equipada
        if (Input.GetKeyDown(KeyCode.R) && !estaRecargando && municionCargador < capacidadCargador && municionReserva > 0)
        {
            StartCoroutine(Recargar());
        }
    }

    private void BuscarReferenciaJugador()
    {
        scriptJugador = GetComponentInParent<ControlJugador>();
        if (scriptJugador == null)
        {
            GameObject jugadorObj = GameObject.Find("Player");
            if (jugadorObj != null)
                scriptJugador = jugadorObj.GetComponent<ControlJugador>();
        }
    }

    public void IntentarDisparar()
    {
        if (estaRecargando || Time.time < tiempoSiguienteDisparo) return;

        if (municionCargador > 0)
        {
            if (modoDisparo == TipoModoDisparo.Escopeta)
            {
                DispararEscopeta();
            }
            else
            {
                DispararProyectilUnico();
            }

            tiempoSiguienteDisparo = Time.time + cadenciaDisparo;
        }
        else
        {
            // Sonido sin balas
            if (ControlAudio.Instancia != null)
            {
                float volSFX = PlayerPrefs.GetFloat("VolumenSFX", 1.0f);
                ControlAudio.Instancia.ReproducirSonido(ControlAudio.Instancia.audioSinBalas, volSFX);
            }

            tiempoSiguienteDisparo = Time.time + cadenciaDisparo;
        }
    }

    // --- DISPARO ESTÁNDAR (Pistola / Rifle) ---
    void DispararProyectilUnico()
    {
        ReproducirSonidoDisparo();
        municionCargador--;

        if (scriptJugador == null) BuscarReferenciaJugador();
        if (scriptJugador != null)
        {
            scriptJugador.GenerarRuidoDisparo(15f);
        }

        Vector3 origen = puntoDisparo != null ? puntoDisparo.position : transform.position;
        Vector3 direccionBase = puntoDisparo != null ? puntoDisparo.forward : transform.forward;

        // Determinar si el jugador está apuntando
        bool apuntando = scriptJugador != null && scriptJugador.estaApuntando;
        float factorDispersion = apuntando ? dispersionApuntando : dispersionCadera;

        // Aplicar la desviación de la bala
        Vector3 dispersion = new Vector3(
            Random.Range(-factorDispersion, factorDispersion),
            0f, // Mantener nivelado en el eje vertical en juegos 2D/Top-Down
            Random.Range(-factorDispersion, factorDispersion)
        );

        Vector3 direccionFinal = (direccionBase + dispersion).normalized;
        Vector3 puntoDestino;

        if (Physics.Raycast(origen, direccionFinal, out RaycastHit hit, alcance, capasImpacto))
        {
            puntoDestino = hit.point;

            SaludZombi zombi = hit.collider.GetComponentInParent<SaludZombi>();
            if (zombi != null)
            {
                zombi.RecibirDanio(dañoPorBala);
            }
        }
        else
        {
            puntoDestino = origen + direccionFinal * alcance;
        }

        if (rastroBalaPrefab != null)
        {
            StartCoroutine(DibujarRastroBala(origen, puntoDestino));
        }
    }
    // --- DISPARO DE ESCOPETA ---
    void DispararEscopeta()
    {
        ReproducirSonidoDisparo();
        municionCargador--;

        if (scriptJugador == null) BuscarReferenciaJugador();
        if (scriptJugador != null)
        {
            scriptJugador.GenerarRuidoDisparo(25f);
        }

        Vector3 origen = puntoDisparo != null ? puntoDisparo.position : transform.position;
        Vector3 direccionBase = puntoDisparo != null ? puntoDisparo.forward : transform.forward;

        bool apuntando = scriptJugador != null && scriptJugador.estaApuntando;
        float factorDispersion = apuntando ? (dispersionEscopeta * 0.4f) : dispersionEscopeta;

        for (int i = 0; i < perdigonesPorDisparo; i++)
        {
            Vector3 dispersion = new Vector3(
                Random.Range(-factorDispersion, factorDispersion),
                0f,
                Random.Range(-factorDispersion, factorDispersion)
            );

            Vector3 direccionFinal = (direccionBase + dispersion).normalized;
            Vector3 puntoDestino;

            if (Physics.Raycast(origen, direccionFinal, out RaycastHit hit, alcance, capasImpacto))
            {
                puntoDestino = hit.point;

                SaludZombi zombi = hit.collider.GetComponentInParent<SaludZombi>();
                if (zombi != null)
                {
                    zombi.RecibirDanio(dañoPorBala);
                }
            }
            else
            {
                puntoDestino = origen + direccionFinal * alcance;
            }

            if (rastroBalaPrefab != null)
            {
                StartCoroutine(DibujarRastroBala(origen, puntoDestino));
            }
        }
    }

    void ReproducirSonidoDisparo()
    {
        float volSFX = PlayerPrefs.GetFloat("VolumenSFX", 1.0f);

        if (sonidoDisparoCustom != null)
        {
            ControlAudio.Instancia.ReproducirSonido(sonidoDisparoCustom, volSFX);
        }
        else if (ControlAudio.Instancia != null)
        {
            ControlAudio.Instancia.ReproducirSonido(ControlAudio.Instancia.audioDisparo, volSFX);
        }
    }

    IEnumerator DibujarRastroBala(Vector3 inicio, Vector3 fin)
    {
        TrailRenderer rastro = Instantiate(rastroBalaPrefab, inicio, Quaternion.identity);
        float velocidad = 250f;
        float distancia = Vector3.Distance(inicio, fin);

        if (distancia > 0f)
        {
            float tiempoTotal = distancia / velocidad;
            float tiempoTranscurrido = 0f;

            while (tiempoTranscurrido < tiempoTotal)
            {
                if (rastro == null) break;
                rastro.transform.position = Vector3.Lerp(inicio, fin, tiempoTranscurrido / tiempoTotal);
                tiempoTranscurrido += Time.deltaTime;
                yield return null;
            }
        }

        if (rastro != null)
        {
            rastro.transform.position = fin;
            Destroy(rastro.gameObject, rastro.time);
        }
    }

    IEnumerator Recargar()
    {
        if (InventarioMunicion.Instancia == null) yield break;

        int reservaDisponible = InventarioMunicion.Instancia.ObtenerReserva(tipoMunicion);
        int balasNecesarias = capacidadCargador - municionCargador;

        // Si no necesita balas o no hay reserva disponible, se cancela
        if (balasNecesarias <= 0 || reservaDisponible <= 0) yield break;

        estaRecargando = true;

        AudioSource audioRecargaTemp = gameObject.AddComponent<AudioSource>();

        if (ControlAudio.Instancia != null && ControlAudio.Instancia.audioRecargar != null)
        {
            float volSFX = PlayerPrefs.GetFloat("VolumenSFX", 1.0f);
            audioRecargaTemp.clip = ControlAudio.Instancia.audioRecargar;
            audioRecargaTemp.volume = volSFX;
            audioRecargaTemp.Play();
        }

        yield return new WaitForSeconds(tiempoRecarga);

        if (audioRecargaTemp != null)
        {
            if (audioRecargaTemp.isPlaying) audioRecargaTemp.Stop();
            Destroy(audioRecargaTemp);
        }

        // Calculamos cuántas balas tomar de la reserva global
        int balasARecargar = Mathf.Min(balasNecesarias, reservaDisponible);

        municionCargador += balasARecargar;
        InventarioMunicion.Instancia.RestarMunicion(tipoMunicion, balasARecargar);

        estaRecargando = false;
    }
}