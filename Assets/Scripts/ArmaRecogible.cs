using UnityEngine;

public class ArmaRecogible : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreArma = "Pistola 9mm";
    public float distanciaRecogida = 2.5f;

    private Transform jugador;
    private bool jugadorCerca = false;

    void Start()
    {
        GameObject jugadorObj = GameObject.Find("Player");
        if (jugadorObj != null) jugador = jugadorObj.transform;
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaRecogida)
        {
            if (!jugadorCerca)
            {
                jugadorCerca = true;
                Debug.Log("Presiona [E] para recoger " + nombreArma);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                RecogerArma();
            }
        }
        else
        {
            jugadorCerca = false;
        }
    }

    void RecogerArma()
    {
        // Le avisamos al jugador que ha equipado el arma
        ControlJugador control = jugador.GetComponent<ControlJugador>();
        if (control != null)
        {
            control.EquiparArma();
            Destroy(gameObject); // Eliminamos el objeto tirado en el suelo
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaRecogida);
    }
}