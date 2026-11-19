using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class PlayerInvincibilityTests
    {
        private GameObject go;
        private PlayerHealth health;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("PlayerInvincibilityTests");
            health = go.AddComponent<PlayerHealth>();
            health.Configure(100f);
            health.Initialize();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(go);

        [Test]
        public void SecondHitInsideWindow_IsIgnored()
        {
            health.TakeDamage(10f);
            health.TakeDamage(10f);

            Assert.AreEqual(90f, health.CurrentHealth);
            Assert.IsTrue(health.IsInvincible);
        }

        [Test]
        public void Initialize_ClearsWindow()
        {
            health.TakeDamage(10f);
            health.Initialize();

            Assert.IsFalse(health.IsInvincible);
            health.TakeDamage(10f);
            Assert.AreEqual(90f, health.CurrentHealth);
        }
    }
}
