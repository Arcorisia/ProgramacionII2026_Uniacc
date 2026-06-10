using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging;
    private Vector3 originalPosition;
    private PosBattlerDropZone lastConfirmedDropZone;
    private Camera mainCam;

    public bool isOverDropZone = false;
    public PosBattlerDropZone currentDropZone;
    public UnitData unitData;

    private void Start()
    {
        originalPosition = transform.position;
        mainCam = Camera.main;
        RegisterChildMouseForwarders();

        if (mainCam == null)
        {
            Debug.LogError("No se encontro una camara con el tag MainCamera.");
        }
    }

    private void OnMouseDown()
    {
        BeginDrag();
    }

    private void OnMouseDrag()
    {
        ContinueDrag();
    }

    private void OnMouseUp()
    {
        EndDrag();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PosBattlerDropZone dropZone = FindDropZone(collision.gameObject);

        if (dropZone != null)
        {
            SetDropZone(dropZone);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PosBattlerDropZone dropZone = FindDropZone(collision.gameObject);

        if (dropZone != null)
        {
            ClearDropZone(dropZone);
        }
    }

    public void BeginDrag()
    {
        if (mainCam == null)
        {
            return;
        }

        if (lastConfirmedDropZone != null && lastConfirmedDropZone.currentUnit == this)
        {
            lastConfirmedDropZone.ClearUnit(this);
        }

        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;
    }

    public void ContinueDrag()
    {
        if (!isDragging || mainCam == null)
        {
            return;
        }

        transform.position = GetMouseWorldPosition() + offset;
    }

    public void EndDrag()
    {
        isDragging = false;

        if (currentDropZone == null || !isOverDropZone || !currentDropZone.CanReceive(this))
        {
            transform.position = originalPosition;
            return;
        }

        ConfirmDrop(currentDropZone);
    }

    public void SetDropZone(PosBattlerDropZone dropZone)
    {
        if (dropZone == null)
        {
            return;
        }

        isOverDropZone = true;
        currentDropZone = dropZone;
    }

    public void ClearDropZone(PosBattlerDropZone dropZone)
    {
        if (dropZone != null && currentDropZone != dropZone)
        {
            return;
        }

        isOverDropZone = false;
        currentDropZone = null;
    }

    public void ConfirmDrop(PosBattlerDropZone dropZone)
    {
        if (dropZone == null)
        {
            return;
        }

        transform.position = dropZone.GetDropPosition(transform.position);
        dropZone.SetUnit(this);
        lastConfirmedDropZone = dropZone;
        originalPosition = transform.position;
        isOverDropZone = true;
        currentDropZone = dropZone;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCam.WorldToScreenPoint(transform.position).z;
        return mainCam.ScreenToWorldPoint(mousePos);
    }

    private PosBattlerDropZone FindDropZone(GameObject candidate)
    {
        if (candidate == null)
        {
            return null;
        }

        PosBattlerDropZone dropZone = candidate.GetComponent<PosBattlerDropZone>();

        if (dropZone == null)
        {
            dropZone = candidate.GetComponentInParent<PosBattlerDropZone>();
        }

        if (dropZone == null)
        {
            dropZone = candidate.GetComponentInChildren<PosBattlerDropZone>();
        }

        return dropZone;
    }

    private void RegisterChildMouseForwarders()
    {
        Collider[] colliders3D = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders3D.Length; i++)
        {
            RegisterForwarder(colliders3D[i].gameObject);
        }

        Collider2D[] colliders2D = GetComponentsInChildren<Collider2D>();

        for (int i = 0; i < colliders2D.Length; i++)
        {
            RegisterForwarder(colliders2D[i].gameObject);
        }
    }

    private void RegisterForwarder(GameObject target)
    {
        if (target == null || target == gameObject)
        {
            return;
        }

        DragAndDropMouseForwarder forwarder = target.GetComponent<DragAndDropMouseForwarder>();

        if (forwarder == null)
        {
            forwarder = target.AddComponent<DragAndDropMouseForwarder>();
        }

        forwarder.Initialize(this);
    }
}

class DragAndDropMouseForwarder : MonoBehaviour
{
    private DragAndDrop owner;

    public void Initialize(DragAndDrop dragAndDrop)
    {
        owner = dragAndDrop;
    }

    private void OnMouseDown()
    {
        if (owner != null)
        {
            owner.BeginDrag();
        }
    }

    private void OnMouseDrag()
    {
        if (owner != null)
        {
            owner.ContinueDrag();
        }
    }

    private void OnMouseUp()
    {
        if (owner != null)
        {
            owner.EndDrag();
        }
    }
}
