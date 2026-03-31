using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyceliumNetwork : MonoBehaviour
{
    [Header("Growth")]
    public float speed = 2f;
    public float lifeTime = 3f;

    [Header("Organic Motion")]
    public float noiseSpeed = 1f;
    public float curvature = 20f;
    public float upwardBias = 0.6f;

    [Header("Visual")]
    public Gradient branchColor;
    public float lineWidth = 0.05f;

    [Header("Caps")]
    public float capRadiusMin = 0.08f;
    public float capRadiusMax = 0.15f;
    public int capSegments = 12;
    public Color capColor = Color.white;
    public float capPopDuration = 0.2f;

    [Header("Lifecycle")]
    public float totalDuration = 8f;
    public float fadeDuration = 2f;

    private List<HyphaNode> nodes = new List<HyphaNode>();
    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<GameObject> caps = new List<GameObject>();

    private float timer;
    private bool isActive;
    private bool finalCapsSpawned = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Generate();

        if (!isActive) return;

        timer += Time.deltaTime;
        Grow();

        if (timer >= totalDuration)
        {
            if (!finalCapsSpawned)
            {
                SpawnFinalCaps();
                finalCapsSpawned = true;
            }

            StartCoroutine(FadeAndDestroy());
            isActive = false;
        }
    }

    void Generate()
    {
        Clear();

        timer = 0f;
        isActive = true;
        finalCapsSpawned = false;

        Vector2 origin = (Vector2)transform.position;

        int roots = Random.Range(1, 4);

        for (int i = 0; i < roots; i++)
        {
            float angle = i * (360f / roots) + Random.Range(-10f, 10f);
            Vector2 dir = Rotate(Vector2.right, angle);

            nodes.Add(new HyphaNode(origin, dir, lifeTime));
        }
    }

    void Grow()
    {
        List<HyphaNode> newNodes = new List<HyphaNode>();

        foreach (var node in nodes)
        {
            if (node.life <= 0f) continue;

            float noise = Mathf.PerlinNoise(Time.time * noiseSpeed, node.noiseOffset) - 0.5f;
            float angleOffset = noise * curvature;

            Vector2 dir = Rotate(node.direction, angleOffset);
            dir = (dir + Vector2.up * upwardBias).normalized;

            Vector2 newPos = node.position + dir * speed * Time.deltaTime;

            float normalized = (node.maxLife - node.life) / node.maxLife;

            if (normalized >= node.targetRatio)
            {
                if (!node.hasExtended && !node.hasCap && node.line != null)
                {
                    node.hasCap = true;
                    CreateCap(node.position, node.direction);
                }

                if (!node.hasExtended && node.targetRatio < 1f)
                {
                    node.hasExtended = true;

                    HyphaNode child = new HyphaNode(
                        node.position,
                        node.direction,
                        node.maxLife * 0.5f,
                        node
                    );

                    newNodes.Add(child);
                }

                continue;
            }

            if (node.line == null)
            {
                Vector2 start = node.parent != null ? node.parent.position : node.position;

                node.line = CreateLine(start);

                node.points.Add(new Vector3(start.x, start.y, 0));
                node.points.Add(new Vector3(newPos.x, newPos.y, 0));

                node.line.positionCount = node.points.Count;
                node.line.SetPositions(node.points.ToArray());

                lines.Add(node.line);
            }
            else
            {
                if (Vector2.Distance(node.position, newPos) > 0.02f)
                {
                    node.points.Add(new Vector3(newPos.x, newPos.y, 0));

                    node.line.positionCount = node.points.Count;
                    node.line.SetPositions(node.points.ToArray());
                }
            }

            node.position = newPos;
            node.direction = dir;
            node.life -= Time.deltaTime;
        }

        nodes.AddRange(newNodes);
    }

    void SpawnFinalCaps()
    {
        foreach (var node in nodes)
        {
            bool isTip = true;

            foreach (var other in nodes)
            {
                if (other.parent == node)
                {
                    isTip = false;
                    break;
                }
            }

            if (isTip && node.line != null && !node.hasCap)
            {
                node.hasCap = true;
                CreateCap(node.position, node.direction);
            }
        }
    }

    LineRenderer CreateLine(Vector2 start)
    {
        GameObject go = new GameObject("Branch");

        LineRenderer lr = go.AddComponent<LineRenderer>();

        lr.useWorldSpace = true;
        lr.positionCount = 1;

        lr.SetPosition(0, new Vector3(start.x, start.y, 0));

        float w = lineWidth * Random.Range(0.8f, 1.2f);

        lr.startWidth = w;
        lr.endWidth = w * 0.6f;

        lr.numCapVertices = 8;
        lr.numCornerVertices = 8;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.colorGradient = branchColor;

        return lr;
    }

    void CreateCap(Vector2 pos, Vector2 dir)
    {
        float radius = Random.Range(capRadiusMin, capRadiusMax);

        GameObject go = new GameObject("Cap");
        caps.Add(go);

        go.transform.position = new Vector3(pos.x, pos.y, 0);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();

        Mesh mesh = GenerateHalfCircleMesh(radius, capSegments);
        mf.mesh = mesh;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = capColor;
        mr.material = mat;

        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(0, 0, ang - 90f);

        go.transform.localScale = Vector3.zero;
        StartCoroutine(PopUp(go));
    }

    Mesh GenerateHalfCircleMesh(float radius, int seg)
    {
        Mesh mesh = new Mesh();

        Vector3[] v = new Vector3[seg + 2];
        int[] t = new int[seg * 3];

        v[0] = Vector3.zero;

        float step = Mathf.PI / seg;

        for (int i = 0; i <= seg; i++)
        {
            float a = i * step;
            v[i + 1] = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0);
        }

        for (int i = 0; i < seg; i++)
        {
            t[i * 3] = 0;
            t[i * 3 + 1] = i + 1;
            t[i * 3 + 2] = i + 2;
        }

        mesh.vertices = v;
        mesh.triangles = t;
        mesh.RecalculateNormals();

        return mesh;
    }

    IEnumerator PopUp(GameObject go)
    {
        float t = 0f;

        while (t < capPopDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / capPopDuration);
            go.transform.localScale = Vector3.one * s;
            yield return null;
        }

        go.transform.localScale = Vector3.one;
    }

    IEnumerator FadeAndDestroy()
    {
        float t = 0f;

        List<MeshRenderer> capRenderers = new List<MeshRenderer>();
        foreach (var c in caps)
            if (c) capRenderers.Add(c.GetComponent<MeshRenderer>());

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);

            foreach (var lr in lines)
            {
                if (!lr) continue;

                Gradient g = lr.colorGradient;
                var c = g.colorKeys;
                var al = g.alphaKeys;

                for (int i = 0; i < al.Length; i++)
                    al[i].alpha = a;

                Gradient ng = new Gradient();
                ng.SetKeys(c, al);
                lr.colorGradient = ng;
            }

            foreach (var r in capRenderers)
            {
                if (!r) continue;
                Color col = r.material.color;
                col.a = a;
                r.material.color = col;
            }

            yield return null;
        }

        Clear();
    }

    void Clear()
    {
        foreach (var l in lines)
            if (l) Destroy(l.gameObject);

        foreach (var c in caps)
            if (c) Destroy(c);

        lines.Clear();
        caps.Clear();
        nodes.Clear();
    }

    Vector2 Rotate(Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        float s = Mathf.Sin(r);
        float c = Mathf.Cos(r);

        return new Vector2(
            v.x * c - v.y * s,
            v.x * s + v.y * c
        );
    }
}