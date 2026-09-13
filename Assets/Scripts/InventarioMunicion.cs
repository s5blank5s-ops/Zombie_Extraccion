using UnityEngine;

public class InventarioMunicion : MonoBehaviour
{
    public enum TipoMunicion { Pistola, Escopeta, Rifle }

    [Header("Reservas de Munición")]
    public int reservaPistola = 60;
    public int reservaEscopeta = 24;
    public int reservaRifle = 120;

    public static InventarioMunicion Instancia;

    private void Awake()
    {
        if (Instancia == null)
            Instancia = this;
        else
            Destroy(gameObject);
    }

    public int ObtenerReserva(TipoMunicion tipo)
    {
        switch (tipo)
        {
            case TipoMunicion.Pistola: return reservaPistola;
            case TipoMunicion.Escopeta: return reservaEscopeta;
            case TipoMunicion.Rifle: return reservaRifle;
            default: return 0;
        }
    }

    public void RestarMunicion(TipoMunicion tipo, int cantidad)
    {
        switch (tipo)
        {
            case TipoMunicion.Pistola:
                reservaPistola = Mathf.Max(0, reservaPistola - cantidad);
                break;
            case TipoMunicion.Escopeta:
                reservaEscopeta = Mathf.Max(0, reservaEscopeta - cantidad);
                break;
            case TipoMunicion.Rifle:
                reservaRifle = Mathf.Max(0, reservaRifle - cantidad);
                break;
        }
    }

    public void AgregarMunicion(TipoMunicion tipo, int cantidad)
    {
        switch (tipo)
        {
            case TipoMunicion.Pistola: reservaPistola += cantidad; break;
            case TipoMunicion.Escopeta: reservaEscopeta += cantidad; break;
            case TipoMunicion.Rifle: reservaRifle += cantidad; break;
        }
    }
}