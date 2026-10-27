using System;
using System.Collections.Generic;
using System.Text;
using SpecterTick.Core;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;

namespace SpecterTick.Presentation
{
    public static class StageCatalog
    {
        public const int Count = 5;

        public static string GetName(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Stage index is out of range.");
            }

            switch (index)
            {
                case 0:
                    return "WALK";
                case 1:
                    return "ECHO";
                case 2:
                    return "GATE";
                case 3:
                    return "TWIN";
                case 4:
                    return "TANGLE";
                default:
                    throw new InvalidOperationException("Unreachable stage index in GetName.");
            }
        }

        public static IStage CreateStage(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Stage index is out of range.");
            }

            StageDefinition definition;
            switch (index)
            {
                case 0:
                    definition = BuildWalkDefinition();
                    break;
                case 1:
                    definition = BuildEchoDefinition();
                    break;
                case 2:
                    definition = BuildGateDefinition();
                    break;
                case 3:
                    definition = BuildTwinDefinition();
                    break;
                case 4:
                    definition = BuildTangleDefinition();
                    break;
                default:
                    throw new InvalidOperationException("Unreachable stage index in CreateStage.");
            }

            var result = StageValidator.Validate(definition);
            if (!result.IsSuccess)
            {
                var message = new StringBuilder("Stage validation failed: ");
                for (var i = 0; i < result.Errors.Count; i++)
                {
                    if (i > 0)
                    {
                        message.Append("; ");
                    }

                    message.Append(result.Errors[i].Message);
                }

                throw new InvalidOperationException(message.ToString());
            }

            return result.Stage;
        }

        private static StageDefinition BuildWalkDefinition()
        {
            var definition = new StageDefinition
            {
                Width = 4,
                Height = 3,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(3, 0),
                ExitPosition = new GridPosition(3, 2)
            };
            definition.Walls.Add(new GridPosition(1, 1));
            definition.Walls.Add(new GridPosition(2, 1));
            return definition;
        }

        private static StageDefinition BuildEchoDefinition()
        {
            var definition = new StageDefinition
            {
                Width = 5,
                Height = 3,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(0, 2),
                ExitPosition = new GridPosition(4, 0)
            };
            definition.Walls.Add(new GridPosition(2, 1));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("S1"), new GridPosition(3, 2)));
            return definition;
        }

        private static StageDefinition BuildGateDefinition()
        {
            var definition = new StageDefinition
            {
                Width = 6,
                Height = 4,
                PlayerPosition = new GridPosition(2, 2),
                SpecterPosition = new GridPosition(3, 0),
                ExitPosition = new GridPosition(1, 0)
            };
            definition.Walls.Add(new GridPosition(2, 0));
            definition.Walls.Add(new GridPosition(2, 1));
            definition.Walls.Add(new GridPosition(4, 0));
            definition.Walls.Add(new GridPosition(4, 2));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("S1"), new GridPosition(0, 1)));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("D1"),
                new GridPosition(1, 3),
                new List<SwitchId> { new SwitchId("S1") }));
            return definition;
        }

        private static StageDefinition BuildTwinDefinition()
        {
            var definition = new StageDefinition
            {
                Width = 6,
                Height = 4,
                PlayerPosition = new GridPosition(0, 3),
                SpecterPosition = new GridPosition(0, 0),
                ExitPosition = new GridPosition(1, 0)
            };
            definition.Walls.Add(new GridPosition(0, 1));
            definition.Walls.Add(new GridPosition(0, 2));
            definition.Walls.Add(new GridPosition(2, 3));
            definition.Walls.Add(new GridPosition(5, 0));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("S1"), new GridPosition(4, 2)));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("S2"), new GridPosition(4, 3)));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("D1"),
                new GridPosition(3, 2),
                new List<SwitchId> { new SwitchId("S1") }));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("D2"),
                new GridPosition(3, 1),
                new List<SwitchId> { new SwitchId("S2") }));
            return definition;
        }

        private static StageDefinition BuildTangleDefinition()
        {
            var definition = new StageDefinition
            {
                Width = 6,
                Height = 4,
                PlayerPosition = new GridPosition(2, 2),
                SpecterPosition = new GridPosition(0, 0),
                ExitPosition = new GridPosition(0, 3)
            };
            definition.Walls.Add(new GridPosition(2, 0));
            definition.Walls.Add(new GridPosition(3, 2));
            definition.Walls.Add(new GridPosition(4, 2));
            definition.Walls.Add(new GridPosition(5, 1));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("S1"), new GridPosition(3, 1)));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("S2"), new GridPosition(4, 3)));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("D1"),
                new GridPosition(0, 1),
                new List<SwitchId> { new SwitchId("S1"), new SwitchId("S2") }));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("D2"),
                new GridPosition(1, 3),
                new List<SwitchId> { new SwitchId("S1"), new SwitchId("S2") }));
            return definition;
        }
    }
}
