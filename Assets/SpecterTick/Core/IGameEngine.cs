using SpecterTick.Core.Stages;

namespace SpecterTick.Core
{
    public interface IGameEngine
    {
        IStage Stage { get; }
    }
}
