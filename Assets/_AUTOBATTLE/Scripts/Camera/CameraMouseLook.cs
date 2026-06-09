using UnityEngine;

public class CameraMouseLook : MonoBehaviour
{
    public float amount = 2f;
    public float smoothSpeed = 5f;

    private Quaternion targetRotation;
    private Quaternion baseRotation;

    void Start()
    {
        baseRotation = transform.rotation;
        targetRotation = baseRotation;
    }

    void LateUpdate()
    {
        float x = (Input.mousePosition.x / Screen.width - 0.5f) * amount;
        float y = (Input.mousePosition.y / Screen.height - 0.5f) * amount;

        targetRotation =
            baseRotation *
            Quaternion.Euler(-y, x, 0);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * smoothSpeed);
    }

    public void SetBaseRotation(Quaternion rot)
    {
        baseRotation = rot;
    }
}