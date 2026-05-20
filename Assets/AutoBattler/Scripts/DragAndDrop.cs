using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 originalPosition;
    public bool isOverDropZone = false;
    public PosBattlerDropZone currentDropZone;
    public UnitData unitData; // Datos de la unidad asociada a este objeto de arrastre
    private void Start()
    {
        originalPosition = transform.position;
    }

    private void OnMouseDown()
    {
        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        if(!isOverDropZone || currentDropZone.isOccupied)
        {
            transform.position = originalPosition; // Vuelve a la posición original al soltar el mouse
        }
        else
        {
            transform.position = currentDropZone.transform.position; // Se posiciona en la zona de drop
            currentDropZone.isOccupied = true; // Marca la zona de drop como ocupada
            currentDropZone.currentUnit = this; // Asocia esta unidad a la zona de drop
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z; // Mantener la misma profundidad
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
