using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmaBase : MonoBehaviour
{
    public enum TipoModoDisparo { Semiautomatico, Automatico, Escopeta }

    [Header("Tipo de Arma")]
    public string nombreArma = "Arma";
    public TipoModoDisparo modoDisparo = TipoModoDisparo.Semiautomatico;

    [Header("Configuración de Disparo")]
    public float dañoPorBala = 25f;
    public float alcance = 20f;
    public float cadenciaDisparo = 0.3f; // Tiempo entre disparos
    public int perdigonesPorDisparo = 8;  // Solo se usa si es modo Escopeta
    public float dispersionEscopeta = 0.1f; // Apertura del perdigonada

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

    private float tiempoSiguienteDisparo = 0f;
    private bool estaRecargando = false;
    private ControlJugador scriptJugador;

    void Start()
    {
        GameObject jugador = GameObject.Find("Player");
        if (jugador != null)
            scriptJugador = jugador.GetComponent<ControlJugador>();
    }

    void Update()
    {
        // Detectar entrada según el modo de disparo
        if (modoDisparo == TipoModoDisparo.Automatico)
        {
            if (Input.GetButton("Fire1")) // Mantener pulsado para Rifle de Asalto
            {
                IntentarDisparar();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1")) // Un solo clic para Pistola / Escopeta
            {
                IntentarDisparar();
            }
        }

        // Recarga
        if (Input.GetKeyDown(KeyCode.R) && !estaRecargando && municionCargador < capacidadCargador && municionReserva > 0)
        {
            StartCoroutine(Recargar());
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

        if (scriptJugador != null)
            scriptJugador.GenerarRuidoDisparo(15f);

        RaycastHit hit;
        Vector3 puntoDestino;

        if (Physics.Raycast(puntoDisparo.position, puntoDisparo.forward, out hit, alcance, capasImpacto))
        {
            puntoDestino = hit.point;

            SaludZombi zombi = hit.collider.GetComponent<SaludZombi>();
            if (zombi != null)
            {
                zombi.RecibirDanio(dañoPorBala);
            }
        }
        else
        {
            puntoDestino = puntoDisparo.position + puntoDisparo.forward * alcance;
        }

        if (rastroBalaPrefab != null)
        {
            StartCoroutine(DibujarRastroBala(puntoDisparo.position, puntoDestino));
        }
    }

    // --- DISPARO DE ESCOPETA (Múltiples perdigones con dispersión) ---
    void DispararEscopeta()
    {
        ReproducirSonidoDisparo();
        municionCargador--;

        if (scriptJugador != null)
            scriptJugador.GenerarRuidoDisparo(25f); // La escopeta atrae zombis desde más lejos

        for (int i = 0; i < perdigonesPorDisparo; i++)
        {
            // Calculamos un cono de dispersión aleatorio
            Vector3 direccionConDispersion = puntoDisparo.forward + new Vector3(
                Random.Range(-dispersionEscopeta, dispersionEscopeta),
                Random.Range(-dispersionEscopeta, dispersionEscopeta),
                Random.Range(-dispersionEscopeta, dispersionEscopeta)
            );

            RaycastHit hit;
            Vector3 puntoDestino;

            if (Physics.Raycast(puntoDisparo.position, direccionConDispersion, out hit, alcance, capasImpacto))
            {
                puntoDestino = hit.point;

                SaludZombi zombi = hit.collider.GetComponent<SaludZombi>();
                if (zombi != null)
                {
                    zombi.RecibirDanio(dañoPorBala);
                }
            }
            else
            {
                puntoDestino = puntoDisparo.position + direccionConDispersion * alcance;
            }

            if (rastroBalaPrefab != null)
            {
                StartCoroutine(DibujarRastroBala(puntoDisparo.position, puntoDestino));
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
        float velocidad = 200f;
        float distancia = Vector3.Distance(inicio, fin);
        float tiempoTotal = distancia / velocidad;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoTotal)
        {
            rastro.transform.position = Vector3.Lerp(inicio, fin, tiempoTranscurrido / tiempoTotal);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        rastro.transform.position = fin;
        Destroy(rastro.gameObject, rastro.time);
    }

    IEnumerator Recargar()
    {
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

        int balasNecesarias = capacidadCargador - municionCargador;
        int balasARecargar = Mathf.Min(balasNecesarias, municionReserva);

        municionCargador += balasARecargar;
        municionReserva -= balasARecargar;

        estaRecargando = false;
    }
}