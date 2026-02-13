using UnityEngine;
using System.Collections;

public class FinalEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator anim;
    private Rigidbody2D rb;
    private Collider2D col;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistance = 1.3f;
    public float retreatDistance = 0.9f;
    public float chaseRange = 8f;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.2f;
    public int damage = 20;
    private float lastAttackTime;

    [Header("Health")]
    public int health = 300;
    private bool isDead = false;
    private bool phaseTwo = false;

    [Header("Phase 2")]
    public GameObject orbPrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isDead || phaseTwo) return;

        float distance = Vector2.Distance(transform.position, player.position);

        FacePlayer();

        if (distance < chaseRange)
        {
            if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
            else if (distance < retreatDistance)
            {
                Move(-1);
            }
            else if (distance > stopDistance)
            {
                Move(1);
            }
            else
            {
                StayStill();
            }
        }
        else
        {
            StayStill();
        }
    }

    void Move(float directionMultiplier)
    {
        float dir = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * moveSpeed * directionMultiplier, rb.linearVelocity.y);

        if (anim != null)
            anim.SetBool("Run", true);
    }

    void StayStill()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (anim != null)
            anim.SetBool("Run", false);
    }

    void FacePlayer()
    {
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        StayStill();

        if (anim != null)
            anim.SetTrigger("Attack");
    }

    public void DealDamage()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange + 0.3f)
        {
            Combat pScript = player.GetComponent<Combat>();

            if (pScript != null)
            {
                Vector2 knockDir = (player.position - transform.position).normalized;
                pScript.TakeDamage(damage, knockDir);
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        health -= dmg;

        if (health <= 0)
        {
            if (!phaseTwo)
                EnterPhaseTwo();
            else
                FinalDeath();
        }
    }

    void EnterPhaseTwo()
    {
        phaseTwo = true;

        Debug.Log("PHASE 2 STARTED!");

        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Phase2");

        // Disable boss body collider
        if (col != null)
            col.enabled = false;

        // Spawn orb
        GameObject orb = Instantiate(orbPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);

        Orb orbScript = orb.GetComponent<Orb>();
        orbScript.boss = this;
    }

    public void FinalDeath()
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Die");

        col.enabled = false;

        Debug.Log("BOSS DEFEATED!");
    }
}
