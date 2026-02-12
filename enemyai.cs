using UnityEngine;
using System.Collections;
public class ShadowEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator anim;
    private Rigidbody2D rb;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float stopDistance = 1.3f;    // The "Duel" distance
    public float retreatDistance = 0.9f; // Backs off if you get too close
    public float chaseRange = 8f;        // Won't move unless you are this close

    [Header("Combat")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public int damage = 10;
    private float lastAttackTime;

    [Header("Health & Status")]
    public int health = 100;
    private bool isHurt = false;
    private bool isDead = false;
    
    [Header("Defense")]
public float blockChance = 0.35f;
public float blockDuration = 0.6f;
private bool isBlocking = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isDead || isHurt) return;

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
	PlayerAttack pScript = player.GetComponent<PlayerAttack>();
	if (pScript != null && distance <= attackRange + 0.5f)
	{
    	if (pScript.IsAttacking() && !isBlocking && Random.value < blockChance)
    	{
        StartCoroutine(BlockRoutine());
        return;
    	}
	}
    }

    void Move(float directionMultiplier)
    {
        float dir = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * moveSpeed * directionMultiplier, rb.linearVelocity.y);
        anim.SetBool("Run", true);
    }

    void StayStill()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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
        int attackType = Random.Range(1, 3); 
        anim.SetTrigger("Attack" + attackType);
    }

    public void DealDamage()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange + 0.3f)
        {
            PlayerAttack pScript = player.GetComponent<PlayerAttack>();
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

    if (isBlocking)
        dmg /= 3;

    health -= dmg;
    anim.SetTrigger("Hurt");
    
    if (health <= 0) Die();
    else StartCoroutine(HurtDelay());
}


    IEnumerator HurtDelay()
    {
        isHurt = true;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.4f);
        isHurt = false;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Die");
        this.enabled = false;
        GetComponent<Collider2D>().enabled = false; 
    }
IEnumerator BlockRoutine()
{
    isBlocking = true;
    StayStill();
    anim.SetBool("Block", true);

    yield return new WaitForSeconds(blockDuration);

    anim.SetBool("Block", false);
    isBlocking = false;
}

}