using UnityEngine;

public class FishHealth : MonoBehaviour
{
    public float maxHP = 100f;
    private float currentHP;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        Debug.Log("Fish HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Fish Dead");
        Destroy(gameObject);
    }
}