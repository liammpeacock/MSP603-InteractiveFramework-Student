using NUnit.Framework;
using UnityEditor;

namespace MSP603.TowerDefense.Tests
{
    public sealed class ProjectEntryEditModeTests
    {
        [TestCase("Assets/InitTestScene-generated.unity", false, false)]
        [TestCase("Assets/Unrelated.unity", true, false)]
        [TestCase("Assets/Unrelated.unity", false, true)]
        [TestCase("Assets/MSP603/Scenes/MainMenu.unity", false, false)]
        [TestCase("Assets/MSP603/Scenes/Level01.unity", false, false)]
        public void SafeEntryPreservesTestBootstrapAndFrameworkScenes(string path, bool batchMode, bool useMenu)
        {
            Assert.That(Editor.MSP603PlayModeEntry.ShouldUseMainMenu(path, batchMode), Is.EqualTo(useMenu));
        }

        [Test]
        public void BuildStartsWithOnlyTheFrameworkScenesInOrder()
        {
            string[] expected =
            {
                "Assets/MSP603/Scenes/MainMenu.unity",
                "Assets/MSP603/Scenes/Level01.unity",
                "Assets/MSP603/Scenes/Level02.unity",
                "Assets/MSP603/Scenes/Level03.unity"
            };

            Assert.That(EditorBuildSettings.scenes, Has.Length.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(EditorBuildSettings.scenes[i].enabled, Is.True);
                Assert.That(EditorBuildSettings.scenes[i].path, Is.EqualTo(expected[i]));
            }
        }
    }
}
