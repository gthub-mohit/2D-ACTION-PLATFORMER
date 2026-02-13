using UnityEngine;

public class Orb : MonoBehaviour
{
    public int health = 150;
    public FinalEnemy boss;

    public void TakeDamage(int dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            Destroy(gameObject);

            if (boss != null)
                boss.FinalDeath();
        }
    }
}
