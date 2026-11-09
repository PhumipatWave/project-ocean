namespace Game.Core
{
    /// <summary>Every scene name string, in one place.</summary>
    public static class SceneNames
    {
        public const string Land      = "Land_Map";
        public const string Ocean     = "Ocean_Map";
        public const string MainMenu  = "MainMenuScene";
        public const string Intro     = "IntroScene";

        /// <summary>True for scenes with a player (Land/Ocean); Bootstrap, Intro and the menu have none.</summary>
        public static bool IsGameplay(string sceneName) => sceneName == Land || sceneName == Ocean;
    }
}
