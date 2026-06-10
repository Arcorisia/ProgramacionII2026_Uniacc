using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BillboardPriority3D : MonoBehaviour
{
    [Header("Tags")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string billboardTag = "Billboard";

    [Header("Sorting Orders")]
    [SerializeField] private int playerDefaultOrder = 50;
    [SerializeField] private int billboardDefaultOrder = 25;
    [SerializeField] private int playerFrontOrder = 100;
    [SerializeField] private int playerHiddenOrder = 10;
    [SerializeField] private int billboardFrontOrder = 100;
    [SerializeField] private int billboardHiddenOrder = 10;

    private readonly HashSet<GameObject> billboardsActivos = new HashSet<GameObject>();

    private void Awake()
    {
        ApplySorting(gameObject, playerDefaultOrder);
    }

    private void LateUpdate()
    {
        if (!CompareTag(playerTag))
        {
            return;
        }

        GameObject billboardMasCercano = null;
        float distanciaMasCercana = float.MaxValue;

        foreach (GameObject billboard in billboardsActivos)
        {
            if (billboard == null)
            {
                continue;
            }

            float distancia = Vector3.SqrMagnitude(transform.position - billboard.transform.position);

            if (distancia < distanciaMasCercana)
            {
                distanciaMasCercana = distancia;
                billboardMasCercano = billboard;
            }
        }

        if (billboardMasCercano == null)
        {
            ApplySorting(gameObject, playerDefaultOrder);
            return;
        }

        ActualizarPrioridadConBillboard(billboardMasCercano);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CompareTag(playerTag)) return;
        if (!other.CompareTag(billboardTag)) return;

        billboardsActivos.Add(other.gameObject);
        ActualizarPrioridadConBillboard(other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!CompareTag(playerTag)) return;
        if (!other.CompareTag(billboardTag)) return;

        billboardsActivos.Add(other.gameObject);
        ActualizarPrioridadConBillboard(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(billboardTag)) return;

        billboardsActivos.Remove(other.gameObject);
        ApplySorting(other.gameObject, billboardDefaultOrder);

        if (billboardsActivos.Count == 0)
        {
            ApplySorting(gameObject, playerDefaultOrder);
        }
    }

    private void ActualizarPrioridadConBillboard(GameObject billboard)
    {
        if (billboard == null)
        {
            return;
        }

        float playerY = transform.position.y;
        float billboardY = billboard.transform.position.y;

        if (playerY > billboardY)
        {
            ApplySorting(gameObject, playerHiddenOrder);
            ApplySorting(billboard, billboardFrontOrder);
        }
        else if (playerY < billboardY)
        {
            ApplySorting(gameObject, playerFrontOrder);
            ApplySorting(billboard, billboardHiddenOrder);
        }
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
