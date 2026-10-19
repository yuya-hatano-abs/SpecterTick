using System;

namespace SpecterTick.Core.Validation
{
    public sealed class StageValidationError : IEquatable<StageValidationError>
    {
        public StageValidationError(
            StageValidationErrorCode code,
            StageValidationSubject subject,
            string message)
        {
            Code = code;
            Subject = subject;
            Message = message ?? string.Empty;
        }

        public StageValidationErrorCode Code { get; }
        public StageValidationSubject Subject { get; }
        public string Message { get; }

        public bool Equals(StageValidationError other)
        {
            if (other == null)
            {
                return false;
            }

            if (Code != other.Code)
            {
                return false;
            }

            return SubjectEquals(Subject, other.Subject);
        }

        public override bool Equals(object obj)
        {
            return obj is StageValidationError other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int)Code;
                hash = (hash * 397) ^ SubjectHashCode(Subject);
                return hash;
            }
        }

        private static bool SubjectEquals(StageValidationSubject a, StageValidationSubject b)
        {
            if (a.Kind != b.Kind || a.Index != b.Index || a.OtherKind != b.OtherKind
                || a.ReferenceIndex != b.ReferenceIndex)
            {
                return false;
            }

            if (a.Position.HasValue != b.Position.HasValue)
            {
                return false;
            }

            if (a.Position.HasValue && !a.Position.Value.Equals(b.Position.Value))
            {
                return false;
            }

            if (!a.SwitchId.Equals(b.SwitchId) || !a.DoorId.Equals(b.DoorId))
            {
                return false;
            }

            return true;
        }

        private static int SubjectHashCode(StageValidationSubject subject)
        {
            unchecked
            {
                var hash = (int)subject.Kind;
                hash = (hash * 397) ^ subject.Index;
                hash = (hash * 397) ^ (subject.Position?.GetHashCode() ?? 0);
                hash = (hash * 397) ^ subject.SwitchId.GetHashCode();
                hash = (hash * 397) ^ subject.DoorId.GetHashCode();
                hash = (hash * 397) ^ (int)subject.OtherKind;
                hash = (hash * 397) ^ subject.ReferenceIndex;
                return hash;
            }
        }
    }
}
