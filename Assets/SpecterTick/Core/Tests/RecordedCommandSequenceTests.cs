using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class RecordedCommandSequenceTests
    {
        [Test]
        public void Empty_HasCountZero()
        {
            Assert.That(RecordedCommandSequence.Empty.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_NullCommands_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new RecordedCommandSequence(null));
            Assert.That(ex.ParamName, Is.EqualTo("commands"));
        }

        [Test]
        public void Constructor_MutableList_DoesNotReflectLaterMutations()
        {
            var list = new List<RecordedCommand>
            {
                new RecordedCommand(MoveDirection.Up)
            };
            var sequence = new RecordedCommandSequence(list);
            list.Add(new RecordedCommand(MoveDirection.Down));
            list.Clear();
            Assert.That(sequence.Count, Is.EqualTo(1));
            Assert.That(sequence[0].Direction, Is.EqualTo(MoveDirection.Up));
        }

        [Test]
        public void Constructor_Array_DoesNotReflectLaterMutations()
        {
            var array = new[]
            {
                new RecordedCommand(MoveDirection.Up)
            };
            var sequence = new RecordedCommandSequence(array);
            array[0] = new RecordedCommand(MoveDirection.Down);
            Assert.That(sequence[0].Direction, Is.EqualTo(MoveDirection.Up));
        }

        [Test]
        public void ToArray_ReturnedArray_DoesNotMutateSequence()
        {
            var sequence = new RecordedCommandSequence(new[]
            {
                new RecordedCommand(MoveDirection.Up)
            });
            var copy = sequence.ToArray();
            copy[0] = new RecordedCommand(MoveDirection.Down);
            Assert.That(sequence[0].Direction, Is.EqualTo(MoveDirection.Up));
        }

        [Test]
        public void Append_DoesNotMutateOriginal_PreservesOrderInNewInstance()
        {
            var original = new RecordedCommandSequence(new[]
            {
                new RecordedCommand(MoveDirection.Up)
            });
            var appended = original.Append(new RecordedCommand(MoveDirection.Left));
            Assert.That(original.Count, Is.EqualTo(1));
            Assert.That(original[0].Direction, Is.EqualTo(MoveDirection.Up));
            Assert.That(appended.Count, Is.EqualTo(2));
            Assert.That(appended[0].Direction, Is.EqualTo(MoveDirection.Up));
            Assert.That(appended[1].Direction, Is.EqualTo(MoveDirection.Left));
        }

        [Test]
        public void Equals_DifferentLength_IsNotEqual()
        {
            var one = new RecordedCommandSequence(new[] { new RecordedCommand(MoveDirection.Up) });
            var two = new RecordedCommandSequence(new[]
            {
                new RecordedCommand(MoveDirection.Up),
                new RecordedCommand(MoveDirection.Up)
            });
            Assert.That(one.Equals(two), Is.False);
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var sequence = new RecordedCommandSequence(new[] { new RecordedCommand(MoveDirection.Up) });
            Assert.That(sequence.Equals(null), Is.False);
        }

        [Test]
        public void Equals_SameOrder_EqualAndSameHashCode()
        {
            var a = new RecordedCommandSequence(new[]
            {
                new RecordedCommand(MoveDirection.Up),
                new RecordedCommand(MoveDirection.Left)
            });
            var b = new RecordedCommandSequence(new[]
            {
                new RecordedCommand(MoveDirection.Up),
                new RecordedCommand(MoveDirection.Left)
            });
            Assert.That(a.Equals(b), Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentOrder_IsNotEqual()
        {
            var a = new RecordedCommandSequence(new[]
            {
                new RecordedCommand(MoveDirection.Up),
                new RecordedCommand(MoveDirection.Left)
            });
            var b = new RecordedCommandSequence(new[]
            {
                new RecordedCommand(MoveDirection.Left),
                new RecordedCommand(MoveDirection.Up)
            });
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void CannotCastToMutableCollectionInterfaces()
        {
            var sequence = new RecordedCommandSequence(new[]
            {
                new RecordedCommand(MoveDirection.Up)
            });
            Assert.That(sequence is IList, Is.False);
            Assert.That(sequence is ICollection<RecordedCommand>, Is.False);
        }
    }
}
