using UnityEngine;

public class ControlJugador : MonoBehaviour
{
    [Header("Velocidades")]
    public float velAgachado = 2.5f;
    public float velCaminar = 5f;
    public float velCorrer = 8f;

    [Header("Ajustes de Agacharse")]
    public float alturaDePie = 2f;
    public float alturaAgachado = 1f;

    [Header("Armas y Anclaje")]
    public Transform puntoAnclajeArma; // Objeto PuntoArma
    public ArmaBase armaPrincipal;    // Ranura 1
    public ArmaBase armaSecundaria;   // Ranura 2
    public ArmaBase armaActual;       // Arma activa

    [Header("Ajustes de Apuntado y Cámara")]
    public bool estaApuntando = false;
    public float fovNormal = 60f;        // FOV base de la cámara
    public float fovApuntando = 45f;     // FOV con zoom al apuntar
    public float offsetAvanceCamara = 2f; // Desplazamiento sutil hacia la mira
    public float velocidadTransicionCamara = 8f;

    [Header("Audio de Pasos")]
    public float cadenciaPasosCaminar = 0.5f;
    public float cadenciaPasosCorrer = 0.3f;
    private float contadorPasos = 0f;

    private float velActual;
    private float nivelRuido = 1f;
    private bool estaAgachado = false;

    private Rigidbody rb;
    private Camera camaraPrincipal;
    private Vector3 offsetCamaraOriginal;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camaraPrincipal = Camera.main;
        velActual = velCaminar;

