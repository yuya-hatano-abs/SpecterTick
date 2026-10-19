using System;
using System.Linq;
using SpecterTick.Core.Stages;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class CoreAssemblyUnityReferenceTests
    {
        [Test]
        public void CoreAssembly_ReferencedNames_DoNotStartWithUnityEngineOrUnityEditor()
        {
            var references = typeof(IStage).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
            var hasUnity = references.Any(name =>
                name.StartsWith("UnityEngine", StringComparison.Ordinal)
                || name.StartsWith("UnityEditor", StringComparison.Ordinal));

            Assert.That(hasUnity, Is.False, string.Join(", ", references));
        }
    }
}
