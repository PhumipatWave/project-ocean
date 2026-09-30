using System.Diagnostics;
using System.Linq;
using Game.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public class GameLogTests
    {
        [Test]
        public void Info_LogsInEditor()
        {
            LogAssert.Expect(LogType.Log, "hello");
            GameLog.Info("hello");
        }

        [Test]
        public void Info_IsConditionalOnEditorAndDevBuild()
        {
            var symbols = typeof(GameLog).GetMethod(nameof(GameLog.Info))
                .GetCustomAttributes(typeof(ConditionalAttribute), false)
                .Cast<ConditionalAttribute>()
                .Select(a => a.ConditionString);
            CollectionAssert.AreEquivalent(new[] { "UNITY_EDITOR", "DEVELOPMENT_BUILD" }, symbols);
        }
    }
}