        if (camaraPrincipal != null)
        {
            fovNormal = camaraPrincipal.fieldOfView;
            // Guardamos la posición relativa inicial respecto al jugador
            offsetCamaraOriginal = camaraPrincipal.transform.position - transform.position;
        }
    }

    void Update()
    {
        ProcesarAgacharse();
        RotarHaciaRaton();
        ProcesarApuntado();
        ControlarDisparo();
        ControlarCambioArma();

        float entradaX = Input.GetAxisRaw("Horizontal");
        float entradaZ = Input.GetAxisRaw("Vertical");
        bool estaMoviendose = new Vector3(entradaX, 0f, entradaZ).sqrMagnitude > 0.01f;

        ControlarSonidoPasos(estaMoviendose);
    }

    void LateUpdate()
    {
        ActualizarPosicionCamara();
    }

    void FixedUpdate()
    {
        Mover();
    }

    void ProcesarAgacharse()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
        {
            estaAgachado = !estaAgachado;

            if (estaAgachado)
                transform.localScale = new Vector3(transform.localScale.x, 0.6f, transform.localScale.z);
            else
                transform.localScale = new Vector3(transform.localScale.x, 1f, transform.localScale.z);
        }
    }

    void Mover()
    {
        float entradaX = Input.GetAxisRaw("Horizontal");
        float entradaZ = Input.GetAxisRaw("Vertical");

        Vector3 direccion = new Vector3(entradaX, 0f, entradaZ).normalized;

        if (direccion.magnitude > 0.1f)
        {
            if (estaAgachado)
            {
                velActual = velAgachado;
                nivelRuido = 0.2f;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                velActual = velCorrer;
                nivelRuido = 3.5f;
                AlertarZombisPorPasos(10f);
            }
            else
            {
                velActual = velCaminar;
                nivelRuido = 1.8f;
                AlertarZombisPorPasos(4f);
            }
        }
        else
        {
            nivelRuido = 0.1f;
        }

        rb.MovePosition(rb.position + direccion * velActual * Time.fixedDeltaTime);
    }

    void ControlarSonidoPasos(bool estaMoviendose)
    {
        if (!estaMoviendose || estaAgachado) return;

        contadorPasos -= Time.deltaTime;

        if (contadorPasos <= 0f)
        {
            float intervalo = Input.GetKey(KeyCode.LeftShift) ? cadenciaPasosCorrer : cadenciaPasosCaminar;
            float volumen = Input.GetKey(KeyCode.LeftShift) ? 0.8f : 0.4f;

            if (ControlAudio.Instancia != null)
            {
                ControlAudio.Instancia.ReproducirSonido(ControlAudio.Instancia.audioPasos, volumen);
            }

            contadorPasos = intervalo;
        }
    }

    void RotarHaciaRaton()
    {
        Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
        Plane planoSuelo = new Plane(Vector3.up, Vector3.zero);

        if (planoSuelo.Raycast(rayo, out float distancia))
        {
            Vector3 puntoImpacto = rayo.GetPoint(distancia);
            Vector3 direccionMira = puntoImpacto - transform.position;
            direccionMira.y = 0;

            if (direccionMira != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direccionMira);
            }
        }
    }

    private void ProcesarApuntado()
    {
        estaApuntando = Input.GetMouseButton(1);

        if (camaraPrincipal != null)
        {
            // Transición suave del Zoom mediante Field of View
            float fovObjetivo = estaApuntando ? fovApuntando : fovNormal;
            camaraPrincipal.fieldOfView = Mathf.Lerp(
                camaraPrincipal.fieldOfView,
                fovObjetivo,
                Time.deltaTime * velocidadTransicionCamara
            );
        }
    }

    private void ActualizarPosicionCamara()
    {
        if (camaraPrincipal == null) return;

        // Mantenemos la cámara centrada exactamente en la posición del jugador
        Vector3 posicionBase = transform.position + offsetCamaraOriginal;

        if (estaApuntando)
        {
            // Añadimos un leve desplazamiento frontal hacia la dirección donde apunta el personaje
            Vector3 avanceApuntado = transform.forward * offsetAvanceCamara;
            posicionBase += avanceApuntado;
        }

        // Seguimiento suave y centrado sin perder la referencia del jugador
        camaraPrincipal.transform.position = Vector3.Lerp(
            camaraPrincipal.transform.position,
            posicionBase,
            Time.deltaTime * velocidadTransicionCamara
        );
    }

    // --- DISPARO Y RECARGA ---
    private void ControlarDisparo()
    {
        if (armaActual == null || !armaActual.gameObject.activeSelf) return;

        if (armaActual.modoDisparo == ArmaBase.TipoModoDisparo.Automatico)
        {
            if (Input.GetMouseButton(0))
            {
                armaActual.IntentarDisparar();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                armaActual.IntentarDisparar();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            armaActual.StartCoroutine("Recargar");
        }
    }

    // --- CAMBIO DE ARMAS (1 Y 2) ---
    private void ControlarCambioArma()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && armaPrincipal != null)
        {
            SeleccionarArma(armaPrincipal);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && armaSecundaria != null)
        {
            SeleccionarArma(armaSecundaria);
        }
    }

    private void SeleccionarArma(ArmaBase nuevaArmaEnMano)
    {
        if (nuevaArmaEnMano == null) return;

        if (armaPrincipal != null) armaPrincipal.gameObject.SetActive(false);
        if (armaSecundaria != null) armaSecundaria.gameObject.SetActive(false);

        armaActual = nuevaArmaEnMano;
        armaActual.gameObject.SetActive(true);
    }

    // --- RECOGIDA Y SOLTADO DE ARMAS ---
    public void EquiparNuevaArma(ArmaBase nuevaArma)
    {
        nuevaArma.enabled = true;

        if (nuevaArma.ranuraArma == ArmaBase.TipoRanuraArma.Secundaria)
        {
            if (armaSecundaria != null)
            {
                SoltarArmaAlSuelo(armaSecundaria);
            }
            armaSecundaria = nuevaArma;
        }
        else
        {
            if (armaPrincipal != null)
            {
                SoltarArmaAlSuelo(armaPrincipal);
            }
            armaPrincipal = nuevaArma;
        }

        if (puntoAnclajeArma != null)
        {
            nuevaArma.transform.SetParent(puntoAnclajeArma);
            nuevaArma.transform.localPosition = Vector3.zero;
            nuevaArma.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        nuevaArma.enElSuelo = false;

        ArmaRecogible recogible = nuevaArma.GetComponent<ArmaRecogible>();
        if (recogible != null) Destroy(recogible);

        Collider[] colliders = nuevaArma.GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }

        SeleccionarArma(nuevaArma);
    }

    private void SoltarArmaAlSuelo(ArmaBase armaASoltar)
    {
        armaASoltar.transform.SetParent(null);
        armaASoltar.transform.position = transform.position + transform.forward * 0.8f + Vector3.up * 0.1f;
        armaASoltar.transform.rotation = Quaternion.identity;
        armaASoltar.gameObject.SetActive(true);

        armaASoltar.enElSuelo = true;

        Collider[] colliders = armaASoltar.GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = true;
        }

        if (armaASoltar.GetComponent<ArmaRecogible>() == null)
        {
            ArmaRecogible nuevoRecogible = armaASoltar.gameObject.AddComponent<ArmaRecogible>();
            nuevoRecogible.distanciaRecogida = 2.5f;
        }
    }

    public void GenerarRuidoDisparo(float radioRuido)
    {
        AlertarZombisPorPasos(radioRuido);
    }

    private void AlertarZombisPorPasos(float radio)
    {
        Collider[] enemigosCercanos = Physics.OverlapSphere(transform.position, radio);
        foreach (var col in enemigosCercanos)
        {
            ZombiIA zombi = col.GetComponent<ZombiIA>();
            if (zombi != null)
            {
                zombi.AlertaPorRuido(transform.position);
            }
        }
    }

    public float ObtenerNivelRuido()
    {
        return nivelRuido;
    }
}