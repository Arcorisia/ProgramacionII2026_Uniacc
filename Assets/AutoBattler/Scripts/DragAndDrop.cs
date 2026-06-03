using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging;

    private Vector3 originalPosition;

    public bool isOverDropZone = false;
    public PosBattlerDropZone currentDropZone;
    public UnitData unitData;

    private Camera mainCam;

    private void Start()
    {
        originalPosition = transform.position;
        mainCam = Camera.main;

        if (mainCam == null)
        {
            Debug.LogError("No se encontró una cámara con el tag MainCamera.");
        }
    }

    private void OnMouseDown()
    {
        if (mainCam == null) return;

        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!isDragging || mainCam == null) return;

        transform.position = GetMouseWorldPosition() + offset;
    }

    private void OnMouseUp()
    {
        isDragging = false;

        if (currentDropZone == null || !isOverDropZone || currentDropZone.isOccupied)
        {
            transform.position = originalPosition;
        }
        else
        {
            transform.position = currentDropZone.transform.position;
            currentDropZone.isOccupied = true;
            currentDropZone.currentUnit = this;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;

        mousePos.z = mainCam.WorldToScreenPoint(transform.position).z;

        return mainCam.ScreenToWorldPoint(mousePos);
    }
}