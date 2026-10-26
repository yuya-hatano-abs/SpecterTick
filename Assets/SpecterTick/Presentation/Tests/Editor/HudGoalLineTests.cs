using System;
using System.Collections.Generic;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class HudGoalLineTests
    {
        [Test]
        public void GL1_Recording_ZeroOfTwo_Closed()
        {
            var directions = new[] { MoveDirection.Up, MoveDirection.Right, MoveDirection.Right };
            var model = HudModelBuilder.Build(GameMode.Recording, 3, 0, directions, 0, 2, false);

            Assert.That(model.ModeLine, Is.EqualTo("MODE: RECORDING"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 3"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 3"));
            Assert.That(model.GoalLine, Is.EqualTo("SWITCHES: 0 / 2  EXIT: CLOSED"));
            Assert.That(model.KeysLine, Is.EqualTo("MOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: U R R"));
            Assert.That(
                model.ToDisplayText(),
                Is.EqualTo("MODE: RECORDING\nTICK: 3\nRECORDED: 3\nSWITCHES: 0 / 2  EXIT: CLOSED\nMOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R\nCOMMANDS: U R R"));
        }

        [Test]
        public void GL2_Playback_OneOfTwo_Closed()
        {
            var directions = new[]
            {
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Right
            };
            var model = HudModelBuilder.Build(GameMode.Playback, 1, 1, directions, 1, 2, false);

            Assert.That(model.ModeLine, Is.EqualTo("MODE: PLAYBACK"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 1"));
            Assert.That(model.ProgressLine, Is.EqualTo("PLAYBACK: 1 / 5"));
            Assert.That(model.GoalLine, Is.EqualTo("SWITCHES: 1 / 2  EXIT: CLOSED"));
            Assert.That(model.KeysLine, Is.EqualTo("STEP: SPACE  RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: R R R R R"));
            Assert.That(
                model.ToDisplayText(),
                Is.EqualTo("MODE: PLAYBACK\nTICK: 1\nPLAYBACK: 1 / 5\nSWITCHES: 1 / 2  EXIT: CLOSED\nSTEP: SPACE  RESET: R\nCOMMANDS: R R R R R"));
        }

        [Test]
        public void GL3_Cleared_TwoOfTwo_Open()
        {
            var directions = new[]
            {
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Right
            };
            var model = HudModelBuilder.Build(GameMode.Cleared, 4, 4, directions, 2, 2, true);

            Assert.That(model.ModeLine, Is.EqualTo("MODE: CLEARED"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 4"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 5"));
            Assert.That(model.GoalLine, Is.EqualTo("SWITCHES: 2 / 2  EXIT: OPEN"));
            Assert.That(model.KeysLine, Is.EqualTo("RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: R R R R R"));
            Assert.That(
                model.ToDisplayText(),
                Is.EqualTo("MODE: CLEARED\nTICK: 4\nRECORDED: 5\nSWITCHES: 2 / 2  EXIT: OPEN\nRESET: R\nCOMMANDS: R R R R R"));
        }

        [Test]
        public void GL4_NoSwitches_ExitOpen()
        {
            var model = HudModelBuilder.Build(GameMode.Recording, 0, 0, Array.Empty<MoveDirection>(), 0, 0, true);

            Assert.That(model.ModeLine, Is.EqualTo("MODE: RECORDING"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 0"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 0"));
            Assert.That(model.GoalLine, Is.EqualTo("SWITCHES: 0 / 0  EXIT: OPEN"));
            Assert.That(model.KeysLine, Is.EqualTo("MOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: -"));
            Assert.That(
                model.ToDisplayText(),
                Is.EqualTo("MODE: RECORDING\nTICK: 0\nRECORDED: 0\nSWITCHES: 0 / 0  EXIT: OPEN\nMOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R\nCOMMANDS: -"));
        }

        [Test]
        public void GL5_FourArgBuild_HasNoGoalLine()
        {
            var directions = new[] { MoveDirection.Up, MoveDirection.Right, MoveDirection.Right };
            var model = HudModelBuilder.Build(GameMode.Recording, 3, 0, directions);

            Assert.That(model.GoalLine, Is.Null);
            Assert.That(model.ModeLine, Is.EqualTo("MODE: RECORDING"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 3"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 3"));
            Assert.That(model.KeysLine, Is.EqualTo("MOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: U R R"));
            Assert.That(
                model.ToDisplayText(),
                Is.EqualTo("MODE: RECORDING\nTICK: 3\nRECORDED: 3\nMOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R\nCOMMANDS: U R R"));
        }

        [Test]
        public void GL6_SevenArgBuild_InvalidArguments()
        {
            var directions = new[] { MoveDirection.Up };

            var nullDirections = Assert.Throws<ArgumentNullException>(() =>
                HudModelBuilder.Build(GameMode.Recording, 0, 0, (IReadOnlyList<MoveDirection>)null, 0, 2, false));
            Assert.That(nullDirections.ParamName, Is.EqualTo("recordedDirections"));

            var tick = Assert.Throws<ArgumentOutOfRangeException>(() =>
                HudModelBuilder.Build(GameMode.Recording, -1, 0, directions, 0, 2, false));
            Assert.That(tick.ParamName, Is.EqualTo("tick"));

            var playbackIndex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                HudModelBuilder.Build(GameMode.Recording, 0, -1, directions, 0, 2, false));
            Assert.That(playbackIndex.ParamName, Is.EqualTo("playbackIndex"));

            var activatedNegative = Assert.Throws<ArgumentOutOfRangeException>(() =>
                HudModelBuilder.Build(GameMode.Recording, 0, 0, directions, -1, 2, false));
            Assert.That(activatedNegative.ParamName, Is.EqualTo("activatedSwitches"));

            var totalNegative = Assert.Throws<ArgumentOutOfRangeException>(() =>
                HudModelBuilder.Build(GameMode.Recording, 0, 0, directions, 0, -1, false));
            Assert.That(totalNegative.ParamName, Is.EqualTo("totalSwitches"));

            var activatedExceeds = Assert.Throws<ArgumentOutOfRangeException>(() =>
                HudModelBuilder.Build(GameMode.Recording, 0, 0, directions, 3, 2, false));
            Assert.That(activatedExceeds.ParamName, Is.EqualTo("activatedSwitches"));

            var mode = Assert.Throws<ArgumentOutOfRangeException>(() =>
                HudModelBuilder.Build((GameMode)99, 0, 0, directions, 0, 2, false));
            Assert.That(mode.ParamName, Is.EqualTo("mode"));

            Assert.DoesNotThrow(() =>
                HudModelBuilder.Build(GameMode.Recording, 0, 0, Array.Empty<MoveDirection>(), 2, 2, false));

            Assert.DoesNotThrow(() =>
                HudModelBuilder.Build(GameMode.Recording, 0, 0, Array.Empty<MoveDirection>(), 0, 0, true));
        }
    }
}
