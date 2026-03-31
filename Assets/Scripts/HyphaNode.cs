using System.Collections.Generic;
using UnityEngine;

public class HyphaNode
{
    public Vector2 position;
    public Vector2 direction;

    public float life;
    public float maxLife;
    public float targetRatio;

    public bool hasExtended;
    public bool hasCap;

    public HyphaNode parent;
    public LineRenderer line;

    public float noiseOffset;

    public List<Vector3> points = new List<Vector3>();

    public HyphaNode(Vector2 pos, Vector2 dir, float lifeTime, HyphaNode parentNode = null)
    {
        position = pos;
        direction = dir.normalized;

        maxLife = lifeTime;
        life = lifeTime;

        parent = parentNode;

        hasExtended = false;
        hasCap = false;

        targetRatio = (Random.value < 0.5f) ? 1f : 0.5f;

        noiseOffset = Random.Range(0f, 100f);
    }
}