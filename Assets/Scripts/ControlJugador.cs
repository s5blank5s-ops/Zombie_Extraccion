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

    [Header("Arma Equipada")]
    public GameObject modeloPistolaEnMano;
    public ArmaBase pistolaEquipada;

    [Header("Audio de Pasos")]
    public float cadenciaPasosCaminar = 0.5f;
    public float cadenciaPasosCorrer = 0.3f;
    private float contadorPasos = 0f;

    private float velActual;
    private float nivelRuido = 1f;
    private bool estaAgachado = false;
    private bool tieneArma = false;

    private Rigidbody rb;
    private Camera camaraPrincipal;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camaraPrincipal = Camera.main;
        velActual = velCaminar;
    }

    void Update()
    {
        ProcesarAgacharse();
        RotarHaciaRaton();
        ControlarDisparo();

        // Comprobamos el movimiento para el sonido de los pasos
        float entradaX = Input.GetAxisRaw("Horizontal");
        float entradaZ = Input.GetAxisRaw("Vertical");
        bool estaMoviendose = new Vector3(entradaX, 0f, entradaZ).sqrMagnitude > 0.01f;

        ControlarSonidoPasos(estaMoviendose);
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

                // Al correr enviamos una onda directa a los zombis en un radio de 10 metros
                AlertarZombisPorPasos(10f);
            }
            else
            {
                velActual = velCaminar;
                nivelRuido = 1.8f;

                // Al caminar emitimos ruido a 4 metros a la redonda
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

    public void EquiparArma()
    {
        tieneArma = true;
        if (modeloPistolaEnMano != null)
        {
            modeloPistolaEnMano.SetActive(true);
        }
        Debug.Log("¡Arma equipada!");
    }

    private void ControlarDisparo()
    {
        if (tieneArma && Input.GetMouseButtonDown(0))
        {
            if (pistolaEquipada != null)
            {
                pistolaEquipada.IntentarDisparar();
            }
        }
    }

    // Emite ruido de disparo en un radio grande
    public void GenerarRuidoDisparo(float radioRuido)
    {
        AlertarZombisPorPasos(radioRuido);
    }

    // Método auxiliar para avisar a la IA
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