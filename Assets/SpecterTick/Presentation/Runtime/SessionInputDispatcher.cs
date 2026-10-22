using System;
using SpecterTick.Core;

namespace SpecterTick.Presentation
{
    public enum InputResult
    {
        None = 0,
        Executed,
        RejectedByMode
    }

    public sealed class SessionInputDispatcher
    {
        public SessionInputDispatcher(GridPuzzleEngineSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            Session = session;
        }

        public GridPuzzleEngineSession Session { get; }

        public InputResult Dispatch(InputIntent intent)
        {
            switch (intent.Kind)
            {
                case InputIntentKind.None:
                    return InputResult.None;

                case InputIntentKind.Retry:
                    Session.Retry();
                    return InputResult.Executed;

                case InputIntentKind.ConfirmRecording:
                    if (Session.State.Mode != GameMode.Recording)
                    {
                        return InputResult.RejectedByMode;
                    }

                    Session.ConfirmRecording();
                    return InputResult.Executed;

                case InputIntentKind.StepForward:
                    if (Session.State.Mode != GameMode.Playback)
                    {
                        return InputResult.RejectedByMode;
                    }

                    Session.StepPlayback();
                    return InputResult.Executed;

                case InputIntentKind.Move:
                    if (Session.State.Mode != GameMode.Recording)
                    {
                        return InputResult.RejectedByMode;
                    }

                    Session.Step(intent.Direction);
                    return InputResult.Executed;

                default:
                    throw new InvalidOperationException("The input intent kind is not defined.");
            }
        }
    }
}
