using UnityEngine;

public class PosBattlerDropZone : MonoBehaviour
{
    public bool isOccupied = false;
    public DragAndDrop currentUnit;

    [Header("Drop Zone Color")]
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color occupiedColor = Color.red;

    private void Awake()
    {
        TryGetSpriteRenderer();
        UpdateZoneColor();
    }

    private void OnValidate()
    {
        TryGetSpriteRenderer();
        UpdateZoneColor();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DragAndDrop dragAndDrop = FindDragAndDrop(collision.gameObject);

        if (dragAndDrop == null)
        {
            return;
        }

        Debug.Log("Colision con zona de drop");
        dragAndDrop.SetDropZone(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        DragAndDrop dragAndDrop = FindDragAndDrop(collision.gameObject);

        if (dragAndDrop == null)
        {
            return;
        }

        Debug.Log("Salida de zona de drop");
        dragAndDrop.ClearDropZone(this);
        ClearUnit(dragAndDrop);
    }

    public bool CanReceive(DragAndDrop dragAndDrop)
    {
        return !isOccupied || currentUnit == null || currentUnit == dragAndDrop;
    }

    public bool IsAvailable()
    {
        return !isOccupied || currentUnit == null;
    }

    public void SetUnit(DragAndDrop dragAndDrop)
    {
        if (dragAndDrop == null)
        {
            return;
        }

        isOccupied = true;
        currentUnit = dragAndDrop;
        UpdateZoneColor();
    }

    public void ClearUnit(DragAndDrop dragAndDrop)
    {
        if (currentUnit != null && currentUnit != dragAndDrop)
        {
            return;
        }

        isOccupied = false;
        currentUnit = null;
        UpdateZoneColor();
    }

    public Vector3 GetDropPosition(Vector3 fallbackPosition)
    {
        Collider2D collider2D = GetComponent<Collider2D>();

        if (collider2D != null)
        {
            Vector3 center = collider2D.bounds.center;
            return new Vector3(center.x, center.y, fallbackPosition.z);
        }

        return new Vector3(transform.position.x, transform.position.y, fallbackPosition.z);
    }

    private DragAndDrop FindDragAndDrop(GameObject candidate)
    {
        if (candidate == null)
        {
            return null;
        }

        DragAndDrop dragAndDrop = candidate.GetComponent<DragAndDrop>();

        if (dragAndDrop == null)
        {
            dragAndDrop = candidate.GetComponentInParent<DragAndDrop>();
        }

        if (dragAndDrop == null)
        {
            dragAndDrop = candidate.GetComponentInChildren<DragAndDrop>();
        }

        return dragAndDrop;
    }

    private void TryGetSpriteRenderer()
    {
        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void UpdateZoneColor()
    {
        if (targetSpriteRenderer == null)
        {
            return;
        }

        targetSpriteRenderer.color = isOccupied ? occupiedColor : availableColor;
    }
}
