using System.Collections.Generic;
using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests
{
    public class TalkSpecificTests
    {
        private static QuestStep TalkStep(TalkCompletionMode mode, string passNpcId) => new QuestStep
        {
            Type = QuestType.Talk,
            TargetEnemyIds = new List<string> { "hunter", "shop", "femboy" },
            TalkCompletionMode = mode,
            PassNpcId = passNpcId,
            TargetCount = 1,
        };

        [Test]
        public void Specific_OnlyPassNpcPasses()
        {
            var step = TalkStep(TalkCompletionMode.Specific, "femboy");
            Assert.IsFalse(step.PassesOnTalk("hunter"));
            Assert.IsFalse(step.PassesOnTalk("shop"));
            Assert.IsTrue(step.PassesOnTalk("femboy"));
        }

        [Test]
        public void Specific_CompassPointsAtPassNpc_NotFirstInList() =>
            Assert.AreEqual("femboy", TalkStep(TalkCompletionMode.Specific, "femboy").NextTalkTargetNpcId);

        [Test]
        public void Specific_EmptyPassNpc_FallsBackToAnyOne()
        {
            var step = TalkStep(TalkCompletionMode.Specific, "");
            Assert.IsTrue(step.PassesOnTalk("hunter"));
            Assert.AreEqual("hunter", step.NextTalkTargetNpcId);
        }

        [Test]
        public void AnyOne_IgnoresPassNpc() =>
            Assert.IsTrue(TalkStep(TalkCompletionMode.AnyOne, "femboy").PassesOnTalk("hunter"));
    }
}
