using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Verifies InputKeys to InputIntent mapping and priority only — not keyboard polling (Keyboard.current), Dispatch, or on-screen movement.
    /// </summary>
    [TestFixture]
    public sealed class InputIntentResolverTests
    {
        [Test]
        public void K1_NoKeys_ReturnsNone()
        {
            var intent = InputIntentResolver.Resolve(InputKeys.None);
            Assert.That(intent.Kind, Is.EqualTo(InputIntentKind.None));
        }

        [Test]
        public void K2_EachKey_MapsToIntent()
        {
            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowUp).Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowUp).Direction, Is.EqualTo(MoveDirection.Up));

            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyW).Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyW).Direction, Is.EqualTo(MoveDirection.Up));

            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowDown).Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowDown).Direction, Is.EqualTo(MoveDirection.Down));

            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyS).Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyS).Direction, Is.EqualTo(MoveDirection.Down));

            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowLeft).Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowLeft).Direction, Is.EqualTo(MoveDirection.Left));

            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyA).Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyA).Direction, Is.EqualTo(MoveDirection.Left));

            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowRight).Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowRight).Direction, Is.EqualTo(MoveDirection.Right));

            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyD).Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyD).Direction, Is.EqualTo(MoveDirection.Right));

            Assert.That(InputIntentResolver.Resolve(InputKeys.Enter).Kind, Is.EqualTo(InputIntentKind.ConfirmRecording));
            Assert.That(InputIntentResolver.Resolve(InputKeys.Space).Kind, Is.EqualTo(InputIntentKind.StepForward));
            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyR).Kind, Is.EqualTo(InputIntentKind.Retry));
        }

        [Test]
        public void K3_Priority_Goldens()
        {
            var keys1 = InputKeys.KeyR | InputKeys.Enter | InputKeys.Space | InputKeys.ArrowUp;
            Assert.That(InputIntentResolver.Resolve(keys1).Kind, Is.EqualTo(InputIntentKind.Retry));

            var keys2 = InputKeys.Enter | InputKeys.Space | InputKeys.ArrowUp;
            Assert.That(InputIntentResolver.Resolve(keys2).Kind, Is.EqualTo(InputIntentKind.ConfirmRecording));

            var keys3 = InputKeys.Space | InputKeys.ArrowUp;
            Assert.That(InputIntentResolver.Resolve(keys3).Kind, Is.EqualTo(InputIntentKind.StepForward));

            var keys4 = InputKeys.ArrowUp | InputKeys.ArrowDown;
            var intent4 = InputIntentResolver.Resolve(keys4);
            Assert.That(intent4.Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(intent4.Direction, Is.EqualTo(MoveDirection.Up));

            var keys5 = InputKeys.ArrowDown | InputKeys.ArrowLeft;
            var intent5 = InputIntentResolver.Resolve(keys5);
            Assert.That(intent5.Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(intent5.Direction, Is.EqualTo(MoveDirection.Down));

            var keys6 = InputKeys.ArrowLeft | InputKeys.ArrowRight;
            var intent6 = InputIntentResolver.Resolve(keys6);
            Assert.That(intent6.Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(intent6.Direction, Is.EqualTo(MoveDirection.Left));

            var keys7 = InputKeys.KeyW | InputKeys.ArrowDown;
            var intent7 = InputIntentResolver.Resolve(keys7);
            Assert.That(intent7.Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(intent7.Direction, Is.EqualTo(MoveDirection.Up));

            var keys8 = InputKeys.ArrowUp | InputKeys.ArrowDown | InputKeys.ArrowLeft | InputKeys.ArrowRight;
            var intent8 = InputIntentResolver.Resolve(keys8);
            Assert.That(intent8.Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(intent8.Direction, Is.EqualTo(MoveDirection.Up));
        }
    }
}
