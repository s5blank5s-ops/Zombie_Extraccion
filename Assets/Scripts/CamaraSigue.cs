using UnityEngine;

public class CamaraSigue : MonoBehaviour
{
    public Transform objetivo;
    public Vector3 offset = new Vector3(0f, 10f, -10f);
    public float suavizado = 5f;

    void LateUpdate()
    {
        if (objetivo == null) return;

        Vector3 posicionDeseada = objetivo.position + offset;
        Vector3 posicionSuave = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);
        transform.position = posicionSuave;
    }
}
