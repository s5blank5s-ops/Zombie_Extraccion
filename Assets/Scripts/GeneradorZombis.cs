using UnityEngine;

public class GeneradorZombis : MonoBehaviour
{
    [Header("Configuración del Generador")]
    public GameObject prefabZombi;
    public float intervaloAparicion = 2f;
    public Transform[] puntosAparicion;

    private bool generando = false;
    private float tiempoSiguienteAparicion = 0f;

    void Update()
    {
        if (!generando || prefabZombi == null || puntosAparicion.Length == 0) return;

        if (Time.time >= tiempoSiguienteAparicion)
        {
            GenerarZombi();
            tiempoSiguienteAparicion = Time.time + intervaloAparicion;
        }
    }

    public void IniciarGeneracion()
    {
        generando = true;
        tiempoSiguienteAparicion = Time.time;
    }

    public void DetenerGeneracion()
    {
        generando = false;
    }

    void GenerarZombi()
    {
        Transform puntoElegido = puntosAparicion[Random.Range(0, puntosAparicion.Length)];
        GameObject nuevoZombi = Instantiate(prefabZombi, puntoElegido.position, puntoElegido.rotation);

        // Activamos la persecución directa SOLO en este nuevo zombi del spawn
        ZombiIA scriptZombi = nuevoZombi.GetComponent<ZombiIA>();
        if (scriptZombi != null)
        {
            scriptZombi.ActivarModoHorda();
        }
    }
}