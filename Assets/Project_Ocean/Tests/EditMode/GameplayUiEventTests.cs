using Game.Core;
using Game.Gameplay;
using Game.UI;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class GameplayUiEventTests
    {
        [Test]
        public void DamagePoint_IsFoundThroughIDamagePopup()
        {
            var root = new GameObject("enemy");
            var child = new GameObject("popup");
            child.transform.SetParent(root.transform);
            child.AddComponent<DamagePopup>();
            try
            {
                Assert.IsNotNull(root.GetComponentInChildren<IDamagePopup>());
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GameplayEvents_Exist()
        {
            System.Action<Quest> q = _ => { };
            System.Action a = () => { };
            QuestManager.OnQuestCompleted += q;   QuestManager.OnQuestCompleted -= q;
            GameManager.OnReturningToMenu += a;   GameManager.OnReturningToMenu -= a;
            Player.OnPlayerDamaged += a;          Player.OnPlayerDamaged -= a;
            Assert.Pass();
        }
    }
}
