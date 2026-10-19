using System;
using System.Collections.Generic;
using SpecterTick.Core.Stages;

namespace SpecterTick.Core.Validation
{
    public sealed class StageValidationResult
    {
        private static readonly IReadOnlyList<StageValidationError> EmptyErrors =
            Array.AsReadOnly(Array.Empty<StageValidationError>());

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

        internal static StageValidationResult Success(IStage stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            return new StageValidationResult(stage, EmptyErrors);
        }

        internal static StageValidationResult Failure(IReadOnlyList<StageValidationError> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            if (errors.Count == 0)
            {
                throw new ArgumentException(
                    "At least one error is required for a failed validation result.",
                    nameof(errors));
            }

            var copy = new StageValidationError[errors.Count];
            for (var i = 0; i < errors.Count; i++)
            {
                if (errors[i] == null)
                {
                    throw new ArgumentException("Error list cannot contain null elements.", nameof(errors));
                }

                copy[i] = errors[i];
            }

            return new StageValidationResult(null, Array.AsReadOnly(copy));
        }
    }
}
