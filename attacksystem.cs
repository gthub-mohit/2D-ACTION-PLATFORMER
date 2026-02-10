using UnityEngine;
using System.Collections;
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public float damage = 10f;
    [Header("Combo Settings")]
    public float comboReset = 0.6f;
    private int comboStep = 0;
    private float lastAttackTime;
    private bool canAttack = true;
    [Header("Player Health")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isHurt = false;
    [Header("Feel Settings")]
    public float knockbackForce = 5f;
    public float hitStopTime = 0.05f;
    private bool isHitStopping = false;
    public Animator anim;
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && canAttack && !isHurt)
        {
            HandleAttack();
        }
        if (Time.time - lastAttackTime > comboReset)
        {
            comboStep = 0;
        }
    }
    void HandleAttack()
    {
        canAttack = false;
        lastAttackTime = Time.time;
        comboStep++;

        if (comboStep > 5) comboStep = 1;

        if (anim != null)
            anim.SetTrigger("Attack" + comboStep);
    }
    public void DealDamage()
    {
        if (attackPoint == null) return;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );
        foreach (Collider2D enemy in enemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 dir = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
                }
                if (!isHitStopping)
                    StartCoroutine(HitStop());
            }
        }
    }
    public void EndAttack()
    {
        canAttack = true;
    }
    public void TakeDamage(int damageAmount, Vector2 knockbackDir)
    {
        if (isHurt) return;
        currentHealth -= damageAmount;
        if (anim != null)
            anim.SetTrigger("Hurt");
        StartCoroutine(HurtRoutine(knockbackDir));
        if (currentHealth <= 0)
            Die();
    }
    IEnumerator HurtRoutine(Vector2 dir)
    {
        isHurt = true;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }
        yield return new WaitForSeconds(0.3f);
        isHurt = false;
    }
    void Die()
    {
        if (anim != null)
            anim.SetTrigger("Die");

        this.enabled = false;
    }
    IEnumerator HitStop()
    {
        isHitStopping = true;
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopTime);
        Time.timeScale = originalTimeScale;
        isHitStopping = false;
    }
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}