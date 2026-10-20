using System;
using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Tests.Stages;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class GridPuzzleEngineSessionDeterminismTests
    {
        private static readonly SwitchId Sa = new SwitchId("SA");
        private static readonly SwitchId Sb = new SwitchId("SB");
        private static readonly DoorId Da = new DoorId("DA");
        private static readonly DoorId Dab = new DoorId("DAB");

        private static readonly MoveDirection[] G13Recording =
        {
            MoveDirection.Down, MoveDirection.Down, MoveDirection.Down,
            MoveDirection.Left, MoveDirection.Left, MoveDirection.Left, MoveDirection.Left,
            MoveDirection.Right, MoveDirection.Right, MoveDirection.Right, MoveDirection.Right
        };

        private static readonly MoveDirection[] LcgDirectionsSeed1 =
        {
            MoveDirection.Up, MoveDirection.Up, MoveDirection.Left, MoveDirection.Right,
            MoveDirection.Up, MoveDirection.Up, MoveDirection.Left, MoveDirection.Left,
            MoveDirection.Up, MoveDirection.Down, MoveDirection.Down, MoveDirection.Up,
            MoveDirection.Right, MoveDirection.Down, MoveDirection.Down, MoveDirection.Up,
            MoveDirection.Right, MoveDirection.Left, MoveDirection.Right, MoveDirection.Down,
            MoveDirection.Right, MoveDirection.Left, MoveDirection.Right, MoveDirection.Left,
            MoveDirection.Right, MoveDirection.Left, MoveDirection.Up, MoveDirection.Left,
            MoveDirection.Up, MoveDirection.Right
        };

        [Test]
        public void T1_G13_ThreeSessions_AllTicksMatch_IncludingDerivedFlags()
        {
            var traceA = RunG13Trace(new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage()));
            var traceB = RunG13Trace(new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage()));
            var sessionC = new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
            RunG13Trace(sessionC);
            sessionC.Retry();
            var traceC = RunG13Trace(sessionC);

            AssertTracesEqual(traceA, traceB, "A vs B");
            AssertTracesEqual(traceA, traceC, "A vs C");

            RunG13Trace(new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage()));
            var finalA = traceA[traceA.Count - 1];
            Assert.That(finalA.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(finalA.State.Tick, Is.EqualTo(7));
            Assert.That(finalA.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(finalA.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(finalA.State.PlaybackIndex, Is.EqualTo(7));
            Assert.That(finalA.State.IsSwitchActivated(Sa), Is.True);
            Assert.That(finalA.State.IsSwitchActivated(Sb), Is.True);
            Assert.That(finalA.RecordedCommands.Count, Is.EqualTo(11));
            Assert.That(finalA.ExitOpen, Is.True);
        }

        [Test]
        public void T2_G13_F2AndF2R_AllTicksMatchStateAndHash()
        {
            var f2 = RunG13Trace(new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage()));
            var f2r = RunG13Trace(new GridPuzzleEngineSession(F2RStageDefinitionBuilder.CreateValidatedStage()));
            Assert.That(f2r.Count, Is.EqualTo(f2.Count));
            for (var i = 0; i < f2.Count; i++)
            {
                Assert.That(f2r[i].State, Is.EqualTo(f2[i].State), "State tick index " + i);
                Assert.That(f2r[i].State.GetHashCode(), Is.EqualTo(f2[i].State.GetHashCode()), "Hash tick index " + i);
                Assert.That(f2r[i].ExitOpen, Is.EqualTo(f2[i].ExitOpen), "ExitOpen index " + i);
                Assert.That(f2r[i].DoorA, Is.EqualTo(f2[i].DoorA), "DoorA index " + i);
                Assert.That(f2r[i].DoorAb, Is.EqualTo(f2[i].DoorAb), "DoorAb index " + i);
                Assert.That(f2r[i].AllSwitches, Is.EqualTo(f2[i].AllSwitches), "AllSwitches index " + i);
            }
        }

        [Test]
        public void T3_LcgDirectionLiterals_MatchGenerator()
        {
            var generated = GenerateLcgDirections(1u, 30);
            Assert.That(LcgDirectionsSeed1, Is.EqualTo(generated));
        }

        [Test]
        public void T3_Lcg_FirstFiveRecordingTicks_HandVerified()
        {
            var session = new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
            session.Step(LcgDirectionsSeed1[0]);
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(4, 4)));

            session.Step(LcgDirectionsSeed1[1]);
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);

            session.Step(LcgDirectionsSeed1[2]);
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 2)));

            session.Step(LcgDirectionsSeed1[3]);
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 2)));

            session.Step(LcgDirectionsSeed1[4]);
            Assert.That(session.State.Tick, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 3)));
            Assert.That(session.State.IsSwitchActivated(Sb), Is.False);
        }

        private static readonly GameState[] LcgCharacterizationGolden =
        {
            new GameState(1, new GridPosition(0, 1), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(2, new GridPosition(0, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(3, new GridPosition(0, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(4, new GridPosition(1, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(5, new GridPosition(1, 3), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(6, new GridPosition(1, 4), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(7, new GridPosition(0, 4), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(8, new GridPosition(0, 4), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(9, new GridPosition(0, 4), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(10, new GridPosition(0, 3), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(11, new GridPosition(0, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(12, new GridPosition(0, 3), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(13, new GridPosition(1, 3), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(14, new GridPosition(1, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(15, new GridPosition(1, 1), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(16, new GridPosition(1, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(17, new GridPosition(1, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(18, new GridPosition(0, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(19, new GridPosition(1, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(20, new GridPosition(1, 1), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa }), GameMode.Recording, 0),
            new GameState(21, new GridPosition(2, 1), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa, Sb }), GameMode.Recording, 0),
            new GameState(22, new GridPosition(1, 1), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa, Sb }), GameMode.Recording, 0),
            new GameState(23, new GridPosition(2, 1), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa, Sb }), GameMode.Recording, 0),
            new GameState(24, new GridPosition(1, 1), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa, Sb }), GameMode.Recording, 0),
            new GameState(25, new GridPosition(2, 1), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa, Sb }), GameMode.Recording, 0),
            new GameState(26, new GridPosition(1, 1), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa, Sb }), GameMode.Recording, 0),
            new GameState(27, new GridPosition(1, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa, Sb }), GameMode.Recording, 0),
            new GameState(28, new GridPosition(0, 2), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa, Sb }), GameMode.Recording, 0),
            new GameState(29, new GridPosition(0, 3), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa, Sb }), GameMode.Recording, 0),
            new GameState(30, new GridPosition(1, 3), new GridPosition(4, 4), new ActivatedSwitchSet(new[] { Sa, Sb }), GameMode.Recording, 0),
            new GameState(0, new GridPosition(1, 3), new GridPosition(4, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 0),
            new GameState(1, new GridPosition(1, 3), new GridPosition(4, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 1),
            new GameState(2, new GridPosition(1, 3), new GridPosition(4, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 2),
            new GameState(3, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 3),
            new GameState(4, new GridPosition(1, 3), new GridPosition(4, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 4),
            new GameState(5, new GridPosition(1, 3), new GridPosition(4, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 5),
            new GameState(6, new GridPosition(1, 3), new GridPosition(4, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 6),
            new GameState(7, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 7),
            new GameState(8, new GridPosition(1, 3), new GridPosition(2, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 8),
            new GameState(9, new GridPosition(1, 3), new GridPosition(2, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 9),
            new GameState(10, new GridPosition(1, 3), new GridPosition(2, 3), ActivatedSwitchSet.Empty, GameMode.Playback, 10),
            new GameState(11, new GridPosition(1, 3), new GridPosition(2, 3), ActivatedSwitchSet.Empty, GameMode.Playback, 11),
            new GameState(12, new GridPosition(1, 3), new GridPosition(2, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 12),
            new GameState(13, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 13),
            new GameState(14, new GridPosition(1, 3), new GridPosition(3, 3), ActivatedSwitchSet.Empty, GameMode.Playback, 14),
            new GameState(15, new GridPosition(1, 3), new GridPosition(3, 2), ActivatedSwitchSet.Empty, GameMode.Playback, 15),
            new GameState(16, new GridPosition(1, 3), new GridPosition(3, 3), ActivatedSwitchSet.Empty, GameMode.Playback, 16),
            new GameState(17, new GridPosition(1, 3), new GridPosition(4, 3), ActivatedSwitchSet.Empty, GameMode.Playback, 17),
            new GameState(18, new GridPosition(1, 3), new GridPosition(3, 3), ActivatedSwitchSet.Empty, GameMode.Playback, 18),
            new GameState(19, new GridPosition(1, 3), new GridPosition(4, 3), ActivatedSwitchSet.Empty, GameMode.Playback, 19),
            new GameState(20, new GridPosition(1, 3), new GridPosition(4, 2), ActivatedSwitchSet.Empty, GameMode.Playback, 20),
            new GameState(21, new GridPosition(1, 3), new GridPosition(4, 2), ActivatedSwitchSet.Empty, GameMode.Playback, 21),
            new GameState(22, new GridPosition(1, 3), new GridPosition(3, 2), ActivatedSwitchSet.Empty, GameMode.Playback, 22),
            new GameState(23, new GridPosition(1, 3), new GridPosition(4, 2), ActivatedSwitchSet.Empty, GameMode.Playback, 23),
            new GameState(24, new GridPosition(1, 3), new GridPosition(3, 2), ActivatedSwitchSet.Empty, GameMode.Playback, 24),
            new GameState(25, new GridPosition(1, 3), new GridPosition(4, 2), ActivatedSwitchSet.Empty, GameMode.Playback, 25),
            new GameState(26, new GridPosition(1, 3), new GridPosition(3, 2), ActivatedSwitchSet.Empty, GameMode.Playback, 26),
            new GameState(27, new GridPosition(1, 3), new GridPosition(3, 3), ActivatedSwitchSet.Empty, GameMode.Playback, 27),
            new GameState(28, new GridPosition(1, 3), new GridPosition(2, 3), ActivatedSwitchSet.Empty, GameMode.Playback, 28),
            new GameState(29, new GridPosition(1, 3), new GridPosition(2, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 29),
            new GameState(30, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
            new GameState(31, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
            new GameState(32, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
            new GameState(33, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
            new GameState(34, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
            new GameState(35, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
            new GameState(36, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
            new GameState(37, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
            new GameState(38, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
            new GameState(39, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
            new GameState(40, new GridPosition(1, 3), new GridPosition(3, 4), ActivatedSwitchSet.Empty, GameMode.Playback, 30),
        };

        // Characterization golden: captured from implementation after Task 06 Retry (seed 1, 30 record + confirm + playback until cleared or 40).
        [Test]
        public void T3_Lcg_CharacterizationStates_MatchImplementationGolden()
        {
            var trace = RunLcgFullTrace(new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage()));
            Assert.That(trace.Count, Is.EqualTo(LcgCharacterizationGolden.Length));
            for (var i = 0; i < LcgCharacterizationGolden.Length; i++)
            {
                Assert.That(trace[i].State, Is.EqualTo(LcgCharacterizationGolden[i]), "Characterization index " + i);
            }
        }

        [Test]
        public void T3_Lcg_ThreeSessions_ProduceIdenticalTraces()
        {
            var a = RunLcgFullTrace(new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage()));
            var b = RunLcgFullTrace(new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage()));
            var cSession = new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
            RunLcgFullTrace(cSession);
            cSession.Retry();
            var c = RunLcgFullTrace(cSession);
            AssertTracesEqual(a, b, "LCG A vs B");
            AssertTracesEqual(a, c, "LCG A vs C");

            var players = new HashSet<GridPosition>();
            var specters = new HashSet<GridPosition>();
            for (var i = 0; i < a.Count; i++)
            {
                players.Add(a[i].State.PlayerPosition);
                specters.Add(a[i].State.SpecterPosition);
            }

            Assert.That(players.Count, Is.GreaterThanOrEqualTo(4), "Distinct player positions");
            Assert.That(specters.Count, Is.GreaterThanOrEqualTo(4), "Distinct specter positions");
        }

        private static List<SessionSnapshot> RunG13Trace(GridPuzzleEngineSession session)
        {
            var trace = new List<SessionSnapshot>();
            for (var i = 0; i < G13Recording.Length; i++)
            {
                session.Step(G13Recording[i]);
                trace.Add(Capture(session));
            }

            session.ConfirmRecording();
            trace.Add(Capture(session));
            var playbackGuard = 0;
            while (session.State.Mode == GameMode.Playback && playbackGuard < 20)
            {
                session.StepPlayback();
                trace.Add(Capture(session));
                playbackGuard++;
            }

            if (session.State.Mode == GameMode.Cleared)
            {
                return trace;
            }

            while (session.State.Mode == GameMode.Playback && playbackGuard < 40)
            {
                session.StepPlayback();
                trace.Add(Capture(session));
                playbackGuard++;
            }

            return trace;
        }

        private static List<SessionSnapshot> RunLcgFullTrace(GridPuzzleEngineSession session)
        {
            var trace = new List<SessionSnapshot>();
            for (var i = 0; i < LcgDirectionsSeed1.Length; i++)
            {
                session.Step(LcgDirectionsSeed1[i]);
                trace.Add(Capture(session));
            }

            session.ConfirmRecording();
            trace.Add(Capture(session));
            var playbackSteps = 0;
            while (session.State.Mode != GameMode.Cleared && playbackSteps < 40)
            {
                if (session.State.Mode != GameMode.Playback)
                {
                    break;
                }

                session.StepPlayback();
                trace.Add(Capture(session));
                playbackSteps++;
            }

            return trace;
        }

        private static void AssertTracesEqual(List<SessionSnapshot> left, List<SessionSnapshot> right, string label)
        {
            Assert.That(right.Count, Is.EqualTo(left.Count), label + " count");
            for (var i = 0; i < left.Count; i++)
            {
                Assert.That(right[i].State, Is.EqualTo(left[i].State), label + " State " + i);
                Assert.That(right[i].RecordedCommands, Is.EqualTo(left[i].RecordedCommands), label + " Commands " + i);
                Assert.That(
                    left[i].RecordedCommands.Equals(right[i].RecordedCommands),
                    Is.True,
                    label + " RecordedCommands.Equals " + i);
                Assert.That(right[i].ExitOpen, Is.EqualTo(left[i].ExitOpen), label + " ExitOpen " + i);
                Assert.That(right[i].DoorA, Is.EqualTo(left[i].DoorA), label + " DoorA " + i);
                Assert.That(right[i].DoorAb, Is.EqualTo(left[i].DoorAb), label + " DoorAb " + i);
                Assert.That(right[i].AllSwitches, Is.EqualTo(left[i].AllSwitches), label + " AllSwitches " + i);
            }
        }

        private static SessionSnapshot Capture(GridPuzzleEngineSession session)
        {
            return new SessionSnapshot(
                session.State,
                session.RecordedCommands,
                session.IsExitOpen,
                session.IsDoorOpen(Da),
                session.IsDoorOpen(Dab),
                session.AllSwitchesActivated);
        }

        private static MoveDirection[] GenerateLcgDirections(uint seed, int count)
        {
            var directions = new MoveDirection[count];
            uint state = seed;
            for (var i = 0; i < count; i++)
            {
                state = unchecked(state * 1664525u + 1013904223u);
                var direction = 1 + (int)((state >> 16) & 3u);
                directions[i] = (MoveDirection)direction;
            }

            return directions;
        }

        private readonly struct SessionSnapshot
        {
            public SessionSnapshot(
                GameState state,
                RecordedCommandSequence recordedCommands,
                bool exitOpen,
                bool doorA,
                bool doorAb,
                bool allSwitches)
            {
                State = state;
                RecordedCommands = recordedCommands;
                ExitOpen = exitOpen;
                DoorA = doorA;
                DoorAb = doorAb;
                AllSwitches = allSwitches;
            }

            public GameState State { get; }
            public RecordedCommandSequence RecordedCommands { get; }
            public bool ExitOpen { get; }
            public bool DoorA { get; }
            public bool DoorAb { get; }
            public bool AllSwitches { get; }
        }
    }
}
