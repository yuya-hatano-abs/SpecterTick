using System;
using System.Collections;
using System.Collections.Generic;

namespace SpecterTick.Core
{
    public sealed class RecordedCommandSequence : IEquatable<RecordedCommandSequence>, IEnumerable<RecordedCommand>
    {
        public static readonly RecordedCommandSequence Empty = new RecordedCommandSequence(Array.Empty<RecordedCommand>());

        private readonly RecordedCommand[] _commands;

        public RecordedCommandSequence(IEnumerable<RecordedCommand> commands)
        {
            if (commands == null)
            {
                throw new ArgumentNullException(nameof(commands));
            }

            if (commands is RecordedCommand[] array && commands.GetType() == typeof(RecordedCommand[]))
            {
                _commands = new RecordedCommand[array.Length];
                Array.Copy(array, _commands, array.Length);
                return;
            }

            var list = new List<RecordedCommand>();
            foreach (var command in commands)
            {
                list.Add(command);
            }

            _commands = list.ToArray();
        }

        private RecordedCommandSequence(RecordedCommand[] commands)
        {
            _commands = commands ?? Array.Empty<RecordedCommand>();
        }

        public int Count => _commands.Length;

        public RecordedCommand this[int index] => _commands[index];

        public RecordedCommandSequence Append(RecordedCommand command)
        {
            var next = new RecordedCommand[_commands.Length + 1];
            if (_commands.Length > 0)
            {
                Array.Copy(_commands, next, _commands.Length);
            }

            next[_commands.Length] = command;
            return new RecordedCommandSequence(next);
        }

        public RecordedCommand[] ToArray()
        {
            var copy = new RecordedCommand[_commands.Length];
            Array.Copy(_commands, copy, _commands.Length);
            return copy;
        }

        public IEnumerator<RecordedCommand> GetEnumerator()
        {
            for (var i = 0; i < _commands.Length; i++)
            {
                yield return _commands[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(RecordedCommandSequence other)
        {
            if (other == null)
            {
                return false;
            }

            if (_commands.Length != other._commands.Length)
            {
                return false;
            }

            for (var i = 0; i < _commands.Length; i++)
            {
                if (!_commands[i].Equals(other._commands[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is RecordedCommandSequence other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = _commands.Length;
                for (var i = 0; i < _commands.Length; i++)
                {
                    hash = (hash * 397) ^ _commands[i].GetHashCode();
                }

                return hash;
            }
        }
    }
}
