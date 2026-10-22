using SpecterTick.Core;

namespace SpecterTick.Presentation
{
    public enum InputIntentKind
    {
        None = 0,
        Move,
        ConfirmRecording,
        StepForward,
        Retry
    }

    public readonly struct InputIntent
    {
        public static readonly InputIntent None = new InputIntent(InputIntentKind.None, default(MoveDirection));
        public static readonly InputIntent ConfirmRecording = new InputIntent(InputIntentKind.ConfirmRecording, default(MoveDirection));
        public static readonly InputIntent StepForward = new InputIntent(InputIntentKind.StepForward, default(MoveDirection));
        public static readonly InputIntent Retry = new InputIntent(InputIntentKind.Retry, default(MoveDirection));

        private InputIntent(InputIntentKind kind, MoveDirection direction)
        {
            Kind = kind;
            Direction = direction;
        }

        public InputIntentKind Kind { get; }

        public MoveDirection Direction { get; }

        public static InputIntent Move(MoveDirection direction)
        {
            return new InputIntent(InputIntentKind.Move, direction);
        }
    }
}
