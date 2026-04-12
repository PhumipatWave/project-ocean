using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 5f;
    public float neighborRadius = 3f;
    public float separationDistance = 1.5f;

    [Header("Coefficients")]
    public float coeffSeparation = 1f;
    public float coeffAlignment = 0.05f;
    public float coeffCohesion = 0.01f;

    private Vector2 velocity;

    void Start()
    {
        // สุ่มทิศทางเริ่มต้น
        velocity = Random.insideUnitCircle.normalized * speed;
    }

    void Update()
    {
        List<Boid> neighbors = GetNeighbors();

        Vector2 separation = CalculateSeparation(neighbors) * coeffSeparation;
        Vector2 alignment = CalculateAlignment(neighbors) * coeffAlignment;
        Vector2 cohesion = CalculateCohesion(neighbors) * coeffCohesion;

        // รวมแรงทั้งหมดเข้าด้วยกัน
        Vector2 acceleration = separation + alignment + cohesion;
        velocity = (velocity + acceleration).normalized * speed;

        // เคลื่อนที่
        transform.position += (Vector3)velocity * Time.deltaTime;

        // หันหน้าไปตามทิศทางที่เคลื่อนที่ (สำหรับ 2D)
        if (velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // 1. Separation: หลีกเลี่ยงการชนกับเพื่อนในฝูง
    Vector2 CalculateSeparation(List<Boid> neighbors)
    {
        Vector2 move = Vector2.zero;
        int count = 0;

        foreach (var neighbor in neighbors)
        {
            float dist = Vector2.Distance(transform.position, neighbor.transform.position);
            if (dist < separationDistance)
            {
                move += (Vector2)(transform.position - neighbor.transform.position).normalized / dist;
                count++;
            }
        }
        return count > 0 ? move / count : Vector2.zero;
    }

    // 2. Alignment: ปรับทิศทางให้ขนานไปกับเพื่อนรอบข้าง
    Vector2 CalculateAlignment(List<Boid> neighbors)
    {
        Vector2 avgVelocity = Vector2.zero;
        if (neighbors.Count == 0) return Vector2.zero;

        foreach (var neighbor in neighbors)
            avgVelocity += neighbor.velocity;

        avgVelocity /= neighbors.Count;
        return avgVelocity - velocity;
    }

    // 3. Cohesion: พยายามว่ายเข้าหาจุดศูนย์กลางของกลุ่ม
    Vector2 CalculateCohesion(List<Boid> neighbors)
    {
        Vector2 avgPosition = Vector2.zero;
        if (neighbors.Count == 0) return Vector2.zero;

        foreach (var neighbor in neighbors)
            avgPosition += (Vector2)neighbor.transform.position;

        avgPosition /= neighbors.Count;
        return avgPosition - (Vector2)transform.position;
    }

    // ค้นหาเพื่อนในฝูงที่อยู่ในระยะ Neighbor Radius
    List<Boid> GetNeighbors()
    {
        List<Boid> neighbors = new List<Boid>();
        Boid[] allBoids = Object.FindObjectsOfType<Boid>();

        foreach (var boid in allBoids)
        {
            if (boid != this && Vector2.Distance(transform.position, boid.transform.position) < neighborRadius)
            {
                neighbors.Add(boid);
            }
        }
        return neighbors;
    }
}