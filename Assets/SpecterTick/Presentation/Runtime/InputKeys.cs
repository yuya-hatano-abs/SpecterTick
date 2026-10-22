using System;

namespace SpecterTick.Presentation
{
    [Flags]
    public enum InputKeys : int
    {
        None = 0,
        ArrowUp = 1,
        ArrowDown = 2,
        ArrowLeft = 4,
        ArrowRight = 8,
        KeyW = 16,
        KeyA = 32,
        KeyS = 64,
        KeyD = 128,
        Enter = 256,
        Space = 512,
        KeyR = 1024
    }
}
