using UnityEngine;
using UnityEngine.Rendering;

public class StartUpBlur : MonoBehaviour
{
    [Header("Volume con el efecto Blur")]
    public Volume blurVolume;

    [Header("Duración del desenfoque inicial")]
    public float blurDuration = 2f;

    [Header("Peso inicial del Blur")]
    [Range(0f, 1f)]
    public float initialWeight = 1f;

    private float timer;

    void Start()
    {
        if (blurVolume != null)
        {
            blurVolume.weight = initialWeight;
        }
    }

    void Update()
    {
        if (blurVolume == null)
            return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / blurDuration);

        // Va de 1 a 0 suavemente
        blurVolume.weight = Mathf.Lerp(initialWeight, 0f, t);
    }
}