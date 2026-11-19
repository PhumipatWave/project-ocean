using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public class GameSessionTests
    {
        private GameState saved;

        [SetUp] public void SetUp() => saved = GameSession.State;
        [TearDown] public void TearDown() => GameSession.State = saved;

        [Test]
        public void State_RoundTrips()
        {
            GameSession.State = GameState.Ocean;
            Assert.AreEqual(GameState.Ocean, GameSession.State);
            GameSession.State = GameState.Menu;
            Assert.AreEqual(GameState.Menu, GameSession.State);
        }
    }
}
