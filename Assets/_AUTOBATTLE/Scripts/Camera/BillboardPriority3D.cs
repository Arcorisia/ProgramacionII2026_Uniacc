using UnityEngine;
using UnityEngine.Rendering;

public class BillboardPriority3D : MonoBehaviour
{
    [Header("Tags")]
    [SerializeField] private string playerTag = "player";
    [SerializeField] private string billboardTag = "Billboard";

    [Header("Sorting Orders")]
    [SerializeField] private int playerDefaultOrder = 50;
    [SerializeField] private int billboardDefaultOrder = 25;

    [SerializeField] private int playerInFrontOrder = 100;
    [SerializeField] private int playerBehindOrder = 10;

    private Renderer playerRenderer;
    private SortingGroup playerSortingGroup;

    private void Awake()
    {
        playerRenderer = GetComponentInChildren<Renderer>();
        playerSortingGroup = GetComponentInChildren<SortingGroup>();

        ApplySorting(gameObject, playerDefaultOrder);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!CompareTag(playerTag)) return;
        if (!other.CompareTag(billboardTag)) return;

        GameObject billboard = other.gameObject;

        float playerY = transform.position.y;
        float billboardY = billboard.transform.position.y;

        if (playerY < billboardY)
        {
            // Player está más abajo en Y: se dibuja delante.
            ApplySorting(gameObject, playerInFrontOrder);
            ApplySorting(billboard, billboardDefaultOrder);
        }
        else if (playerY > billboardY)
        {
            // Player está más arriba en Y: se dibuja detrás.
            ApplySorting(gameObject, playerBehindOrder);
            ApplySorting(billboard, billboardDefaultOrder);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(billboardTag)) return;

        ApplySorting(gameObject, playerDefaultOrder);
        ApplySorting(other.gameObject, billboardDefaultOrder);
    }

    private void ApplySorting(GameObject target, int order)
    {
        SortingGroup sortingGroup = target.GetComponentInChildren<SortingGroup>();

        if (sortingGroup != null)
        {
            sortingGroup.sortingOrder = order;
            return;
        }

        Renderer renderer = target.GetComponentInChildren<Renderer>();

        if (renderer != null)
        {
            renderer.sortingOrder = order;
        }
    }
}