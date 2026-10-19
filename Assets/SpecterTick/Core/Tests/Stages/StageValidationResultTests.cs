using System;
using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class StageValidationResultTests
    {
        [Test]
        public void Failure_WithEmptyErrors_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                StageValidationResult.Failure(new List<StageValidationError>()));

            Assert.That(ex.ParamName, Is.EqualTo("errors"));
        }

        [Test]
        public void Success_ExposesStage_AndEmptyErrors()
        {
            var stage = StageDefinitionTestBuilder.CreateMinimalValid();
            var result = StageValidator.Validate(stage);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Errors, Is.Empty);
            IStage resolved;
            Assert.That(result.TryGetStage(out resolved), Is.True);
            Assert.That(resolved, Is.Not.Null);
            Assert.That(result.Stage.Width, Is.EqualTo(4));
        }

        [Test]
        public void Failure_StageProperty_Throws()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.PlayerPosition = null;
            var result = StageValidator.Validate(definition);

            Assert.That(result.IsSuccess, Is.False);
            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = result.Stage;
            });
            IStage stage;
            Assert.That(result.TryGetStage(out stage), Is.False);
            Assert.That(stage, Is.Null);
        }
    }
}
