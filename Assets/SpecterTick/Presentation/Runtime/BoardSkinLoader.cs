using System;
using UnityEngine;

namespace SpecterTick.Presentation
{
    public static class BoardSkinLoader
    {
        public const string DefaultFolder = "Skin";

        public static BoardSkin Load()
        {
            return Load(DefaultFolder);
        }

        public static BoardSkin Load(string resourceFolder)
        {
            if (resourceFolder == null)
            {
                throw new ArgumentNullException(nameof(resourceFolder));
            }

            Sprite floor = null;
            Sprite wall = null;
            Sprite player = null;
            Sprite specter = null;
            Sprite exitClosed = null;
            Sprite exitOpen = null;
            Sprite switchOff = null;
            Sprite switchOn = null;
            Sprite doorClosed = null;
            Sprite doorOpen = null;

            for (var i = 0; i < BoardSkin.SpriteNames.Count; i++)
            {
                var name = BoardSkin.SpriteNames[i];
                var texture = Resources.Load<Texture2D>(resourceFolder + "/" + name);
                if (texture == null)
                {
                    continue;
                }

                var sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    texture.width);
                sprite.name = name;

                switch (name)
                {
                    case "Floor":
                        floor = sprite;
                        break;
                    case "Wall":
                        wall = sprite;
                        break;
                    case "Player":
                        player = sprite;
                        break;
                    case "Specter":
                        specter = sprite;
                        break;
                    case "ExitClosed":
                        exitClosed = sprite;
                        break;
                    case "ExitOpen":
                        exitOpen = sprite;
                        break;
                    case "SwitchOff":
                        switchOff = sprite;
                        break;
                    case "SwitchOn":
                        switchOn = sprite;
                        break;
                    case "DoorClosed":
                        doorClosed = sprite;
                        break;
                    case "DoorOpen":
                        doorOpen = sprite;
                        break;
                }
            }

            return new BoardSkin(
                floor,
                wall,
                player,
                specter,
                exitClosed,
                exitOpen,
                switchOff,
                switchOn,
                doorClosed,
                doorOpen);
        }
    }
}
