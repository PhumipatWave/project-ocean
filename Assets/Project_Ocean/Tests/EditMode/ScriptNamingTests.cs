using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Game.Tests
{
    public class ScriptNamingTests
    {
        private const string Root = "Assets/Project_Ocean/Scripts";

        // Unity links a MonoBehaviour/ScriptableObject to its asset by file name == class name.
        private static readonly string[] Typos =
        {
            "Entitys", "Enemys", "Beharviours", "Partbraked", "Upgrads", "LoadingSecne",
            "MovePlayerDirsction", "DayAndResetEnemys", "Save Game", "Not User", "Draggableltem"
        };

        private static bool IsLive(string path) =>
            !path.Replace('\\', '/').Contains("/Deprecated/") && !path.EndsWith("InputSystem_Actions.cs");

        [Test]
        public void NoTypoFolderOrFileNames()
        {
            var entries = Directory.GetDirectories(Root, "*", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(Root, "*.cs", SearchOption.AllDirectories))
                .Where(IsLive);
            var bad = entries
                .Where(p => Typos.Contains(Path.GetFileNameWithoutExtension(p)))
                .ToList();
            Assert.IsEmpty(bad, string.Join("\n", bad));
        }

        [Test]
        public void ScriptFileNameMatchesADeclaredType()
        {
            var bad = Directory.GetFiles(Root, "*.cs", SearchOption.AllDirectories)
                .Where(IsLive)
                .Where(p => !Regex.IsMatch(File.ReadAllText(p),
                    $@"\b(class|struct|interface|enum|record)\s+{Regex.Escape(Path.GetFileNameWithoutExtension(p))}\b"))
                .ToList();
            Assert.IsEmpty(bad, string.Join("\n", bad));
        }
    }
}
