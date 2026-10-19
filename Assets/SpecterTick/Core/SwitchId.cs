using System;

namespace SpecterTick.Core
{
    public readonly struct SwitchId : IEquatable<SwitchId>
    {
        private readonly string _value;

        public SwitchId(string value)
        {
            _value = value;
        }

        public bool IsValid => !string.IsNullOrEmpty(_value) && !IsWhiteSpaceOnly(_value);

        public string RawValue => _value ?? string.Empty;

        public bool Equals(SwitchId other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is SwitchId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(_value ?? string.Empty);
        }

        public static bool operator ==(SwitchId left, SwitchId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SwitchId left, SwitchId right)
        {
            return !left.Equals(right);
        }

        private static bool IsWhiteSpaceOnly(string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
