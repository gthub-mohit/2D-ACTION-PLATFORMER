using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    public Transform Attackpoint;
    public float attackrange = 0.5f;
    public LayerMask enemyLayer;
    public float damage = 10f;
    public float comboreset = 0.6f;
    private int combostep = 0;
    private float lastattacktime; 
    public float knockbackforce = 5f;
    public float hitstoptime = 0.05f;
    public Animator anim;
    private bool canattack = true;

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.J) && canattack)
        {
            HandleAttack();
        }

        if (Time.time - lastattacktime > comboreset)
        {
            combostep = 0;
        }
    }

    void HandleAttack()
    {
        canattack = false;
        lastattacktime = Time.time;
        combostep++;

        if (combostep > 5) combostep = 1;

        if (anim != null) anim.SetTrigger("Attack" + combostep);
    }

    public void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(Attackpoint.position, attackrange, enemyLayer);
        
        foreach (Collider2D enemy in enemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);

                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = (enemy.transform.position - transform.position).normalized;
                    rb.AddForce(dir * knockbackforce, ForceMode2D.Impulse); 
                }
                
                StartCoroutine(Hitstop());
            }
        }
    }

    IEnumerator Hitstop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitstoptime);
        Time.timeScale = 1f;
    }

    public void EndAttack()
    {
        canattack = true;
    }
}