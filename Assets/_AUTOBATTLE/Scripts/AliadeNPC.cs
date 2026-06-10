using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum AliadeEstadoMovimiento
{
    Quieto,
    Pasear,
    Follow
}

public class AliadeNPC : MonoBehaviour
{
    [Header("Movimiento")]
    public AliadeEstadoMovimiento estadoActual = AliadeEstadoMovimiento.Quieto;
    public float velocidadMovimiento = 2f;
    public float tiempoQuietoAntesPasear = 4f;
    public float tiempoMaximoPaseando = 6f;
    public float radioPaseo = 2f;
    public float distanciaPrudentePlayer = 1.25f;
    public float distanciaLlegada = 0.08f;
    public bool usarPlanoXY = true;
    public bool detectarPlanoPorMovimientoPlayer = true;

    [Header("Follow")]
    public bool followActivo;
    public Transform player;
    public string playerTag = "Player";
    public int bateriaNecesariaFollow = 25;
    public int costoCancelarFollow = 25;
    public float sensibilidadMovimientoPlayer = 0.01f;
    public int indicePosicionFollow = -1;
    public float separacionFollowLateral = 0.8f;
    public bool orbitarAlrededorDelPlayer = true;
    public float velocidadOrbitaFollow = 90f;

    [Header("Sprites")]
    public GameObject spriteActual;
    public GameObject spriteAlegria;
    public SpriteRenderer spriteRendererColor;
    public Color colorCancelado = Color.red;
    public float duracionColorCancelado = 10f;

    [Header("Alegria")]
    public float duracionAlegria = 4f;
    public float alturaSaltito = 0.15f;
    public float velocidadSaltito = 12f;

    [Header("Prefab interno")]
    public GameObject prefabInstanciable;
    public RectTransform prefabCanvasParent;
    public Vector3 prefabOffsetDesdeCentroPantalla = Vector3.zero;
    public Vector3 prefabRotacionEuler = Vector3.zero;
    public Vector3 prefabEscala = new Vector3(0.1f, 0.1f, 0.1f);
    public int prefabLayer = 0;
    public bool usarSortingDelAliade = true;
    public string prefabSortingLayer = "";
    public int prefabSortingOrder = 10;
    public int prefabSortingOrderOffset = 0;
    public float prefabDistanciaDesdeCamara = 10f;
    public float prefabDuracionAntesDesvanecer = 10f;
    public float prefabTiempoDesvanecer = 2f;
    public PosBattlerDropZone autoColocadoOpcion1;
    public PosBattlerDropZone autoColocadoOpcion2;
    public PosBattlerDropZone autoColocadoOpcion3;
    public PosBattlerDropZone autoColocadoOpcion4;

    [Header("Efecto Caminata")]
    public Caminata caminata;

    private static GameObject instanciaPrefabUnica;
    private static AliadeNPC duenoPrefab;

    private Camera camaraPrincipal;
    private SocialBatteryManager socialBattery;
    private Rigidbody rigidbody3D;
    private Rigidbody2D rigidbody2D;
    private Vector3 centroPaseo;
    private Vector3 objetivoPaseo;
    private Vector3 ultimaPosicionPlayer;
    private float tiempoQuieto;
    private float tiempoPaseando;
    private bool arrastrandoPrefab;
    private bool prefabAutoColocadoEnDropZone;
    private Vector3 offsetArrastrePrefab;
    private Vector2 offsetArrastrePrefabUI;
    private Coroutine rutinaAlegria;
    private Coroutine rutinaPrefab;
    private Coroutine rutinaColorCancelado;
    private Color colorOriginal;
    private int indiceFollowAsignado = -1;
    private float anguloOrbitaFollow;
    private static int siguienteIndiceFollow;

