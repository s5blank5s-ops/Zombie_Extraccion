using UnityEngine;
using UnityEngine.AI;

public class ZombiIA : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;

    [Header("Configuración de Persecución")]
    public bool perseguirSiempre = false;
    public float velocidadHorda = 6.0f;
    public float tiempoMemoriaPersecucion = 4.0f; // Segundos que te sigue buscando tras perderte de vista

    [Header("Detección por Visión")]
    public float rangoVision = 12f;
    public float anguloVision = 60f;
    public LayerMask capasObstaculos;

    [Header("Detección por Oído")]
    public float rangoOidoBase = 4f;

    [Header("Ataque Cuerpo a Cuerpo")]
    public float danioAtaque = 20f;
    public float distanciaAtaque = 1.5f;
    public float cadenciaAtaque = 1.2f;

    private NavMeshAgent agente;
    private ControlJugador scriptJugador;
    private float tiempoSiguienteAtaque = 0f;
    private float tiempoUltimaDeteccion = -999f; // Registra cuándo te vio/oyó por última vez
    private bool detectado = false;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        BuscarJugador();

        if (jugador != null && perseguirSiempre && agente != null)
        {
            agente.SetDestination(jugador.position);
        }
    }

    void Update()
    {
        if (jugador == null)
        {
            BuscarJugador();
            if (jugador == null) return;
        }

        float distancia = Vector3.Distance(transform.position, jugador.position);

        // 1. Comprobar si lo ve o lo oye en este frame
        bool loVe = PuedeVerJugador();
        bool loOye = PuedeOirJugador();

        if (loVe || loOye)
        {
            detectado = true;
            tiempoUltimaDeteccion = Time.time;
        }

        // Si ha pasado el tiempo de memoria sin verlo ni oírlo, pierde el rastro
        if (!perseguirSiempre && Time.time - tiempoUltimaDeteccion > tiempoMemoriaPersecucion)
        {
            detectado = false;
        }

        // 2. Comportamiento según estado
        if (distancia <= distanciaAtaque)
        {
            if (agente != null) agente.ResetPath(); // Se detiene para atacar
            if (Time.time >= tiempoSiguienteAtaque)
            {
                AtacarJugador();
                tiempoSiguienteAtaque = Time.time + cadenciaAtaque;
            }
        }
        else if (perseguirSiempre || detectado)
        {
            // Si es de horda O si te ha detectado (y sigue en tiempo de memoria), te persigue
            if (agente != null) agente.SetDestination(jugador.position);
        }
    }

    void BuscarJugador()
    {
        if (jugador == null)
        {
            GameObject jugadorObj = GameObject.Find("Player");
            if (jugadorObj == null)
            {
                jugadorObj = GameObject.FindGameObjectWithTag("Player");
            }

            if (jugadorObj != null)
            {
                jugador = jugadorObj.transform;
                scriptJugador = jugador.GetComponent<ControlJugador>();
            }
        }
    }

    bool PuedeVerJugador()
    {
        Vector3 direccionJugador = (jugador.position - transform.position);
        float distancia = direccionJugador.magnitude;

        if (distancia <= rangoVision)
        {
            float angulo = Vector3.Angle(transform.forward, direccionJugador);
            if (angulo <= anguloVision / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, direccionJugador.normalized, distancia, capasObstaculos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool PuedeOirJugador()
    {
        if (scriptJugador == null) return false;

        float distancia = Vector3.Distance(transform.position, jugador.position);
        float ruidoActual = scriptJugador.ObtenerNivelRuido();
        float radioAudicion = rangoOidoBase * ruidoActual;

        // Si el jugador está MUY cerca (ej. pegado a su espalda a < 1.5m), lo oye aunque esté quieto
        if (distancia <= 1.5f) return true;

        return distancia <= radioAudicion;
    }

    void AtacarJugador()
    {
        SaludJugador salud = jugador.GetComponent<SaludJugador>();
        if (salud != null)
        {
            salud.RecibirDanio(danioAtaque);
        }
    }

    public void AlertaPorRuido(Vector3 posicionRuido)
    {
        detectado = true;
        tiempoUltimaDeteccion = Time.time;

        if (agente != null)
        {
            agente.SetDestination(posicionRuido);
        }
    }

    public void ActivarModoHorda()
    {
        perseguirSiempre = true;
        BuscarJugador();

        if (agente == null) agente = GetComponent<NavMeshAgent>();

        if (agente != null)
        {
            agente.speed = velocidadHorda;

            if (jugador != null)
            {
                agente.SetDestination(jugador.position);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoVision);
    }
}