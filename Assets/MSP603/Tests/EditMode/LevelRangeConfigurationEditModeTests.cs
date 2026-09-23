using NUnit.Framework;

namespace MSP603.TowerDefense.Tests
{
    public sealed class LevelRangeConfigurationEditModeTests
    {
        [Test]
        public void OnlyLevelTwoArcherReceivesTheApprovedBaseRangeBonus()
        {
            TowerStats archer = TowerCatalog.Get(TowerType.Archer);

            Assert.That(archer.Range + LevelDefinition.FromScene("Level01").GetTowerRangeBonus(TowerType.Archer), Is.EqualTo(4.2f).Within(0.0001f));
            Assert.That(archer.Range + LevelDefinition.FromScene("Level02").GetTowerRangeBonus(TowerType.Archer), Is.EqualTo(4.55f).Within(0.0001f));
            Assert.That(archer.Range + LevelDefinition.FromScene("Level03").GetTowerRangeBonus(TowerType.Archer), Is.EqualTo(4.2f).Within(0.0001f));

            foreach (TowerType type in new[] { TowerType.Mage, TowerType.Cannon, TowerType.Inferno })
            {
                Assert.That(LevelDefinition.FromScene("Level02").GetTowerRangeBonus(type), Is.Zero, type.ToString());
            }
        }

        [Test]
        public void StartingGoldPreservesLevelOneAndFundsTheIntendedOpeningDefences()
        {
            int archerCost = TowerCatalog.Get(TowerType.Archer).Cost;

            Assert.That(LevelDefinition.FromScene("Level01").StartingGold, Is.EqualTo(250));
            Assert.That(LevelDefinition.FromScene("Level02").StartingGold, Is.EqualTo(250));
            Assert.That(LevelDefinition.FromScene("Level03").StartingGold, Is.EqualTo(6 * archerCost));
        }

        [Test]
        public void EveryEnemySpeedIsReducedByTwentyPercentWithoutChangingRelativeDifferences()
        {
            foreach (float levelMultiplier in new[] { 1f, 1.08f, 1.15f })
            {
                float raider = EnemyUnit.CalculateMovementSpeed(EnemyType.GoblinRaider, levelMultiplier);
                float brute = EnemyUnit.CalculateMovementSpeed(EnemyType.GoblinBrute, levelMultiplier);
                float knight = EnemyUnit.CalculateMovementSpeed(EnemyType.RuinKnight, levelMultiplier);

                Assert.That(raider, Is.EqualTo(2.25f * levelMultiplier * .80f).Within(.0001f));
                Assert.That(brute, Is.EqualTo(1.15f * levelMultiplier * .80f).Within(.0001f));
                Assert.That(knight, Is.EqualTo(.72f * levelMultiplier * .80f).Within(.0001f));
                Assert.That(raider, Is.GreaterThan(brute));
                Assert.That(brute, Is.GreaterThan(knight));
            }
        }

        [Test]
        public void EveryTowerUsesTheSameAdditionalUpgradeDamageProgression()
        {
            foreach (TowerType type in new[] { TowerType.Archer, TowerType.Mage, TowerType.Cannon, TowerType.Inferno })
            {
                float baseDamage = TowerCatalog.Get(type).Damage;
                Assert.That(Tower.CalculateDamageForLevel(baseDamage, 1), Is.EqualTo(baseDamage).Within(.0001f), type.ToString());
                Assert.That(Tower.CalculateDamageForLevel(baseDamage, 2), Is.EqualTo(baseDamage * 1.35f * 1.10f).Within(.0001f), type.ToString());
                Assert.That(Tower.CalculateDamageForLevel(baseDamage, 3), Is.EqualTo(baseDamage * 1.70f * 1.20f).Within(.0001f), type.ToString());
            }
        }
    }
}