    private void Awake()
    {
        camaraPrincipal = Camera.main;
        socialBattery = SocialBatteryManager.Instance;
        rigidbody3D = GetComponent<Rigidbody>();
        rigidbody2D = GetComponent<Rigidbody2D>();

        if (caminata == null)
        {
            caminata = BuscarCaminataVisual();
        }
        else if (caminata.transform == transform)
        {
            caminata.enabled = false;
            caminata = BuscarCaminataVisual();
        }

        if (spriteRendererColor == null)
        {
            spriteRendererColor = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRendererColor != null)
        {
            colorOriginal = spriteRendererColor.color;
        }
    }

    private void Start()
    {
        BuscarPlayerSiHaceFalta();
        centroPaseo = transform.position;
        objetivoPaseo = transform.position;

        if (player != null)
        {
            ultimaPosicionPlayer = player.position;
        }

        if (spriteAlegria != null)
        {
            spriteAlegria.SetActive(false);
        }
    }

    private void Update()
    {
        BuscarPlayerSiHaceFalta();
        ActualizarMovimiento();
        MantenerCaminataActiva();
        ActualizarPrefabInstanciado();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClickIzquierdo();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ClickDerecho();
        }
    }

    private void ClickIzquierdo()
    {
        MostrarAlegria();

        if (followActivo)
        {
            InstanciarPrefabUnico();
            return;
        }

        if (socialBattery == null)
        {
            socialBattery = SocialBatteryManager.Instance;
        }

        if (socialBattery != null && socialBattery.TieneBateria(bateriaNecesariaFollow))
        {
            followActivo = true;
            estadoActual = AliadeEstadoMovimiento.Follow;
        }
    }

    private void ClickDerecho()
    {
        if (!followActivo)
        {
            return;
        }

        followActivo = false;
        estadoActual = AliadeEstadoMovimiento.Quieto;
        tiempoQuieto = 0f;
        tiempoPaseando = 0f;

        MostrarColorCancelado();

        if (socialBattery == null)
        {
            socialBattery = SocialBatteryManager.Instance;
        }

        if (socialBattery != null)
        {
            socialBattery.RestarBateria(costoCancelarFollow);
        }
    }

    private void ActualizarMovimiento()
    {
        if (followActivo && player != null)
        {
            ActualizarPlanoSiHaceFalta();
            estadoActual = AliadeEstadoMovimiento.Follow;
            SeguirPlayer();
            ultimaPosicionPlayer = player.position;
            return;
        }

        if (estadoActual == AliadeEstadoMovimiento.Follow)
        {
            estadoActual = AliadeEstadoMovimiento.Quieto;
        }

        ActualizarQuietoYPaseo();

        if (player != null)
        {
            ultimaPosicionPlayer = player.position;
        }
    }

    private void ActualizarQuietoYPaseo()
    {
        if (estadoActual == AliadeEstadoMovimiento.Quieto)
        {
            tiempoQuieto += Time.deltaTime;

            if (tiempoQuieto >= tiempoQuietoAntesPasear)
            {
                centroPaseo = transform.position;
                objetivoPaseo = CrearObjetivoPaseo();
                estadoActual = AliadeEstadoMovimiento.Pasear;
                tiempoPaseando = 0f;
            }

            return;
        }

        if (estadoActual != AliadeEstadoMovimiento.Pasear)
        {
            return;
        }

        MoverHacia(objetivoPaseo);
        tiempoPaseando += Time.deltaTime;

        if (tiempoPaseando >= tiempoMaximoPaseando)
        {
            estadoActual = AliadeEstadoMovimiento.Quieto;
            tiempoQuieto = 0f;
            tiempoPaseando = 0f;
            return;
        }

        if (Vector3.Distance(PosicionEnPlano(transform.position), PosicionEnPlano(objetivoPaseo)) <= distanciaLlegada)
        {
            objetivoPaseo = CrearObjetivoPaseo();
        }
    }

    private void SeguirPlayer()
    {
        Vector3 objetivo = player.position + ObtenerOffsetFollow();
        MoverHacia(objetivo);
    }

    private Vector3 ObtenerOffsetFollow()
    {
        int indice = ObtenerIndiceFollow();
        float distanciaPrincipal = Mathf.Max(0.1f, distanciaPrudentePlayer);
        float distanciaLateral = Mathf.Max(0f, separacionFollowLateral);

        if (orbitarAlrededorDelPlayer)
        {
            anguloOrbitaFollow += velocidadOrbitaFollow * Time.deltaTime;
            float angulo = anguloOrbitaFollow + indice * 90f;
            float radianes = angulo * Mathf.Deg2Rad;
            float x = Mathf.Cos(radianes) * distanciaPrincipal;
            float yOZ = Mathf.Sin(radianes) * distanciaPrincipal;

            if (usarPlanoXY)
            {
                return new Vector3(x, yOZ, 0f);
            }

            return new Vector3(x, 0f, yOZ);
        }

        if (usarPlanoXY)
        {
            switch (indice % 4)
            {
                case 0:
                    return new Vector3(0f, -distanciaPrincipal, 0f);
                case 1:
                    return new Vector3(0f, distanciaPrincipal, 0f);
                case 2:
                    return new Vector3(-distanciaPrincipal, -distanciaLateral, 0f);
                default:
                    return new Vector3(distanciaPrincipal, -distanciaLateral, 0f);
            }
        }

        switch (indice % 4)
        {
            case 0:
                return new Vector3(0f, 0f, -distanciaPrincipal);
            case 1:
                return new Vector3(0f, 0f, distanciaPrincipal);
            case 2:
                return new Vector3(-distanciaPrincipal, 0f, -distanciaLateral);
            default:
                return new Vector3(distanciaPrincipal, 0f, -distanciaLateral);
        }
    }

    private int ObtenerIndiceFollow()
    {
        if (indicePosicionFollow >= 0)
        {
            return indicePosicionFollow;
        }

        if (indiceFollowAsignado < 0)
        {
            indiceFollowAsignado = siguienteIndiceFollow;
            siguienteIndiceFollow++;
            anguloOrbitaFollow = indiceFollowAsignado * 90f;
        }

        return indiceFollowAsignado;
    }

    private void MoverHacia(Vector3 objetivo)
    {
        Vector3 posicionActual = transform.position;
        Vector3 objetivoPlano = AjustarObjetivoAlPlano(objetivo, posicionActual);
        Vector3 nuevaPosicion = Vector3.MoveTowards(posicionActual, objetivoPlano, velocidadMovimiento * Time.deltaTime);

        if (rigidbody2D != null)
        {
            rigidbody2D.MovePosition(new Vector2(nuevaPosicion.x, nuevaPosicion.y));
            return;
        }

        if (rigidbody3D != null)
        {
            rigidbody3D.MovePosition(nuevaPosicion);
            return;
        }

        transform.position = nuevaPosicion;
    }

    private Vector3 CrearObjetivoPaseo()
    {
        Vector2 punto = Random.insideUnitCircle * radioPaseo;

        if (usarPlanoXY)
        {
            return centroPaseo + new Vector3(punto.x, punto.y, 0f);
        }

        return centroPaseo + new Vector3(punto.x, 0f, punto.y);
    }

    private bool PlayerSeMovio()
    {
        if (player == null)
        {
            return false;
        }

        return Vector3.Distance(player.position, ultimaPosicionPlayer) > sensibilidadMovimientoPlayer;
    }

    private void ActualizarPlanoSiHaceFalta()
    {
        if (!detectarPlanoPorMovimientoPlayer || player == null)
        {
            return;
        }

        Vector3 deltaPlayer = player.position - ultimaPosicionPlayer;

        if (deltaPlayer.sqrMagnitude <= sensibilidadMovimientoPlayer * sensibilidadMovimientoPlayer)
        {
            return;
        }

        usarPlanoXY = Mathf.Abs(deltaPlayer.y) >= Mathf.Abs(deltaPlayer.z);
    }

    private Vector3 PosicionEnPlano(Vector3 posicion)
    {
        if (usarPlanoXY)
        {
            return new Vector3(posicion.x, posicion.y, 0f);
        }

        return new Vector3(posicion.x, 0f, posicion.z);
    }

    private Vector3 AjustarObjetivoAlPlano(Vector3 objetivo, Vector3 posicionActual)
    {
        if (usarPlanoXY)
        {
            return new Vector3(objetivo.x, objetivo.y, posicionActual.z);
        }

        return new Vector3(objetivo.x, posicionActual.y, objetivo.z);
    }

    private void MostrarAlegria()
    {
        if (spriteAlegria == null)
        {
            return;
        }

        if (rutinaAlegria != null)
        {
            StopCoroutine(rutinaAlegria);
        }

        rutinaAlegria = StartCoroutine(RutinaAlegria());
    }

    private void MostrarColorCancelado()
    {
        if (spriteRendererColor == null)
        {
            return;
        }

        if (rutinaColorCancelado != null)
        {
            StopCoroutine(rutinaColorCancelado);
        }

        rutinaColorCancelado = StartCoroutine(RutinaColorCancelado());
    }

    private IEnumerator RutinaColorCancelado()
    {
        spriteRendererColor.color = colorCancelado;
        yield return new WaitForSeconds(duracionColorCancelado);

        if (spriteRendererColor != null)
        {
            spriteRendererColor.color = colorOriginal;
        }

        rutinaColorCancelado = null;
    }

    private IEnumerator RutinaAlegria()
    {
        if (spriteActual != null)
        {
            spriteActual.SetActive(false);
        }

        spriteAlegria.SetActive(true);

        Transform spriteTransform = spriteAlegria.transform;
        Vector3 posicionInicial = spriteTransform.localPosition;
        float tiempo = 0f;

        while (tiempo < duracionAlegria)
        {
            tiempo += Time.deltaTime;
            float salto = Mathf.Abs(Mathf.Sin(tiempo * velocidadSaltito)) * alturaSaltito;
            spriteTransform.localPosition = posicionInicial + new Vector3(0f, salto, 0f);
            yield return null;
        }

        spriteTransform.localPosition = posicionInicial;
        spriteAlegria.SetActive(false);

        if (spriteActual != null)
        {
            spriteActual.SetActive(true);
        }
    }

    private void InstanciarPrefabUnico()
    {
        if (prefabInstanciable == null)
        {
            return;
        }

        PosBattlerDropZone dropZoneLibre = BuscarDropZoneLibre();

        if (dropZoneLibre == null)
        {
            Debug.Log("AliadeNPC: no hay PosBattlerDropZone libre para generar el prefab.");
            return;
        }

        if (prefabCanvasParent != null)
        {
            instanciaPrefabUnica = Instantiate(prefabInstanciable, prefabCanvasParent, false);
            instanciaPrefabUnica.transform.localRotation = Quaternion.Euler(prefabRotacionEuler);
            instanciaPrefabUnica.transform.localScale = prefabEscala;
            PosicionarPrefabEnDropZoneCanvas(dropZoneLibre);
        }
        else
        {
            Vector3 posicion = dropZoneLibre.GetDropPosition(transform.position);
            instanciaPrefabUnica = Instantiate(prefabInstanciable, posicion, Quaternion.Euler(prefabRotacionEuler));
            instanciaPrefabUnica.transform.SetParent(null);
            instanciaPrefabUnica.transform.localScale = prefabEscala;
        }

        instanciaPrefabUnica.layer = Mathf.Clamp(prefabLayer, 0, 31);
        AplicarSorting(instanciaPrefabUnica);
        duenoPrefab = this;

        arrastrandoPrefab = false;
        prefabAutoColocadoEnDropZone = true;
        RegistrarPrefabEnDropZone(dropZoneLibre);
    }

    private PosBattlerDropZone BuscarDropZoneLibre()
    {
        PosBattlerDropZone[] opcionesAsignadas = new PosBattlerDropZone[]
        {
            autoColocadoOpcion1,
            autoColocadoOpcion2,
            autoColocadoOpcion3,
            autoColocadoOpcion4
        };

        for (int i = 0; i < opcionesAsignadas.Length; i++)
        {
            if (opcionesAsignadas[i] != null && opcionesAsignadas[i].IsAvailable())
            {
                return opcionesAsignadas[i];
            }
        }

        for (int i = 0; i < opcionesAsignadas.Length; i++)
        {
            if (opcionesAsignadas[i] != null)
            {
                return null;
            }
        }

        PosBattlerDropZone[] dropZones = FindObjectsOfType<PosBattlerDropZone>();

        for (int i = 0; i < dropZones.Length; i++)
        {
            if (dropZones[i] != null && dropZones[i].IsAvailable())
            {
                return dropZones[i];
            }
        }

        return null;
    }

    private void RegistrarPrefabEnDropZone(PosBattlerDropZone dropZone)
    {
        if (instanciaPrefabUnica == null || dropZone == null)
        {
            return;
        }

        DragAndDrop dragAndDrop = instanciaPrefabUnica.GetComponent<DragAndDrop>();

        if (dragAndDrop == null)
        {
            dragAndDrop = instanciaPrefabUnica.AddComponent<DragAndDrop>();
        }

        if (PrefabEstaEnCanvas())
        {
            dragAndDrop.SetDropZone(dropZone);
            dropZone.SetUnit(dragAndDrop);
        }
        else
        {
            dragAndDrop.ConfirmDrop(dropZone);
        }

        AutoDestroyWhenRemovedFromDropZone monitor = instanciaPrefabUnica.GetComponent<AutoDestroyWhenRemovedFromDropZone>();

        if (monitor == null)
        {
            monitor = instanciaPrefabUnica.AddComponent<AutoDestroyWhenRemovedFromDropZone>();
        }

        monitor.Initialize(this, dragAndDrop, dropZone);
    }

    public void NotifyGeneratedPrefabDestroyed(GameObject prefab, DragAndDrop dragAndDrop, PosBattlerDropZone dropZone)
    {
        if (dropZone != null)
        {
            dropZone.ClearUnit(dragAndDrop);
        }

        if (instanciaPrefabUnica == prefab)
        {
            instanciaPrefabUnica = null;
            duenoPrefab = null;
            arrastrandoPrefab = false;
            prefabAutoColocadoEnDropZone = false;
            rutinaPrefab = null;
        }
    }

    private void ActualizarPrefabInstanciado()
    {
        if (duenoPrefab != this || instanciaPrefabUnica == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1) && MouseSobrePrefab())
        {
            DestruirPrefabInstanciado(true);
            return;
        }

        if (prefabAutoColocadoEnDropZone)
        {
            return;
        }

        if (DebeArrastrarPrefabInternamente() && Input.GetMouseButtonDown(0) && MouseSobrePrefab())
        {
            arrastrandoPrefab = true;

            if (PrefabEstaEnCanvas())
            {
                offsetArrastrePrefabUI = ObtenerPrefabAnchoredPosition() - ObtenerMouseEnCanvas();
            }
            else
            {
                offsetArrastrePrefab = instanciaPrefabUnica.transform.position - ObtenerMouseEnMundo();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            arrastrandoPrefab = false;
        }

        if (DebeArrastrarPrefabInternamente() && arrastrandoPrefab && Input.GetMouseButton(0))
        {
            if (PrefabEstaEnCanvas())
            {
                MoverPrefabEnCanvas(ObtenerMouseEnCanvas() + offsetArrastrePrefabUI);
            }
            else
            {
                instanciaPrefabUnica.transform.position = ObtenerMouseEnMundo() + offsetArrastrePrefab;
            }
        }
    }

    private bool MouseSobrePrefab()
    {
        if (instanciaPrefabUnica == null)
        {
            return false;
        }

        if (PrefabEstaEnCanvas() && MouseSobrePrefabUI())
        {
            return true;
        }

        if (camaraPrincipal == null)
        {
            return false;
        }

        Ray ray = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits3D = Physics.RaycastAll(ray);

        for (int i = 0; i < hits3D.Length; i++)
        {
            if (hits3D[i].collider != null && hits3D[i].collider.transform.IsChildOf(instanciaPrefabUnica.transform))
            {
                return true;
            }
        }

        Vector3 mouseWorld = ObtenerMouseEnMundo();
        Collider2D[] hits2D = Physics2D.OverlapPointAll(mouseWorld);

        for (int i = 0; i < hits2D.Length; i++)
        {
            if (hits2D[i] != null && hits2D[i].transform.IsChildOf(instanciaPrefabUnica.transform))
            {
                return true;
            }
        }

        return false;
    }

    private bool MouseSobrePrefabUI()
    {
        RectTransform[] rectTransforms = instanciaPrefabUnica.GetComponentsInChildren<RectTransform>();
        Camera uiCamera = ObtenerCamaraCanvas();

        for (int i = 0; i < rectTransforms.Length; i++)
        {
            if (rectTransforms[i] != null &&
                RectTransformUtility.RectangleContainsScreenPoint(rectTransforms[i], Input.mousePosition, uiCamera))
            {
                return true;
            }
        }

        return false;
    }

    private Vector3 ObtenerMouseEnMundo()
    {
        if (camaraPrincipal == null)
        {
            camaraPrincipal = Camera.main;
        }

        if (camaraPrincipal == null)
        {
            return transform.position;
        }

        Vector3 mouse = Input.mousePosition;
        mouse.z = prefabDistanciaDesdeCamara;
        return camaraPrincipal.ScreenToWorldPoint(mouse);
    }

    private void AplicarSorting(GameObject objetivo)
    {
        SpriteRenderer[] renderers = objetivo.GetComponentsInChildren<SpriteRenderer>();
        string sortingLayer = prefabSortingLayer;
        int sortingOrder = prefabSortingOrder;

        if (usarSortingDelAliade && spriteRendererColor != null)
        {
            sortingLayer = spriteRendererColor.sortingLayerName;
            sortingOrder = spriteRendererColor.sortingOrder + prefabSortingOrderOffset;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (!string.IsNullOrEmpty(sortingLayer))
            {
                renderers[i].sortingLayerName = sortingLayer;
            }

            renderers[i].sortingOrder = sortingOrder;
        }
    }

    private IEnumerator DesvanecerYDestruirPrefab()
    {
        yield return new WaitForSeconds(prefabDuracionAntesDesvanecer);

        if (instanciaPrefabUnica == null || duenoPrefab != this)
        {
            yield break;
        }

        if (PrefabEstaEnDropZone())
        {
            rutinaPrefab = null;
            yield break;
        }

        SpriteRenderer[] spriteRenderers = instanciaPrefabUnica.GetComponentsInChildren<SpriteRenderer>();
        Graphic[] graphics = instanciaPrefabUnica.GetComponentsInChildren<Graphic>();
        Color[] coloresSprite = new Color[spriteRenderers.Length];
        Color[] coloresGraphic = new Color[graphics.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            coloresSprite[i] = spriteRenderers[i].color;
        }

        for (int i = 0; i < graphics.Length; i++)
        {
            coloresGraphic[i] = graphics[i].color;
        }

        float duracion = Mathf.Max(0.01f, prefabTiempoDesvanecer);
        float tiempo = 0f;

        while (tiempo < duracion && instanciaPrefabUnica != null)
        {
            if (PrefabEstaEnDropZone())
            {
                RestaurarAlphaPrefab(spriteRenderers, coloresSprite, graphics, coloresGraphic);
                rutinaPrefab = null;
                yield break;
            }

            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempo / duracion);

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    Color color = coloresSprite[i];
                    color.a *= alpha;
                    spriteRenderers[i].color = color;
                }
            }

            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] != null)
                {
                    Color color = coloresGraphic[i];
                    color.a *= alpha;
                    graphics[i].color = color;
                }
            }

            yield return null;
        }

        DestruirPrefabInstanciado(false);
    }

    private Vector3 ObtenerCentroPantalla()
    {
        if (camaraPrincipal == null)
        {
            camaraPrincipal = Camera.main;
        }

        if (camaraPrincipal == null)
        {
            return transform.position;
        }

        float distancia = prefabDistanciaDesdeCamara;

        if (player != null)
        {
            distancia = camaraPrincipal.WorldToScreenPoint(player.position).z;
        }
        else
        {
            distancia = camaraPrincipal.WorldToScreenPoint(transform.position).z;
        }

        if (distancia <= 0f)
        {
            distancia = prefabDistanciaDesdeCamara;
        }

        return camaraPrincipal.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, distancia));
    }

    private void PosicionarPrefabEnCentroCanvas()
    {
        Vector2 posicion = new Vector2(prefabOffsetDesdeCentroPantalla.x, prefabOffsetDesdeCentroPantalla.y);
        MoverPrefabEnCanvas(posicion);
    }

    private void PosicionarPrefabEnDropZoneCanvas(PosBattlerDropZone dropZone)
    {
        if (prefabCanvasParent == null || dropZone == null)
        {
            PosicionarPrefabEnCentroCanvas();
            return;
        }

        RectTransform dropRect = dropZone.GetComponent<RectTransform>();
        Vector2 localPoint;
        Camera uiCamera = ObtenerCamaraCanvas();

        if (dropRect != null)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, dropRect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                prefabCanvasParent,
                screenPoint,
                uiCamera,
                out localPoint);

            MoverPrefabEnCanvas(localPoint);
            return;
        }

        Vector3 worldPosition = dropZone.GetDropPosition(transform.position);
        Vector2 fallbackScreenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            prefabCanvasParent,
            fallbackScreenPoint,
            uiCamera,
            out localPoint);

        MoverPrefabEnCanvas(localPoint);
    }

    private void MoverPrefabEnCanvas(Vector2 anchoredPosition)
    {
        RectTransform rectTransform = instanciaPrefabUnica != null ? instanciaPrefabUnica.GetComponent<RectTransform>() : null;

        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            return;
        }

        if (instanciaPrefabUnica != null)
        {
            instanciaPrefabUnica.transform.localPosition = new Vector3(anchoredPosition.x, anchoredPosition.y, 0f);
        }
    }

    private Vector2 ObtenerPrefabAnchoredPosition()
    {
        RectTransform rectTransform = instanciaPrefabUnica != null ? instanciaPrefabUnica.GetComponent<RectTransform>() : null;

        if (rectTransform != null)
        {
            return rectTransform.anchoredPosition;
        }

        return instanciaPrefabUnica != null ? instanciaPrefabUnica.transform.localPosition : Vector2.zero;
    }

    private Vector2 ObtenerMouseEnCanvas()
    {
        if (prefabCanvasParent == null)
        {
            return Vector2.zero;
        }

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            prefabCanvasParent,
            Input.mousePosition,
            ObtenerCamaraCanvas(),
            out localPoint);

        return localPoint;
    }

    private Camera ObtenerCamaraCanvas()
    {
        if (prefabCanvasParent == null)
        {
            return camaraPrincipal;
        }

        Canvas canvas = prefabCanvasParent.GetComponentInParent<Canvas>();

        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera != null ? canvas.worldCamera : camaraPrincipal;
    }

    private bool PrefabEstaEnCanvas()
    {
        return prefabCanvasParent != null &&
            instanciaPrefabUnica != null &&
            instanciaPrefabUnica.transform.IsChildOf(prefabCanvasParent);
    }

    private bool PrefabEstaEnDropZone()
    {
        if (instanciaPrefabUnica == null)
        {
            return false;
        }

        DragAndDrop[] dragAndDrops = instanciaPrefabUnica.GetComponentsInChildren<DragAndDrop>();

        for (int i = 0; i < dragAndDrops.Length; i++)
        {
            if (dragAndDrops[i] != null && dragAndDrops[i].isOverDropZone)
            {
                return true;
            }
        }

        return false;
    }

    private bool PrefabTieneDragAndDrop()
    {
        return instanciaPrefabUnica != null && instanciaPrefabUnica.GetComponentInChildren<DragAndDrop>() != null;
    }

    private bool DebeArrastrarPrefabInternamente()
    {
        return !PrefabTieneDragAndDrop() || PrefabEstaEnCanvas();
    }

    private void RestaurarAlphaPrefab(
        SpriteRenderer[] spriteRenderers,
        Color[] coloresSprite,
        Graphic[] graphics,
        Color[] coloresGraphic)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null && i < coloresSprite.Length)
            {
                spriteRenderers[i].color = coloresSprite[i];
            }
        }

        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] != null && i < coloresGraphic.Length)
            {
                graphics[i].color = coloresGraphic[i];
            }
        }
    }

    private void DestruirPrefabInstanciado(bool detenerRutina)
    {
        if (detenerRutina && rutinaPrefab != null)
        {
            StopCoroutine(rutinaPrefab);
        }

        rutinaPrefab = null;

        if (instanciaPrefabUnica != null)
        {
            Destroy(instanciaPrefabUnica);
        }

        instanciaPrefabUnica = null;
        duenoPrefab = null;
        arrastrandoPrefab = false;
        prefabAutoColocadoEnDropZone = false;
    }

    private void MantenerCaminataActiva()
    {
        if (caminata == null)
        {
            return;
        }

        if (caminata.transform == transform)
        {
            caminata.enabled = false;
            caminata = null;
            return;
        }

        caminata.enabled = true;
    }

    private Caminata BuscarCaminataVisual()
    {
        Caminata[] caminatas = GetComponentsInChildren<Caminata>();

        for (int i = 0; i < caminatas.Length; i++)
        {
            if (caminatas[i] != null && caminatas[i].transform != transform)
            {
                return caminatas[i];
            }
        }

        return null;
    }

    private void BuscarPlayerSiHaceFalta()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
            ultimaPosicionPlayer = player.position;
        }
    }
}

class AutoDestroyWhenRemovedFromDropZone : MonoBehaviour
{
    private AliadeNPC owner;
    private DragAndDrop dragAndDrop;
    private PosBattlerDropZone dropZone;
    private bool destroying;

    public void Initialize(AliadeNPC aliadeOwner, DragAndDrop trackedDragAndDrop, PosBattlerDropZone trackedDropZone)
    {
        owner = aliadeOwner;
        dragAndDrop = trackedDragAndDrop;
        dropZone = trackedDropZone;
    }

    private void Update()
    {
        if (destroying || dragAndDrop == null || dropZone == null)
        {
            return;
        }

        if (dropZone.currentUnit != dragAndDrop || !dropZone.isOccupied)
        {
            DestroyGeneratedPrefab();
        }
    }

    private void OnDestroy()
    {
        if (owner != null)
        {
            owner.NotifyGeneratedPrefabDestroyed(gameObject, dragAndDrop, dropZone);
        }
    }

    private void DestroyGeneratedPrefab()
    {
        destroying = true;

        if (owner != null)
        {
            owner.NotifyGeneratedPrefabDestroyed(gameObject, dragAndDrop, dropZone);
        }

        Destroy(gameObject);
    }
}
