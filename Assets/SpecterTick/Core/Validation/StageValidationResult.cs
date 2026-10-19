using System;
using System.Collections.Generic;
using SpecterTick.Core.Stages;

namespace SpecterTick.Core.Validation
{
    public sealed class StageValidationResult
    {
        private readonly IStage _stage;
        private readonly IReadOnlyList<StageValidationError> _errors;

        private StageValidationResult(IStage stage, IReadOnlyList<StageValidationError> errors)
        {
            _stage = stage;
            _errors = errors;
        }

        public bool IsSuccess => _stage != null;

        public IReadOnlyList<StageValidationError> Errors => _errors;

        public bool TryGetStage(out IStage stage)
        {
            stage = _stage;
            return IsSuccess;
        }

        public IStage Stage
        {
            get
            {
                if (!IsSuccess)
                {
                    throw new InvalidOperationException("Stage validation failed.");
                }

                return _stage;
            }
        }

        public static StageValidationResult Success(IStage stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            return new StageValidationResult(stage, Array.Empty<StageValidationError>());
        }

        public static StageValidationResult Failure(IReadOnlyList<StageValidationError> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            if (errors.Count == 0)
            {
                throw new ArgumentException("At least one error is required for a failed validation result.", nameof(errors));
            }

            return new StageValidationResult(null, errors);
        }
    }
}
