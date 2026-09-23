using NUnit.Framework;
using UnityEditor;

namespace MSP603.TowerDefense.Tests
{
    public sealed class ProjectEntryEditModeTests
    {
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
