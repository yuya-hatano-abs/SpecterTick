using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Verifies Editor build settings scene registration. Does not guarantee standalone builds succeed.
    /// </summary>
    public sealed class BuildSettingsTests
    {
        [Test]
        public void BS1_SampleScene_IsEnabledInBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            var matchCount = 0;
            for (var i = 0; i < scenes.Length; i++)
            {
                var scene = scenes[i];
                if (scene.enabled && scene.path == "Assets/Scenes/SampleScene.unity")
                {
                    matchCount++;
                }
            }

            Assert.That(matchCount, Is.EqualTo(1));
        }

        [Test]
        public void BS2_EnabledScenes_AllExist()
        {
            var scenes = EditorBuildSettings.scenes;
            for (var i = 0; i < scenes.Length; i++)
            {
                var scene = scenes[i];
                if (!scene.enabled)
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                Assert.That(asset == null, Is.False);
            }
        }
    }
}
