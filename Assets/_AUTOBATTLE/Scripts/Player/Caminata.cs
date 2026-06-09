using UnityEngine;

public class Caminata : MonoBehaviour
{
    [Header("Detección de movimiento")]
    public float sensibilidadMovimiento = 0.001f;

    [Header("Balanceo tipo recorte / South Park")]
    public float velocidadCaminata = 10f;
    public float amplitudRotacion = 6f;
    public float amplitudSubida = 0.04f;
    public float amplitudEscalaX = 0.03f;
    public float amplitudEscalaY = 0.03f;

    [Header("Suavizado")]
    public float velocidadVolverNormal = 10f;
    public bool usarMovimientoLocal = false;

    [Header("Opciones")]
    public bool activarRotacion = true;
    public bool activarSubida = true;
    public bool activarEscala = true;

    private Vector3 posicionAnterior;
    private Vector3 posicionInicialLocal;
    private Quaternion rotacionInicialLocal;
    private Vector3 escalaInicialLocal;

    private float tiempoCaminata;
    private bool estaMoviendose;

    void Start()
    {
        posicionAnterior = usarMovimientoLocal ? transform.localPosition : transform.position;

        posicionInicialLocal = transform.localPosition;
        rotacionInicialLocal = transform.localRotation;
        escalaInicialLocal = transform.localScale;
    }

    void Update()
    {
        DetectarMovimiento();
        AplicarEfectoCaminata();
    }

    void DetectarMovimiento()
    {
        Vector3 posicionActual = usarMovimientoLocal ? transform.localPosition : transform.position;

        float distancia = Vector3.Distance(posicionActual, posicionAnterior);
        estaMoviendose = distancia > sensibilidadMovimiento;

        posicionAnterior = posicionActual;
    }

    void AplicarEfectoCaminata()
    {
        if (estaMoviendose)
        {
            tiempoCaminata += Time.deltaTime * velocidadCaminata;

            float onda = Mathf.Sin(tiempoCaminata);
            float ondaAbs = Mathf.Abs(onda);

            Quaternion rotacionObjetivo = rotacionInicialLocal;
            Vector3 posicionObjetivo = posicionInicialLocal;
            Vector3 escalaObjetivo = escalaInicialLocal;

            if (activarRotacion)
            {
                rotacionObjetivo = rotacionInicialLocal * Quaternion.Euler(0f, 0f, onda * amplitudRotacion);
            }

            if (activarSubida)
            {
                posicionObjetivo = posicionInicialLocal + new Vector3(0f, ondaAbs * amplitudSubida, 0f);
            }

            if (activarEscala)
            {
                escalaObjetivo = escalaInicialLocal + new Vector3(
                    ondaAbs * amplitudEscalaX,
                    -ondaAbs * amplitudEscalaY,
                    0f
                );
            }

            transform.localRotation = rotacionObjetivo;
            transform.localPosition = posicionObjetivo;
            transform.localScale = escalaObjetivo;
        }
        else
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                rotacionInicialLocal,
                Time.deltaTime * velocidadVolverNormal
            );

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                posicionInicialLocal,
                Time.deltaTime * velocidadVolverNormal
            );

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                escalaInicialLocal,
                Time.deltaTime * velocidadVolverNormal
            );
        }
    }
}
