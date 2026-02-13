using UnityEngine;
using System.Collections;
public class Combat : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public int damage = 10;
    [Header("Combo Settings")]
    public float comboReset = 0.6f;
    private int comboStep = 0;
    private float lastAttackTime;
    private bool canAttack = true;
    [Header("Special Attack")]
    public float specialMeter = 0f;
    public float maxSpecial = 100f;
    public float specialFillPerHit = 10f;
    public float specialDamageMultiplier = 2.5f;
    private bool specialReady = false;

    [Header("Player Health")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isHurt = false;
    [Header("Feel Settings")]
    public float knockbackForce = 5f;
    public float hitStopTime = 0.05f;
    private bool isHitStopping = false;
    private bool isAttacking = false;

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
        if (Input.GetKeyDown(KeyCode.K) && specialReady)
        {
            PerformSpecialAttack();
        }
        if (Time.time - lastAttackTime > comboReset)
        {
            comboStep = 0;
        }
    }
    void HandleAttack()
    {
        isAttacking = true;
        canAttack = false;
        lastAttackTime = Time.time;
        comboStep++;
        // DealDamage(); 
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
        // 🔹 Normal Enemy
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage(damage);
            FillSpecialMeter();
        }

        // 🔹 Final Boss
        FinalEnemy bossScript = enemy.GetComponent<FinalEnemy>();
        if (bossScript != null)
        {
            bossScript.TakeDamage(damage);
            FillSpecialMeter();
        }

        // 🔹 Orb
        Orb orbScript = enemy.GetComponent<Orb>();
        if (orbScript != null)
        {
            orbScript.TakeDamage(damage);
        }

        // 🔹 Knockback (for anything that has Rigidbody)
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 dir = (enemy.transform.position - transform.position).normalized;
            enemyRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }

        // 🔹 Hit Stop
        if (!isHitStopping)
            StartCoroutine(HitStop());
    }

    Debug.Log("Hit detected");
}

    void FillSpecialMeter()
    {
        if (specialReady) return;

        specialMeter += specialFillPerHit;

        if (specialMeter >= maxSpecial)
        {
            specialMeter = maxSpecial;
            specialReady = true;
            Debug.Log("Special Ready!");
        }
    }
    void PerformSpecialAttack()
    {
        Debug.Log("SPECIAL ATTACK!");

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange + 1.5f,   // bigger radius
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                int finalDamage = Mathf.RoundToInt(damage * specialDamageMultiplier);
                enemyScript.TakeDamage(finalDamage);
            }
        }
        StartCoroutine(SpecialSlowMo());
        ResetSpecialMeter();
    }
    void ResetSpecialMeter()
    {
        specialMeter = 0f;
        specialReady = false;
    }
    IEnumerator SpecialSlowMo()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(0.2f);
        Time.timeScale = 1f;
    }


    public void EndAttack()
    {
        isAttacking = false;
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
    public bool IsAttacking()
    {
        return isAttacking;
    }

    IEnumerator HurtRoutine(Vector2 dir)
    {
        isHurt = true;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
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