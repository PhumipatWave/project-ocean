using UnityEngine;

public class BodyPart : MonoBehaviour
{
    public FishHealth fish; // อ้างอิงไปตัวหลัก
    public float damageMultiplier = 1f;

    public void TakeHit(float baseDamage)
    {
        float finalDamage = baseDamage * damageMultiplier;
        fish.TakeDamage(finalDamage);
    }
}