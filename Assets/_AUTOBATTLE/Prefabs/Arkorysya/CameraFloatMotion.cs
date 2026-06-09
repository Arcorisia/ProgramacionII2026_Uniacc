using UnityEngine;

public class CameraFloatMotion : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float amplitude = 0.2f;
    [SerializeField] private float frequency = 0.3f;

    [Header("Rotación")]
    [SerializeField] private float rotationAmplitude = 1f;
    [SerializeField] private float rotationFrequency = 0.2f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private float seedX;
    private float seedY;

    private void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        seedX = Random.Range(0f, 1000f);
        seedY = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        float t = Time.time;

        float offsetX = (Mathf.PerlinNoise(seedX, t * frequency) - 0.5f) * amplitude;
        float offsetY = (Mathf.PerlinNoise(seedY, t * frequency) - 0.5f) * amplitude;

        transform.localPosition = startPosition +
                                  new Vector3(offsetX, offsetY, 0f);

        float rotZ = (Mathf.PerlinNoise(seedX + 100f, t * rotationFrequency) - 0.5f)
                     * rotationAmplitude;

        transform.localRotation =
            startRotation * Quaternion.Euler(0f, 0f, rotZ);
    }
}