using Game.Core;
using Game.Gameplay;
using Game.UI;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class NotificationTests
    {
        private NotificationMessage received;

        private void Capture(NotificationMessage msg) => received = msg;

        [SetUp]
        public void SetUp()
        {
            received = null;
            NotificationManager.OnNotify += Capture;
        }

        [TearDown]
        public void TearDown() => NotificationManager.OnNotify -= Capture;

        [Test]
        public void Notify_WithoutChannel_UsesGeneral()
        {
            NotificationManager.Notify("hello");

            Assert.AreEqual("hello", received.Text);
            Assert.AreEqual(NotificationChannel.General, received.Channel);
            Assert.IsNull(received.Icon);
        }

        [Test]
        public void Notify_KeepsChannelAndIcon()
        {
            var icon = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.zero);

            NotificationManager.Notify("+1 Tuna", NotificationChannel.Resource, icon);

            Assert.AreEqual(NotificationChannel.Resource, received.Channel);
            Assert.AreSame(icon, received.Icon);
            Object.DestroyImmediate(icon);
        }

        [Test]
        public void Notify_EmptyText_SendsNothing()
        {
            NotificationManager.Notify("");

            Assert.IsNull(received);
        }

        [Test]
        public void Notify_StickyDefaultsOffAndPassesThrough()
        {
            NotificationManager.Notify("a");
            Assert.IsFalse(received.Sticky);

            NotificationManager.Notify("b", NotificationChannel.System, null, sticky: true);
            Assert.IsTrue(received.Sticky);
        }

        [Test]
        public void Clear_RaisesChannel()
        {
            NotificationChannel? cleared = null;
            void Capture(NotificationChannel c) => cleared = c;
            NotificationManager.OnClear += Capture;

            NotificationManager.Clear(NotificationChannel.System);

            NotificationManager.OnClear -= Capture;
            Assert.AreEqual(NotificationChannel.System, cleared);
        }

        [Test]
        public void PunchScale_StartsAndEndsAtOne_PeaksInMiddle()
        {
            Assert.AreEqual(1f, PunchScale.Evaluate(0f, 1.2f), 1e-4f);
            Assert.AreEqual(1.2f, PunchScale.Evaluate(0.5f, 1.2f), 1e-4f);
            Assert.AreEqual(1f, PunchScale.Evaluate(1f, 1.2f), 1e-4f);
        }

        [Test]
        public void Format_AddsSignAndName()
        {
            Assert.AreEqual("+1 Tuna", ResourceNotificationPresenter.Format(1, "Tuna"));
            Assert.AreEqual("-20 Money", ResourceNotificationPresenter.Format(-20, "Money"));
        }
    }
}
