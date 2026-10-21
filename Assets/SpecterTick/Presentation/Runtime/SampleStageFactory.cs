using System;
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
    }
}
