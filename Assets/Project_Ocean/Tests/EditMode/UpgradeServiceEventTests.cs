using Game.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class UpgradeServiceEventTests
    {
        private UpgradeData gun;
        private UpgradeData granted;
        private int grantedLevelSeen;
        private int loadedCalls;

        private void OnGranted(UpgradeData d)
        {
            granted = d;
            grantedLevelSeen = UpgradeService.Instance.GetLevel(d.type);
        }

        private void OnLoaded() => loadedCalls++;

        [SetUp]
        public void SetUp()
        {
            UpgradeService.Instance.ResetToDefault();
            gun = ScriptableObject.CreateInstance<UpgradeData>();
            gun.type = UpgradeType.NormalBullet;
            gun.maxLevel = 3;
            granted = null;
            grantedLevelSeen = -1;
            loadedCalls = 0;
            UpgradeService.OnLevelGranted += OnGranted;
            UpgradeService.OnLevelsLoaded += OnLoaded;
        }

        [TearDown]
        public void TearDown()
        {
            UpgradeService.OnLevelGranted -= OnGranted;
            UpgradeService.OnLevelsLoaded -= OnLoaded;
            UpgradeService.Instance.ResetToDefault();
            Object.DestroyImmediate(gun);
        }

        [Test]
        public void GrantingALevel_RaisesOnLevelGranted_BeforeTheLevelIncrements()
        {
            Assert.IsTrue(UpgradeService.Instance.DevGrantNextLevel(gun, out _));
            Assert.AreSame(gun, granted);
            Assert.AreEqual(0, grantedLevelSeen); // raised before the level increments
            Assert.AreEqual(1, UpgradeService.Instance.GetLevel(UpgradeType.NormalBullet));
        }

        [Test]
        public void AtMaxLevel_DoesNotRaise()
        {
            UpgradeService.Instance.SetLevel(UpgradeType.NormalBullet, 3);
            Assert.IsFalse(UpgradeService.Instance.DevGrantNextLevel(gun, out _));
            Assert.IsNull(granted);
        }

        [Test]
        public void ApplyingASave_RaisesOnLevelsLoaded_AfterLevelsAreSet()
        {
            var data = new SaveData { inventoryUpgradeLevel = 2, ropeUpgradeLevel = 1, normalBulletUpgradeLevel = 0 };
            ((ISaveParticipant)UpgradeService.Instance).Apply(data);
            Assert.AreEqual(1, loadedCalls);
            Assert.AreEqual(2, UpgradeService.Instance.GetLevel(UpgradeType.Inventory));
        }
    }
}
