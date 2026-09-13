using UnityEngine;
public class ArmaRecogible : MonoBehaviour
{
    public float distanciaRecogida = 2.5f;

    private Transform jugador;
    private ArmaBase armaBase;
    private bool jugadorCerca = false;

    void Start()
    {
        GameObject jugadorObj = GameObject.Find("Player");
        if (jugadorObj != null) jugador = jugadorObj.transform;

        armaBase = GetComponent<ArmaBase>();
    }

    void Update()
    {
        if (jugador == null || armaBase == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaRecogida)
        {
            if (!jugadorCerca)
            {
                jugadorCerca = true;
                Debug.Log("Presiona [E] para recoger " + armaBase.nombreArma);
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
        ControlJugador control = jugador.GetComponent<ControlJugador>();
        if (control != null)
        {
            control.EquiparNuevaArma(armaBase);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaRecogida);
    }
}