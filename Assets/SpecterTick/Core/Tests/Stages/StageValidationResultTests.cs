using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public void Failure_Errors_IListMutation_DoesNotChangeResult()
        {
            var source = new List<StageValidationError>
            {
                new StageValidationError(
                    StageValidationErrorCode.PlayerPositionNotSet,
                    new StageValidationSubject(StageElementKind.Player),
                    "msg")
            };
            var result = StageValidationResult.Failure(source);
            var originalCodes = result.Errors.Select(e => e.Code).ToList();

            var list = result.Errors as IList;
            if (list != null)
            {
                try
                {
                    list.Add(source[0]);
                }
                catch (NotSupportedException)
                {
                }

                try
                {
                    list.Clear();
                }
                catch (NotSupportedException)
                {
                }
            }

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.Select(e => e.Code).ToList(), Is.EqualTo(originalCodes));
        }

        [Test]
        public void Failure_IsolatedFromSourceListMutation()
        {
            var source = new List<StageValidationError>
            {
                new StageValidationError(
                    StageValidationErrorCode.ExitPositionNotSet,
                    new StageValidationSubject(StageElementKind.Exit),
                    "msg")
            };
            var result = StageValidationResult.Failure(source);
            source.Clear();
            source.Add(new StageValidationError(
                StageValidationErrorCode.WidthInvalid,
                new StageValidationSubject(StageElementKind.Stage),
                "other"));

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].Code, Is.EqualTo(StageValidationErrorCode.ExitPositionNotSet));
        }

        [Test]
        public void Failure_WithNullElement_ThrowsArgumentException()
        {
            var errors = new List<StageValidationError>
            {
                new StageValidationError(
                    StageValidationErrorCode.WidthInvalid,
                    new StageValidationSubject(StageElementKind.Stage),
                    "msg"),
                null
            };

            var ex = Assert.Throws<ArgumentException>(() => StageValidationResult.Failure(errors));
            Assert.That(ex.ParamName, Is.EqualTo("errors"));
        }
    }
}
