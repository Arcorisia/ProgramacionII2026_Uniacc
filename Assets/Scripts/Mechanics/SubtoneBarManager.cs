using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SubtoneBarManager : MonoBehaviour
{
    [Header("Referencia UI")]
    public Image targetImage;

    [Header("Texto de Estado")]
    public TextMeshProUGUI subtoneText;

    [Header("Game Manager")]
    public GameManager gameManager;

    [Header("Configuración")]
    [Range(-12, 12)]
    public int currentSubtone = 0;

    public float stepTime = 4f;

    [Header("Animación Texto")]
    public float popScale = 1.2f;
    public float animDuration = 0.25f;

    private int lastCoinCount;
    private Coroutine textAnimCoroutine;

    void Start()
    {
        lastCoinCount = gameManager.coinCount;
        StartCoroutine(AutoDecrease());
        UpdateVisuals(true);
    }

    void Update()
    {
        if (gameManager.coinCount > lastCoinCount)
        {
            int difference = gameManager.coinCount - lastCoinCount;
            IncreaseSubtone(difference);
        }

        lastCoinCount = gameManager.coinCount;
    }

    IEnumerator AutoDecrease()
    {
        while (true)
        {
            yield return new WaitForSeconds(stepTime);

            currentSubtone -= 1;
            currentSubtone = Mathf.Clamp(currentSubtone, -12, 12);

            UpdateVisuals(false);
        }
    }

    public void IncreaseSubtone(int amount)
    {
        currentSubtone += amount;
        currentSubtone = Mathf.Clamp(currentSubtone, -12, 12);

        UpdateVisuals(false);
    }

    void UpdateVisuals(bool instant)
    {
        UpdateColor();
        UpdateText(instant);
    }

    // 🎨 COLOR
    void UpdateColor()
    {
        float t;
        Color baseColor = Color.white;

        if (currentSubtone > 0)
        {
            t = currentSubtone / 12f;
            targetImage.color = Color.Lerp(baseColor, Color.green, t);
        }
        else if (currentSubtone < 0)
        {
            t = Mathf.Abs(currentSubtone) / 12f;
            targetImage.color = Color.Lerp(baseColor, Color.red, t);
        }
        else
        {
            targetImage.color = baseColor;
        }
    }

    // 🔤 TEXTO
    void UpdateText(bool instant)
    {
        if (subtoneText == null) return;

        subtoneText.text = GetSubtoneLabel(currentSubtone);

        if (textAnimCoroutine != null)
            StopCoroutine(textAnimCoroutine);

        if (instant)
        {
            subtoneText.alpha = 1f;
            subtoneText.transform.localScale = Vector3.one;
        }
        else
        {
            textAnimCoroutine = StartCoroutine(TextPopAnimation());
        }
    }

    IEnumerator TextPopAnimation()
    {
        float time = 0f;

        Vector3 startScale = Vector3.one * popScale;
        Vector3 endScale = Vector3.one;

        subtoneText.transform.localScale = startScale;
        subtoneText.alpha = 0f;

        while (time < animDuration)
        {
            time += Time.deltaTime;
            float t = time / animDuration;

            // Fade
            subtoneText.alpha = Mathf.Lerp(0f, 1f, t);

            // Scale (pop)
            subtoneText.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        subtoneText.alpha = 1f;
        subtoneText.transform.localScale = endScale;
    }

    // 🧠 MAPEO DE TEXTO
    string GetSubtoneLabel(int value)
    {
        switch (value)
        {
            case -12: return "Extinción";
            case -11: return "Catástrofe";
            case -10: return "Fatal";
            case -9: return "Devastación";
            case -8: return "Tragedia";
            case -7: return "Colapso";
            case -6: return "Trágico";

            case -5:
            case -4:
            case -3: return "Peligroso";

            case -2:
            case -1: return "Infortunio";

            case 0: return "Neutralidad";

            case 1: return "Esperanzador";
            case 2: return "Estabilidad";
            case 3: return "Serenidad";
            case 4: return "Bienvivir";
            case 5: return "Prosperidad";
            case 6: return "Utopía";
            case 7: return "Majestuosidad";
            case 8: return "El mejor mundo posible";

            case 9: return "Ascenso";

            case 10: return "Fungitopía";
            case 11: return "Miceliotopía";
            case 12: return "La perfección";

            default: return "";
        }
    }
}