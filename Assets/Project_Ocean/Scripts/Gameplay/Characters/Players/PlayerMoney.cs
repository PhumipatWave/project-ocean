using UnityEngine;

namespace Game.Testing
{
    public class PlayerMoney : MonoBehaviour
    {
        public int money = 100;

        public void Add(int amount)
        {
            money += amount;
        }

        public bool Use(int amount)
        {
            if (money >= amount)
            {
                money -= amount;
                return true;
            }
            return false;
        }

    }
}
