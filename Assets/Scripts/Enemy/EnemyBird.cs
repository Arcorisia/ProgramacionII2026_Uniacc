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

    private Vector3 startPosition;
    private Transform player;
    private bool playerInRange = false;
    private bool isAttacking = false;
    private bool isReturning = false;

    private float attackTimer;
    private float randomAttackTime;

    void Start()
    {
        startPosition = transform.position;
        SetRandomAttackTime();
    }

    void Update()
    {
        if (isAttacking)
        {
            AttackPlayer();
        }
        else if (isReturning)
        {
            ReturnToStart();
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
            isAttacking = false;
            isReturning = true;
        }
    }
}