using UnityEngine;
using System.Collections;

public class TransformTransition : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform targetTransform;

    [Header("Duración")]
    public float duration = 2f;

    [Header("Curva")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Opciones")]
    public bool playOnStart = true;
    public bool keepFinalTransform = true;
    
    [Header("Escala")]
    [Tooltip("Si está activado, se interpretará la escala del target como un multiplicador relativo a la escala inicial.")]
    public bool useRelativeScale = false;

    [Tooltip("Activa para limitar la escala final si es demasiado grande.")]
    public bool clampMaxScale = false;

    [Tooltip("Valor máximo uniforme permitido para cualquier componente de escala cuando 'clampMaxScale' está activo.")]
    public float maxUniformScale = 3f;
    private bool isTransitioning;

    void Start()
    {
        if (playOnStart)
            PlayTransition();
    }

    [ContextMenu("Play Transition")]
    public void PlayTransition()
    {
        if (targetTransform == null || isTransitioning)
            return;

        StartCoroutine(TransitionCoroutine());
    }

    IEnumerator TransitionCoroutine()
    {
        isTransitioning = true;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;

        Vector3 endPosition = targetTransform.position;
        Quaternion endRotation = targetTransform.rotation;
        Vector3 endScale;

        if (useRelativeScale)
        {
            endScale = Vector3.Scale(startScale, targetTransform.localScale);
        }
        else
        {
            endScale = targetTransform.localScale;
        }

        if (clampMaxScale)
        {
            endScale = Vector3.Min(endScale, Vector3.one * maxUniformScale);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = curve.Evaluate(t);

            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        if (keepFinalTransform)
        {
            transform.position = endPosition;
            transform.rotation = endRotation;
            transform.localScale = endScale;
        }

        isTransitioning = false;
    }
}