using System.Collections;
using UnityEngine;

public class EnemyBird : MonoBehaviour
{
    [Header("Movimiento ZigZag")]
    public float speed = 2f;
    public float zigzagAmplitude = 1f;
    public float zigzagFrequency = 2f;

    [Header("Ataque")]
    public float attackSpeed = 6f;
    public float minAttackDelay = 1f;
    public float maxAttackDelay = 10f;

    [Header("Respawn")]
    public float respawnTime = 3f;

    [Header("Límite de distancia")]
    public float maxDistanceFromStart = 8f;

    private Vector3 startPosition;
    private Transform player;

    private bool playerInRange = false;
    private bool isAttacking = false;
    private bool isRespawning = false;
    private bool isReturning = false;

    private float attackTimer;
    private float randomAttackTime;

    private SpriteRenderer spriteRenderer;
    private Collider2D[] colliders;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();

        SetRandomAttackTime();
    }

    void Update()
    {
        if (isRespawning) return;

        // 🔴 Si se aleja demasiado → volver
        float distance = Vector2.Distance(transform.position, startPosition);
        if (distance > maxDistanceFromStart && !isReturning)
        {
            StartReturn();
        }

        if (isReturning)
        {
            ReturnToStart();
            return;
        }

        if (isAttacking)
        {
            AttackPlayer();
        }
        else
        {
            ZigZagMovement();

            if (playerInRange)
            {
                attackTimer += Time.deltaTime;

                if (attackTimer >= randomAttackTime)
                {
                    StartAttack();
                }
            }
        }
    }

    void ZigZagMovement()
    {
        float zigzag = Mathf.Sin(Time.time * zigzagFrequency) * zigzagAmplitude;
        transform.position += new Vector3(speed * Time.deltaTime, zigzag * Time.deltaTime, 0);
    }

    void StartAttack()
    {
        if (player != null)
        {
            isAttacking = true;
            attackTimer = 0f;
        }
    }

    void AttackPlayer()
    {
        if (player == null)
        {
            isAttacking = false;
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            attackSpeed * Time.deltaTime
        );
    }

    void StartReturn()
    {
        isReturning = true;
        isAttacking = false;
    }

    void ReturnToStart()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            startPosition,
            attackSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, startPosition) < 0.1f)
        {
            isReturning = false;

            if (playerInRange)
            {
                ResetAttackTimer();
            }
        }
    }

    void SetRandomAttackTime()
    {
        randomAttackTime = Random.Range(minAttackDelay, maxAttackDelay);
    }

    void ResetAttackTimer()
    {
        attackTimer = 0f;
        SetRandomAttackTime();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.transform;
            playerInRange = true;
            ResetAttackTimer();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        isAttacking = false;
        isReturning = false;

        spriteRenderer.enabled = false;
        foreach (var col in colliders)
            col.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        transform.position = startPosition;

        spriteRenderer.enabled = true;
        foreach (var col in colliders)
            col.enabled = true;

        isRespawning = false;

        if (playerInRange)
        {
            ResetAttackTimer();
        }
    }
}