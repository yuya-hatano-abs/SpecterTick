using SpecterTick.Core.Stages;
using SpecterTick.Core.Tests.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class GameEngineStubTests
    {
        [Test]
        public void Stub_ReturnsIStage()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            IGameEngine engine = new StubGameEngine(StageValidator.Validate(definition).Stage);
            Assert.That(engine.Stage.Width, Is.EqualTo(4));
        }

        private sealed class StubGameEngine : IGameEngine
        {
            public StubGameEngine(IStage stage)
            {
                Stage = stage;
            }

            public IStage Stage { get; }
        }
    }
}
