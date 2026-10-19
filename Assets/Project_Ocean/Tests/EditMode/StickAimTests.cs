using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class StickAimTests
    {
        [Test]
        public void ResolveStickAim_FollowsStick_KeepsLastInsideDeadzone()
        {
            Vector2 last = Vector2.right;

            Vector2 up = PlayerGunHandler.ResolveStickAim(new Vector2(0f, 0.8f), last);
            Assert.AreEqual(Vector2.up, up);

            // A neutral or barely-touched stick keeps the previous aim, so letting go before firing doesn't swing it.
            Assert.AreEqual(up, PlayerGunHandler.ResolveStickAim(Vector2.zero, up));
            Assert.AreEqual(up, PlayerGunHandler.ResolveStickAim(new Vector2(0.2f, 0f), up));

            Vector2 diag = PlayerGunHandler.ResolveStickAim(new Vector2(0.5f, -0.5f), up);
            Assert.AreEqual(1f, diag.magnitude, 1e-5f);
            Assert.AreEqual(-45f, Mathf.Atan2(diag.y, diag.x) * Mathf.Rad2Deg, 1e-3f);
        }
    }
}
