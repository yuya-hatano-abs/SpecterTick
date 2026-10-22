using SpecterTick.Core;

namespace SpecterTick.Presentation
{
    public static class InputIntentResolver
    {
        public static InputIntent Resolve(InputKeys keys)
        {
            if ((keys & InputKeys.KeyR) != 0)
            {
                return InputIntent.Retry;
            }

            if ((keys & InputKeys.Enter) != 0)
            {
                return InputIntent.ConfirmRecording;
            }

            if ((keys & InputKeys.Space) != 0)
            {
                return InputIntent.StepForward;
            }

            if ((keys & InputKeys.ArrowUp) != 0 || (keys & InputKeys.KeyW) != 0)
            {
                return InputIntent.Move(MoveDirection.Up);
            }

            if ((keys & InputKeys.ArrowDown) != 0 || (keys & InputKeys.KeyS) != 0)
            {
                return InputIntent.Move(MoveDirection.Down);
            }

            if ((keys & InputKeys.ArrowLeft) != 0 || (keys & InputKeys.KeyA) != 0)
            {
                return InputIntent.Move(MoveDirection.Left);
            }

            if ((keys & InputKeys.ArrowRight) != 0 || (keys & InputKeys.KeyD) != 0)
            {
                return InputIntent.Move(MoveDirection.Right);
            }

            return InputIntent.None;
        }
    }
}
