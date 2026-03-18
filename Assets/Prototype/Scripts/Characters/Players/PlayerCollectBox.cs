using UnityEngine;

namespace Game.Testing
{
    public class PlayerCollectBox : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            switch (collision.tag)
            {
                case "Bullet":
                    Debug.Log("Bullet pull");
                    if (collision.GetComponent<Bullet>().IsPullBack)
                    {
                        Debug.Log("Collect bullet");
                        Destroy(collision.gameObject);
                    }
                    break;

                case "Enemy":
                    if (collision.GetComponent<Enemy>().IsPullBack)
                    {
                        Debug.Log("Collect Fish");
                        Destroy(collision.gameObject);
                    }
                    break;
            }
        }
    }
}
