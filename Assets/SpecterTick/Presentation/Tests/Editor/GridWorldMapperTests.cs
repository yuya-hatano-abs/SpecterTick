using System;
using NUnit.Framework;
using SpecterTick.Core;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class GridWorldMapperTests
    {
        [Test]
        public void G1_CellCenter_Goldens()
        {
            var mapper1 = new GridWorldMapper(1f);
            Assert.That(mapper1.CellCenterX(0), Is.EqualTo(0f).Within(1e-4));
            Assert.That(mapper1.CellCenterY(0), Is.EqualTo(0f).Within(1e-4));
            Assert.That(mapper1.CellCenterX(4), Is.EqualTo(4f).Within(1e-4));
            Assert.That(mapper1.CellCenterY(3), Is.EqualTo(3f).Within(1e-4));
            Assert.That(mapper1.CellCenterX(0), Is.EqualTo(0f).Within(1e-4));
            Assert.That(mapper1.CellCenterY(1), Is.EqualTo(1f).Within(1e-4));

            var mapper2 = new GridWorldMapper(2f);
            Assert.That(mapper2.CellCenterX(3), Is.EqualTo(6f).Within(1e-4));
            Assert.That(mapper2.CellCenterY(1), Is.EqualTo(2f).Within(1e-4));

            var mapperHalf = new GridWorldMapper(0.5f);
            Assert.That(mapperHalf.CellCenterX(3), Is.EqualTo(1.5f).Within(1e-4));
            Assert.That(mapperHalf.CellCenterY(2), Is.EqualTo(1.0f).Within(1e-4));
        }

        [Test]
        public void G2_BoardCenter_Goldens()
        {
            var mapper1 = new GridWorldMapper(1f);
            mapper1.GetBoardCenter(5, 4, out var x1, out var y1);
            Assert.That(x1, Is.EqualTo(2.0f).Within(1e-4));
            Assert.That(y1, Is.EqualTo(1.5f).Within(1e-4));

            var mapper2 = new GridWorldMapper(2f);
            mapper2.GetBoardCenter(5, 4, out var x2, out var y2);
            Assert.That(x2, Is.EqualTo(4.0f).Within(1e-4));
            Assert.That(y2, Is.EqualTo(3.0f).Within(1e-4));
        }

        [Test]
        public void G3_CameraOrthographicSize_Goldens()
        {
            var mapper = new GridWorldMapper(1f);
            Assert.That(mapper.ComputeOrthographicSize(5, 4, 2f), Is.EqualTo(3.0f).Within(1e-4));
            Assert.That(mapper.ComputeOrthographicSize(12, 2, 2f), Is.EqualTo(3.5f).Within(1e-4));

            var mapper2 = new GridWorldMapper(2f);
            Assert.That(mapper2.ComputeOrthographicSize(5, 4, 1f), Is.EqualTo(7.0f).Within(1e-4));

            Assert.That(mapper.ComputeOrthographicSize(5, 4, 1f), Is.EqualTo(3.5f).Within(1e-4));
        }

        [Test]
        public void G4_InvalidArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridWorldMapper(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridWorldMapper(-1f));

            var mapper = new GridWorldMapper(1f);
            Assert.Throws<ArgumentOutOfRangeException>(() => mapper.GetBoardCenter(0, 4, out _, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => mapper.GetBoardCenter(5, 0, out _, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => mapper.ComputeOrthographicSize(5, 4, 0f));
        }
    }
}
