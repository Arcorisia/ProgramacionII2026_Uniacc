using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public Camera mainCamera;
    public Transform[] cameraPoints;

    public float moveDuration = 1f;
    public bool autoSwitch = true;
    public float switchInterval = 4f;

    private int currentIndex = -1;
    private Coroutine moveRoutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (cameraPoints.Length > 0)
        {
            MoveToCameraPoint(cameraPoints[0]);
        }

        if (autoSwitch)
        {
            StartCoroutine(AutoSwitchRoutine());
        }
    }

    IEnumerator AutoSwitchRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchInterval);

            if (cameraPoints.Length <= 1)
                continue;

            int randomIndex;

            do
            {
                randomIndex = Random.Range(0, cameraPoints.Length);
            }
            while (randomIndex == currentIndex);

            MoveToCameraPoint(cameraPoints[randomIndex]);
        }
    }

    public void MoveToCameraPoint(Transform target)
    {
        currentIndex = System.Array.IndexOf(cameraPoints, target);

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(target));
    }

    IEnumerator MoveRoutine(Transform target)
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;

            mainCamera.transform.position =
                Vector3.Lerp(startPos, target.position, t);

            mainCamera.transform.rotation =
                Quaternion.Slerp(startRot, target.rotation, t);

            yield return null;
        }
    }
}
