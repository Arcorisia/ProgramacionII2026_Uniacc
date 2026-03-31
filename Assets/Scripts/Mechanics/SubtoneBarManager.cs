using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SubtoneBarManager : MonoBehaviour
{
    [Header("Referencia UI")]
    public Image barraImagen;

    [Header("Configuración Subtonos")]
    [Range(-12, 12)]
    public int subtonoActual = 0;

    public float tiempoCambio = 4f;

    [Header("Debug")]
    public bool autoStart = true;

    Coroutine rutina;

    void Start()
    {
        AplicarSubtono();

        if (autoStart)
            rutina = StartCoroutine(BucleSubtono());
    }

    IEnumerator BucleSubtono()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoCambio);

            int descenso = Random.Range(1, 4); // 1,2,3
            CambiarSubtono(-descenso);
        }
    }

    // 🔻 CAMBIO CENTRAL
    public void CambiarSubtono(int delta)
    {
        subtonoActual += delta;
        subtonoActual = Mathf.Clamp(subtonoActual, -12, 12);

        AplicarSubtono();
    }

    // 🎨 APLICACIÓN VISUAL
    void AplicarSubtono()
    {
        float t = (subtonoActual + 12f) / 24f;

        // Separación conceptual:
        float intensidadBlanca = Mathf.Clamp01((t - 0.5f) * 2f); 
        float saturacionGris = Mathf.Clamp01((0.5f - t) * 2f);

        // 🎯 Resultado final:
        Color colorBase = Color.white;

        // Gris (lado negativo)
        Color gris = Color.gray;
        colorBase = Color.Lerp(colorBase, gris, saturacionGris);

        // Intensidad (lado positivo)
        colorBase *= (1f + intensidadBlanca);

        barraImagen.color = colorBase;
    }

    // 🪙 EVENTO DESDE GAME MANAGER
    public void OnCoinsChanged(int coinCount)
    {
        if (coinCount % 2 == 0)
        {
            CambiarSubtono(+1);
        }
    }
}