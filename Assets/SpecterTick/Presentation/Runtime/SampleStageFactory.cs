using System;
using System.Collections.Generic;
using SpecterTick.Core;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;

namespace SpecterTick.Presentation
{
    public static class SampleStageFactory
    {
        public static IStage Create()
        {
            var definition = new StageDefinition
            {
                Width = 5,
                Height = 4,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(1, 0),
                ExitPosition = new GridPosition(4, 3)
            };

            definition.Walls.Add(new GridPosition(2, 0));
            definition.Walls.Add(new GridPosition(2, 1));
            definition.Walls.Add(new GridPosition(2, 2));
            definition.Walls.Add(new GridPosition(4, 1));

            var result = StageValidator.Validate(definition);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException("Sample stage SG failed validation.");
            }

            return result.Stage;
        }

        // Visualization demo for switches, doors, and exit — not a Task 15 authored stage.
        public static IStage CreateDemo()
        {
            var definition = new StageDefinition
            {
                Width = 6,
                Height = 4,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(0, 2),
                ExitPosition = new GridPosition(5, 0)
            };

            definition.Walls.Add(new GridPosition(2, 1));
            definition.Walls.Add(new GridPosition(3, 1));

            definition.Switches.Add(new SwitchPlacement(new SwitchId("S1"), new GridPosition(1, 2)));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("S2"), new GridPosition(4, 2)));

            definition.Doors.Add(
                new DoorPlacement(
                    new DoorId("D1"),
                    new GridPosition(3, 2),
                    new List<SwitchId> { new SwitchId("S1") }));
            definition.Doors.Add(
                new DoorPlacement(
                    new DoorId("D2"),
                    new GridPosition(5, 1),
                    new List<SwitchId> { new SwitchId("S1"), new SwitchId("S2") }));

            var result = StageValidator.Validate(definition);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException("Demo stage failed validation.");
            }

            return result.Stage;
        }
    }
}
