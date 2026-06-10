using UnityEngine;

public class FishHealth : MonoBehaviour
{
    public float maxHP = 20f;
    private float currentHP;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float dmg)
    {
        currentHP -= dmg;

        Debug.Log("Fish HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Fish Dead!");
        Destroy(gameObject);
    }
}