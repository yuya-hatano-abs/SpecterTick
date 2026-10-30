using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpecterTick.Presentation
{
    public sealed class BoardSkin
    {
        private static readonly string[] SpriteNamesInternal =
        {
            "Floor",
            "Wall",
            "Player",
            "Specter",
            "ExitClosed",
            "ExitOpen",
            "SwitchOff",
            "SwitchOn",
            "DoorClosed",
            "DoorOpen"
        };

        public BoardSkin(
            Sprite floor,
            Sprite wall,
            Sprite player,
            Sprite specter,
            Sprite exitClosed,
            Sprite exitOpen,
            Sprite switchOff,
            Sprite switchOn,
            Sprite doorClosed,
            Sprite doorOpen)
        {
            Floor = floor;
            Wall = wall;
            Player = player;
            Specter = specter;
            ExitClosed = exitClosed;
            ExitOpen = exitOpen;
            SwitchOff = switchOff;
            SwitchOn = switchOn;
            DoorClosed = doorClosed;
            DoorOpen = doorOpen;

            var missing = 0;
            if (floor == null)
            {
                missing++;
            }

            if (wall == null)
            {
                missing++;
            }

            if (player == null)
            {
                missing++;
            }

            if (specter == null)
            {
                missing++;
            }

            if (exitClosed == null)
            {
                missing++;
            }

            if (exitOpen == null)
            {
                missing++;
            }

            if (switchOff == null)
            {
                missing++;
            }

            if (switchOn == null)
            {
                missing++;
            }

            if (doorClosed == null)
            {
                missing++;
            }

            if (doorOpen == null)
            {
                missing++;
            }

            MissingCount = missing;
        }

        public Sprite Floor { get; }

        public Sprite Wall { get; }

        public Sprite Player { get; }

        public Sprite Specter { get; }

        public Sprite ExitClosed { get; }

        public Sprite ExitOpen { get; }

        public Sprite SwitchOff { get; }

        public Sprite SwitchOn { get; }

        public Sprite DoorClosed { get; }

        public Sprite DoorOpen { get; }

        public int MissingCount { get; }

        public static IReadOnlyList<string> SpriteNames => SpriteNamesInternal;

        public void Release()
        {
            DestroySprite(Floor);
            DestroySprite(Wall);
            DestroySprite(Player);
            DestroySprite(Specter);
            DestroySprite(ExitClosed);
            DestroySprite(ExitOpen);
            DestroySprite(SwitchOff);
            DestroySprite(SwitchOn);
            DestroySprite(DoorClosed);
            DestroySprite(DoorOpen);
        }

        private static void DestroySprite(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(sprite);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(sprite);
            }
        }
    }
}
