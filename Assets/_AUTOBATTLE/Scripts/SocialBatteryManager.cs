using UnityEngine;
using UnityEngine.UI;

public class SocialBatteryManager : MonoBehaviour
{
    public static SocialBatteryManager Instance { get; private set; }

    [Header("Bateria Social")]
    public int bateriaSocial = 100;
    public int bateriaMaxima = 100;
    public Slider sliderBateriaSocial;
    public Image fillBateriaSocial;

    [Header("Regeneracion")]
    public int regeneracionCantidad = 1;
    public float regeneracionIntervalo = 0.5f;

    [Header("Colores")]
    public Color colorBajo = Color.red;
    public Color colorMedio = new Color(1f, 0.5f, 0f);
    public Color colorAlto = Color.green;

    private float tiempoRegeneracion;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        bateriaSocial = Mathf.Clamp(bateriaSocial, 0, bateriaMaxima);

        if (fillBateriaSocial == null && sliderBateriaSocial != null && sliderBateriaSocial.fillRect != null)
        {
            fillBateriaSocial = sliderBateriaSocial.fillRect.GetComponent<Image>();
        }

        ActualizarVisual();
    }

    private void Update()
    {
        RegenerarBateria();
        RevisarTeclasNumericas();
    }

    public bool TieneBateria(int cantidad)
    {
        return bateriaSocial >= cantidad;
    }

    public bool RestarBateria(int cantidad)
    {
        bateriaSocial = Mathf.Clamp(bateriaSocial - cantidad, 0, bateriaMaxima);
        ActualizarVisual();
        return true;
    }

    public void SumarBateria(int cantidad)
    {
        bateriaSocial = Mathf.Clamp(bateriaSocial + cantidad, 0, bateriaMaxima);
        ActualizarVisual();
    }

    private void RegenerarBateria()
    {
        tiempoRegeneracion += Time.deltaTime;

        if (tiempoRegeneracion < regeneracionIntervalo)
        {
            return;
        }

        tiempoRegeneracion = 0f;
        SumarBateria(regeneracionCantidad);
    }

    private void RevisarTeclasNumericas()
    {
        for (int i = 0; i <= 9; i++)
        {
            KeyCode teclaSuperior = (KeyCode)((int)KeyCode.Alpha0 + i);
            KeyCode teclaNumerica = (KeyCode)((int)KeyCode.Keypad0 + i);

            if (Input.GetKeyDown(teclaSuperior) || Input.GetKeyDown(teclaNumerica))
            {
                SumarBateria(1);
            }
        }
    }

    private void ActualizarVisual()
    {
        float valorNormalizado = bateriaMaxima <= 0 ? 0f : (float)bateriaSocial / bateriaMaxima;

        if (sliderBateriaSocial != null)
        {
            sliderBateriaSocial.minValue = 0f;
            sliderBateriaSocial.maxValue = bateriaMaxima;
            sliderBateriaSocial.value = bateriaSocial;
        }

        if (fillBateriaSocial != null)
        {
            if (valorNormalizado <= 0.5f)
            {
                fillBateriaSocial.color = Color.Lerp(colorBajo, colorMedio, valorNormalizado / 0.5f);
            }
            else
            {
                fillBateriaSocial.color = Color.Lerp(colorMedio, colorAlto, (valorNormalizado - 0.5f) / 0.5f);
            }
        }
    }
}
