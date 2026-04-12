using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject boidPrefab;
    public int boidCount = 100;
    public Vector3 boundaryArea = new Vector3(10, 10, 10);

    // เก็บรายชื่อ Boid ทั้งหมดไว้ที่นี่เพื่อให้ตัวละครแต่ละตัวดึงไปใช้ได้เร็วขึ้น
    public static List<Boid> allBoids = new List<Boid>();

    void Start()
    {
        for (int i = 0; i < boidCount; i++)
        {
            // สุ่มตำแหน่งเริ่มต้นภายใน Boundary
            Vector3 spawnPos = new Vector3(
                Random.Range(-boundaryArea.x / 2, boundaryArea.x / 2),
                Random.Range(-boundaryArea.y / 2, boundaryArea.y / 2),
                Random.Range(-boundaryArea.z / 2, boundaryArea.z / 2)
            );

            GameObject b = Instantiate(boidPrefab, spawnPos, Quaternion.identity, transform);
            allBoids.Add(b.GetComponent<Boid>());
        }
    }

    // วาดกล่อง Boundary ในหน้า Scene เพื่อให้เรามองเห็นขอบเขตได้ง่ายๆ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, boundaryArea);
    }
}