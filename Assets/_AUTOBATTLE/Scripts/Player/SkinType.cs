using System.Collections;
using UnityEngine;

public enum SkinTypeMode
{
    Objeto,
    Aliade
}

public class SkinType : MonoBehaviour
{
    [Range(0, 10)]
    public int skinType;

    public SkinTypeMode modo = SkinTypeMode.Objeto;
    public float tiempoAntesDeAplicar = 1f;
    public Transform contenedorObjeto;
    public Collider collider3D;
    public Collider2D collider2D;

    private bool fueUsado;

    private void Reset()
    {
        collider3D = GetComponent<Collider>();
        collider2D = GetComponent<Collider2D>();
        contenedorObjeto = transform.parent != null ? transform.parent : transform;
    }

    private void OnValidate()
    {
        skinType = Mathf.Clamp(skinType, 0, 10);
        tiempoAntesDeAplicar = Mathf.Max(0f, tiempoAntesDeAplicar);
    }

    public void OnPlayerCollected(GameObject player)
    {
        if (fueUsado)
        {
            return;
        }

        fueUsado = true;
        StartCoroutine(AplicarModoDespuesDeEspera());
    }

    private IEnumerator AplicarModoDespuesDeEspera()
    {
        yield return new WaitForSeconds(tiempoAntesDeAplicar);

        if (modo == SkinTypeMode.Objeto)
        {
            Transform objetivo = contenedorObjeto != null ? contenedorObjeto : transform.parent;
            Destroy(objetivo != null ? objetivo.gameObject : gameObject);
            yield break;
        }

        Collider objetivo3D = collider3D != null ? collider3D : GetComponent<Collider>();
        Collider2D objetivo2D = collider2D != null ? collider2D : GetComponent<Collider2D>();

        if (objetivo3D != null)
        {
            objetivo3D.enabled = false;
        }

        if (objetivo2D != null)
        {
            objetivo2D.enabled = false;
        }
    }
}
