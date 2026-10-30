using System;
using System.Collections.Generic;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Core.Stages;
using SpecterTick.Presentation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class BoardSkinLoaderTests
    {
        [Test]
        public void LD1_Load_NoSuchFolder_AllNull()
        {
            var skin = BoardSkinLoader.Load("NoSuchFolderTask16b");

            Assert.That(BoardSkinLoader.DefaultFolder, Is.EqualTo("Skin"));
            Assert.That(skin.MissingCount, Is.EqualTo(10));
            Assert.That(skin.Floor == null, Is.True);
            Assert.That(skin.Wall == null, Is.True);
            Assert.That(skin.Player == null, Is.True);
            Assert.That(skin.Specter == null, Is.True);
            Assert.That(skin.ExitClosed == null, Is.True);
            Assert.That(skin.ExitOpen == null, Is.True);
            Assert.That(skin.SwitchOff == null, Is.True);
            Assert.That(skin.SwitchOn == null, Is.True);
            Assert.That(skin.DoorClosed == null, Is.True);
            Assert.That(skin.DoorOpen == null, Is.True);
        }

        [Test]
        public void LD2_Load_NullFolder_Throws()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => BoardSkinLoader.Load(null));
            Assert.That(exception.ParamName, Is.EqualTo("resourceFolder"));
        }

        [Test]
        public void LD3_Load_RealAssets_AllPresent()
        {
            var skin = BoardSkinLoader.Load();

            Assert.That(skin.MissingCount, Is.EqualTo(0));

            for (var i = 0; i < BoardSkin.SpriteNames.Count; i++)
            {
                var expectedName = BoardSkin.SpriteNames[i];
                Sprite sprite = null;
                switch (expectedName)
                {
                    case "Floor":
                        sprite = skin.Floor;
                        break;
                    case "Wall":
                        sprite = skin.Wall;
                        break;
                    case "Player":
                        sprite = skin.Player;
                        break;
                    case "Specter":
                        sprite = skin.Specter;
                        break;
                    case "ExitClosed":
                        sprite = skin.ExitClosed;
                        break;
                    case "ExitOpen":
                        sprite = skin.ExitOpen;
                        break;
                    case "SwitchOff":
                        sprite = skin.SwitchOff;
                        break;
                    case "SwitchOn":
                        sprite = skin.SwitchOn;
                        break;
                    case "DoorClosed":
                        sprite = skin.DoorClosed;
                        break;
                    case "DoorOpen":
                        sprite = skin.DoorOpen;
                        break;
                }

                Assert.That(sprite == null, Is.False);
                Assert.That(sprite.name, Is.EqualTo(expectedName));
                Assert.That(sprite.texture.width, Is.EqualTo(256));
                Assert.That(sprite.texture.height, Is.EqualTo(256));
                Assert.That(sprite.bounds.size.x, Is.EqualTo(1f).Within(0.0001f));
                Assert.That(sprite.bounds.size.y, Is.EqualTo(1f).Within(0.0001f));
            }

            skin.Release();
        }
    }
}
