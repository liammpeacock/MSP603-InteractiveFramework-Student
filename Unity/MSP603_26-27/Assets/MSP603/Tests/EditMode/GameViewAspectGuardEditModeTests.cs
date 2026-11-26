using MSP603.TowerDefense.Editor;
using NUnit.Framework;

namespace MSP603.TowerDefense.Tests
{
    public sealed class GameViewAspectGuardEditModeTests
    {
        [TestCase(1920f, 1080f)]
        [TestCase(1600f, 900f)]
        [TestCase(1280f, 720f)]
        public void CommonSixteenByNineSizesAreAccepted(float width, float height)
        {
            Assert.That(GameViewAspectGuard.IsApproximatelySixteenByNine(width, height), Is.True);
        }

        [TestCase(1024f, 768f)]
        [TestCase(2560f, 1080f)]
        public void ClearlyIncompatibleAspectsAreRejected(float width, float height)
        {
            Assert.That(GameViewAspectGuard.IsApproximatelySixteenByNine(width, height), Is.False);
        }

        [Test]
        public void SmallSizingVariationWithinToleranceIsAccepted()
        {
            Assert.That(GameViewAspectGuard.IsApproximatelySixteenByNine(176f, 100f), Is.True);
            Assert.That(GameViewAspectGuard.IsApproximatelySixteenByNine(174f, 100f), Is.False);
        }

        [Test]
        public void InvalidDimensionsAreRejectedWithoutConsumingWarningState()
        {
            bool warningShown = false;

            Assert.That(GameViewAspectGuard.ShouldWarnForSession(0f, 0f, ref warningShown), Is.False);
            Assert.That(warningShown, Is.False);
        }

        [Test]
        public void CompatibleAspectDoesNotWarnOrConsumeWarningState()
        {
            bool warningShown = false;

            Assert.That(GameViewAspectGuard.ShouldWarnForSession(1920f, 1080f, ref warningShown), Is.False);
            Assert.That(warningShown, Is.False);
        }

        [Test]
        public void IncompatibleAspectWarnsOnlyOncePerSession()
        {
            bool warningShown = false;

            Assert.That(GameViewAspectGuard.ShouldWarnForSession(1024f, 768f, ref warningShown), Is.True);
            Assert.That(warningShown, Is.True);
            Assert.That(GameViewAspectGuard.ShouldWarnForSession(1024f, 768f, ref warningShown), Is.False);
        }
    }
}
