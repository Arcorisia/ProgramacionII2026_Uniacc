using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 4f;
    public bool normalizarMovimientoDiagonal = true;

    [Header("Skin Player")]
    public GameObject spritePlayer;
    public GameObject[] skinPlayers = new GameObject[11];

    [Header("Deteccion")]
    public string skinTag = "Skin";

    public PlayerSkinState SkinState
    {
        get { return skinState; }
    }

    private PlayerSkinState skinState;
    private int skinActivo = -1;
    private int skinSolicitado = -1;
    private Rigidbody2D rb2D;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        skinState = ScriptableObject.CreateInstance<PlayerSkinState>();
        skinState.Load();
        skinSolicitado = skinState.skinPlayerActual;

        CompletarSkinsDesdeHijosSiHaceFalta();
    }

    private void Start()
    {
        AplicarSkinSolicitado();
    }

    private void Update()
    {
        MoverPlayer();
        AplicarSkinSolicitado();
    }

    public void CambiarSkin(int skinIndex)
    {
        skinSolicitado = Mathf.Clamp(skinIndex, 0, 10);
        skinState.Save(skinSolicitado);
    }

    private void MoverPlayer()
    {
        Vector2 entrada = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (normalizarMovimientoDiagonal && entrada.sqrMagnitude > 1f)
        {
            entrada.Normalize();
        }

        Vector3 desplazamiento = new Vector3(entrada.x, entrada.y, 0f) * velocidadMovimiento * Time.deltaTime;

        if (rb2D != null)
        {
            rb2D.MovePosition(rb2D.position + new Vector2(desplazamiento.x, desplazamiento.y));
            return;
        }

        transform.position += desplazamiento;
    }

    private void AplicarSkinSolicitado()
    {
        int skinIndex = Mathf.Clamp(skinSolicitado, 0, 10);

        if (skinActivo == skinIndex && SoloSkinSolicitadoEstaActivo(skinIndex))
        {
            return;
        }

        for (int i = 0; i < skinPlayers.Length; i++)
        {
            if (skinPlayers[i] != null)
            {
                skinPlayers[i].SetActive(i == skinIndex);
            }
        }

        skinActivo = skinIndex;
    }

    private bool SoloSkinSolicitadoEstaActivo(int skinIndex)
    {
        for (int i = 0; i < skinPlayers.Length; i++)
        {
            if (skinPlayers[i] == null)
            {
                continue;
            }

            if (skinPlayers[i].activeSelf != (i == skinIndex))
            {
                return false;
            }
        }

        return true;
    }

    private void CompletarSkinsDesdeHijosSiHaceFalta()
    {
        if (spritePlayer == null)
        {
            return;
        }

        for (int i = 0; i < skinPlayers.Length && i < spritePlayer.transform.childCount; i++)
        {
            if (skinPlayers[i] == null)
            {
                skinPlayers[i] = spritePlayer.transform.GetChild(i).gameObject;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        RevisarSkin(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        RevisarSkin(other.gameObject);
    }

    private void RevisarSkin(GameObject candidato)
    {
        if (candidato == null || candidato.tag != skinTag)
        {
            return;
        }

        SkinType skinType = candidato.GetComponent<SkinType>();

        if (skinType == null)
        {
            skinType = candidato.GetComponentInParent<SkinType>();
        }

        if (skinType == null)
        {
            return;
        }

        CambiarSkin(skinType.skinType);
        skinType.OnPlayerCollected(gameObject);
    }
}
