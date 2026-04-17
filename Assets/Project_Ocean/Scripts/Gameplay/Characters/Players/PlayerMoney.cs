using UnityEngine;

namespace Game.Testing
{
    public class PlayerMoney : MonoBehaviour
    {
        private int money = 100;
        public int Money { get { return money; } }

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
