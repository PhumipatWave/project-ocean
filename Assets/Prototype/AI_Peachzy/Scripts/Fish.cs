using UnityEngine;

public class Fish : MonoBehaviour
{
    public float speed = 2f;
    public float boostSpeed = 5f;

    private Vector2 direction;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        PickRandomDirection();
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < 3f)
        {
            RunAway();
        }
        else
        {
            Swim();
        }
    }

    void Swim()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void RunAway()
    {
        Vector2 dir = (transform.position - player.position).normalized;
        transform.Translate(dir * boostSpeed * Time.deltaTime);
    }

    void PickRandomDirection()
    {
        direction = Random.insideUnitCircle.normalized;
        Invoke(nameof(PickRandomDirection), 3f);
    }
}