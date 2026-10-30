using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class BoardSkinTests
    {
        private readonly List<Texture2D> _textures = new List<Texture2D>();

        [TearDown]
        public void TearDown()
        {
            for (var i = 0; i < _textures.Count; i++)
            {
                if (_textures[i] != null)
                {
                    Object.DestroyImmediate(_textures[i]);
                }
            }

            _textures.Clear();
        }

        [Test]
        public void SK1_Constructor_Properties_And_Names()
        {
            var skin = CreateFullSyntheticSkin(out var sprites);

            Assert.That(skin.Floor, Is.SameAs(sprites[0]));
            Assert.That(skin.Wall, Is.SameAs(sprites[1]));
            Assert.That(skin.Player, Is.SameAs(sprites[2]));
            Assert.That(skin.Specter, Is.SameAs(sprites[3]));
            Assert.That(skin.ExitClosed, Is.SameAs(sprites[4]));
            Assert.That(skin.ExitOpen, Is.SameAs(sprites[5]));
            Assert.That(skin.SwitchOff, Is.SameAs(sprites[6]));
            Assert.That(skin.SwitchOn, Is.SameAs(sprites[7]));
            Assert.That(skin.DoorClosed, Is.SameAs(sprites[8]));
            Assert.That(skin.DoorOpen, Is.SameAs(sprites[9]));
            Assert.That(skin.MissingCount, Is.EqualTo(0));

            var partial = new BoardSkin(
                sprites[0],
                null,
                sprites[2],
                null,
                sprites[4],
                sprites[5],
                sprites[6],
                sprites[7],
                sprites[8],
                null);
            Assert.That(partial.Wall == null, Is.True);
            Assert.That(partial.Specter == null, Is.True);
            Assert.That(partial.DoorOpen == null, Is.True);
            Assert.That(partial.Floor, Is.SameAs(sprites[0]));
            Assert.That(partial.Player, Is.SameAs(sprites[2]));
            Assert.That(partial.MissingCount, Is.EqualTo(3));

            var allNull = new BoardSkin(null, null, null, null, null, null, null, null, null, null);
            Assert.That(allNull.MissingCount, Is.EqualTo(10));

            var names = BoardSkin.SpriteNames;
            Assert.That(names.Count, Is.EqualTo(10));
            Assert.That(names[0], Is.EqualTo("Floor"));
            Assert.That(names[1], Is.EqualTo("Wall"));
            Assert.That(names[2], Is.EqualTo("Player"));
            Assert.That(names[3], Is.EqualTo("Specter"));
            Assert.That(names[4], Is.EqualTo("ExitClosed"));
            Assert.That(names[5], Is.EqualTo("ExitOpen"));
            Assert.That(names[6], Is.EqualTo("SwitchOff"));
            Assert.That(names[7], Is.EqualTo("SwitchOn"));
            Assert.That(names[8], Is.EqualTo("DoorClosed"));
            Assert.That(names[9], Is.EqualTo("DoorOpen"));

            skin.Release();
        }

        [Test]
        public void SK2_Release_DestroysSprites()
        {
            var skin = CreateFullSyntheticSkin(out var sprites);
            var texture = sprites[0].texture;
            Assert.That(texture == null, Is.False);

            skin.Release();

            for (var i = 0; i < sprites.Length; i++)
            {
                Assert.That(sprites[i] == null, Is.True);
            }

            Assert.That(texture == null, Is.False);
            Assert.That(skin.MissingCount, Is.EqualTo(0));
            Assert.DoesNotThrow(() => skin.Release());

            var partialSkin = new BoardSkin(
                CreateNamedSprite("FloorPartial"),
                null,
                CreateNamedSprite("PlayerPartial"),
                null,
                null,
                null,
                null,
                null,
                null,
                null);
            Assert.DoesNotThrow(() => partialSkin.Release());
        }

        private BoardSkin CreateFullSyntheticSkin(out Sprite[] sprites)
        {
            sprites = new Sprite[10];
            for (var i = 0; i < BoardSkin.SpriteNames.Count; i++)
            {
                sprites[i] = CreateNamedSprite(BoardSkin.SpriteNames[i]);
            }

            return new BoardSkin(
                sprites[0],
                sprites[1],
                sprites[2],
                sprites[3],
                sprites[4],
                sprites[5],
                sprites[6],
                sprites[7],
                sprites[8],
                sprites[9]);
        }

        private Sprite CreateNamedSprite(string name)
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            _textures.Add(texture);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
            sprite.name = name;
            return sprite;
        }
    }
}
