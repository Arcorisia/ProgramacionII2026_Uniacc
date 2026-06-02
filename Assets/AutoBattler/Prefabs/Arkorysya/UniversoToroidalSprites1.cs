using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class UniversoToroidalSprites : MonoBehaviour
{
    [Header("Cantidad")]
    [Range(100, 10000)]
    public int cantidadParticulas = 3000;

    [Header("Forma")]
    public float radioEsfera = 10f;
    public float radioToroide = 4f;
    public float grosorToroide = 2f;

    [Header("Movimiento")]
    public float velocidadOrbita = 1f;
    public float velocidadRuido = 0.5f;
    public float fuerzaCentro = 2f;

    [Header("Visual")]
    public Sprite spriteParticula;
    public Material materialSprite;

    [Range(0.001f, 1f)]
    public float escalaSprite = 0.05f;

    public Color colorParticulas = Color.white;

    [Header("Distribución")]
    [Range(0f, 1f)]
    public float porcentajeToroide = 0.7f;

    class Particula
    {
        public GameObject obj;
        public SpriteRenderer sr;

        public Vector3 posicion;
        public float offset;
        public bool esToroide;
    }

    private List<Particula> particulas = new List<Particula>();

    private Camera cam;

    void Start()
    {
        GenerarUniverso();
    }

    void OnEnable()
    {
        GenerarUniverso();
    }

    void OnValidate()
    {
        if (!Application.isPlaying) return;

        Limpiar();
        GenerarUniverso();
    }

    void GenerarUniverso()
    {
        cam = Camera.main;

        if (spriteParticula == null)
            return;

        Limpiar();

        for (int i = 0; i < cantidadParticulas; i++)
        {
            Particula p = new Particula();

            p.esToroide = Random.value < porcentajeToroide;
            p.offset = Random.Range(0f, 9999f);

            GameObject g = new GameObject("Particula_" + i);
            g.transform.SetParent(transform);

            SpriteRenderer sr = g.AddComponent<SpriteRenderer>();

            sr.sprite = spriteParticula;
            sr.color = colorParticulas;

            if (materialSprite != null)
                sr.material = materialSprite;

            g.transform.localScale = Vector3.one * escalaSprite;

            p.obj = g;
            p.sr = sr;

            if (!p.esToroide)
            {
                p.posicion = Random.insideUnitSphere * (radioEsfera * 0.2f);
            }

            particulas.Add(p);
        }
    }

    void Update()
    {
        if (cam == null)
            cam = Camera.main;

        float tiempo = Application.isPlaying
            ? Time.time
            : Time.realtimeSinceStartup;

        foreach (Particula p in particulas)
        {
            if (p.obj == null)
                continue;

            if (p.esToroide)
            {
                float t = tiempo * velocidadOrbita + p.offset;

                float anguloMayor = t;
                float anguloMenor = t * 2f;

                float r = radioToroide +
                          Mathf.Sin(anguloMenor + p.offset) * grosorToroide;

                float x = Mathf.Cos(anguloMayor) * r;
                float z = Mathf.Sin(anguloMayor) * r;
                float y = Mathf.Sin(anguloMenor) * grosorToroide;

                Vector3 posicionToroide = new Vector3(x, y, z);

                posicionToroide += new Vector3(
                    Mathf.PerlinNoise(p.offset, tiempo * velocidadRuido) - 0.5f,
                    Mathf.PerlinNoise(tiempo * velocidadRuido, p.offset) - 0.5f,
                    Mathf.PerlinNoise(p.offset + 5f, tiempo * velocidadRuido) - 0.5f
                ) * 0.5f;

                posicionToroide -= posicionToroide.normalized *
                                   fuerzaCentro * 0.01f;

                p.posicion = posicionToroide;
            }
            else
            {
                Vector3 dirCentro = -p.posicion.normalized;

                p.posicion += dirCentro * fuerzaCentro * Time.deltaTime;

                p.posicion += Random.insideUnitSphere * 0.002f;
            }

            p.obj.transform.localPosition = p.posicion;

            // Billboard hacia cámara
            if (cam != null)
            {
                p.obj.transform.forward =
                    cam.transform.forward;
            }

            // Escala editable en tiempo real
            p.obj.transform.localScale =
                Vector3.one * escalaSprite;
        }
    }

    void Limpiar()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        particulas.Clear();
    }
}