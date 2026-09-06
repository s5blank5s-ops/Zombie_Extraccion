using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Configuración de Escena")]
    public string nombreEscenaJuego = "SampleScene";

    [Header("Paneles de la UI")]
    public GameObject panelMenuPrincipal;
    public GameObject panelOpciones;

    [Header("Referencia de Audio del Menú")]
    public AudioSource musicaMenu; // Arrastra aquí el GameObject 'MusicaMenu'

    [Header("UI Audio Controls")]
    public Toggle toggleMusica;
    public Slider sliderMusica;
    public Slider sliderSFX;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelOpciones != null) panelOpciones.SetActive(false);

        CargarAjustesAudio();
    }

    void CargarAjustesAudio()
    {
        // 1. Música activada/desactivada
        bool musicaActivada = PlayerPrefs.GetInt("MusicaActivada", 1) == 1;
        if (toggleMusica != null)
        {
            toggleMusica.isOn = musicaActivada;
            toggleMusica.onValueChanged.RemoveAllListeners();
            toggleMusica.onValueChanged.AddListener(CambiarEstadoMusica);
        }

        // Aplicar estado inicial
        if (musicaMenu != null) musicaMenu.mute = !musicaActivada;

        // 2. Volumen de música
        float volMusica = PlayerPrefs.GetFloat("VolumenMusica", 1.0f);
        if (sliderMusica != null)
        {
            sliderMusica.value = volMusica;
            sliderMusica.onValueChanged.RemoveAllListeners();
            sliderMusica.onValueChanged.AddListener(CambiarVolumenMusica);
        }

        // Aplicar volumen inicial
        if (musicaMenu != null) musicaMenu.volume = volMusica;

        // 3. Volumen de SFX
        float volSFX = PlayerPrefs.GetFloat("VolumenSFX", 1.0f);
        if (sliderSFX != null)
        {
            sliderSFX.value = volSFX;
            sliderSFX.onValueChanged.RemoveAllListeners();
            sliderSFX.onValueChanged.AddListener(CambiarVolumenSFX);
        }
    }

    public void CambiarEstadoMusica(bool activada)
    {
        PlayerPrefs.SetInt("MusicaActivada", activada ? 1 : 0);
        if (musicaMenu != null)
        {
            musicaMenu.mute = !activada;
        }
    }

    public void CambiarVolumenMusica(float valor)
    {
        PlayerPrefs.SetFloat("VolumenMusica", valor);
        if (musicaMenu != null)
        {
            musicaMenu.volume = valor;
        }
    }

    public void CambiarVolumenSFX(float valor)
    {
        PlayerPrefs.SetFloat("VolumenSFX", valor);
    }

    public void Jugar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void AbrirOpciones()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(true);
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}