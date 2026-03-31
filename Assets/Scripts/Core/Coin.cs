using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    public int value = 1;

    private Collider2D col;
    private SpriteRenderer sr;

    void Start()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            GameManager.instance.CollectCoin(value);

            StartCoroutine(RespawnCoin());
        }
    }

    IEnumerator RespawnCoin()
    {
        // Desactivar visual y colisión
        col.enabled = false;
        sr.enabled = false;

        // Elegir tiempo aleatorio
        int[] tiempos = {10, 15, 30, 35};
        int tiempoElegido = tiempos[Random.Range(0, tiempos.Length)];

        yield return new WaitForSeconds(tiempoElegido);

        // Reactivar
        col.enabled = true;
        sr.enabled = true;
    }
}