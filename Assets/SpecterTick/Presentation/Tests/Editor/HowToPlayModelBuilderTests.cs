using System;
using System.Collections.Generic;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class HowToPlayModelBuilderTests
    {
        [Test]
        public void HB1_Build_Heading()
        {
            var model = HowToPlayModelBuilder.Build();
            Assert.That(model.Heading, Is.EqualTo("HOW TO PLAY"));
        }

        [Test]
        public void HB2_Build_StepsStartWithRecordConfirmStep()
        {
            var model = HowToPlayModelBuilder.Build();
            Assert.That(model.StepsText, Does.StartWith("1. RECORD"));
            Assert.That(model.StepsText, Does.Contain("2. CONFIRM"));
            Assert.That(model.StepsText, Does.Contain("3. STEP"));
        }

        [Test]
        public void HB3_Build_KeysMatchInputGuideAndContainMoveConfirmSpace()
        {
            var model = HowToPlayModelBuilder.Build();
            Assert.That(model.KeysText, Does.Contain(InputGuide.KeysLineFor(GameMode.Recording)));
            Assert.That(model.KeysText, Does.Contain(InputGuide.KeysLineFor(GameMode.Playback)));
            Assert.That(model.KeysText, Does.Contain(InputGuide.KeysLineFor(GameMode.Cleared)));
            Assert.That(model.KeysText, Does.Contain("ARROWS/WASD"));
            Assert.That(model.KeysText, Does.Contain("ENTER"));
            Assert.That(model.KeysText, Does.Contain("SPACE"));
        }

        [Test]
        public void HB4_Build_StagesAndHelpLines()
        {
            var model = HowToPlayModelBuilder.Build();
            Assert.That(model.KeysText, Does.Contain("STAGES: N = NEXT  P = PREVIOUS"));
            Assert.That(model.KeysText, Does.Contain("HELP: H"));
        }

        [Test]
        public void HB5_Build_AllStringsPrintableAscii()
        {
            var model = HowToPlayModelBuilder.Build();
            foreach (var text in HowToPlayModelBuilder.AllStrings(model))
            {
                Assert.That(HowToPlayModelBuilder.IsPrintableAscii(text), Is.True, text);
            }
        }

        [Test]
        public void HB6_Build_MaxLineLengthWithinLimit()
        {
            var model = HowToPlayModelBuilder.Build();
            foreach (var text in HowToPlayModelBuilder.AllStrings(model))
            {
                Assert.That(
                    HowToPlayModelBuilder.MaxLineLength(text),
                    Is.LessThanOrEqualTo(HowToPlayModelBuilder.MaxSingleLineLength),
                    text);
            }
        }

        [Test]
        public void HB7_Build_LegendOrderAndSpriteKeys()
        {
            var model = HowToPlayModelBuilder.Build();
            Assert.That(model.LegendEntries.Count, Is.EqualTo(6));
            Assert.That(model.LegendEntries[0].Title, Is.EqualTo("PLAYER"));
            Assert.That(model.LegendEntries[1].Title, Is.EqualTo("SPECTER"));
            Assert.That(model.LegendEntries[2].Title, Is.EqualTo("WALL"));
            Assert.That(model.LegendEntries[3].Title, Is.EqualTo("SWITCH"));
            Assert.That(model.LegendEntries[4].Title, Is.EqualTo("DOOR"));
            Assert.That(model.LegendEntries[5].Title, Is.EqualTo("EXIT"));
            Assert.That(model.LegendEntries[3].SpriteKeys, Is.EqualTo(new[] { "SwitchOff", "SwitchOn" }));
        }

        [Test]
        public void HB8_Build_LegendSpriteKeysExistInBoardSkinNames()
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < BoardSkin.SpriteNames.Count; i++)
            {
                names.Add(BoardSkin.SpriteNames[i]);
            }

            var model = HowToPlayModelBuilder.Build();
            for (var i = 0; i < model.LegendEntries.Count; i++)
            {
                var keys = model.LegendEntries[i].SpriteKeys;
                for (var k = 0; k < keys.Count; k++)
                {
                    Assert.That(names.Contains(keys[k]), Is.True, keys[k]);
                }
            }
        }

        [Test]
        public void HB9_Build_LegendCollectionsAreNotCastableArraysAndDefensiveCopy()
        {
            var passedLegend = new HowToPlayLegendEntry[]
            {
                new HowToPlayLegendEntry("PLAYER", "YOU MOVE DURING RECORDING.", new[] { "Player" })
            };
            var model = new HowToPlayModel(
                "HOW TO PLAY",
                "A",
                "B",
                "C",
                "D",
                "PRESS ANY KEY TO CONTINUE",
                passedLegend);

            Assert.That(model.LegendEntries is HowToPlayLegendEntry[], Is.False);
            Assert.That(model.LegendEntries[0].SpriteKeys is string[], Is.False);

            passedLegend[0] = new HowToPlayLegendEntry("X", "Y", new[] { "Wall" });
            Assert.That(model.LegendEntries[0].Title, Is.EqualTo("PLAYER"));

            var mutableKeys = new List<string> { "Player" };
            var entry = new HowToPlayLegendEntry("PLAYER", "YOU MOVE DURING RECORDING.", mutableKeys);
            mutableKeys[0] = "Wall";
            Assert.That(entry.SpriteKeys[0], Is.EqualTo("Player"));

            var rebuilt = HowToPlayModelBuilder.Build();
            Assert.That(rebuilt.LegendEntries[0].Title, Is.EqualTo("PLAYER"));
        }

        [Test]
        public void HB10_Build_Twice_EqualContent()
        {
            var left = HowToPlayModelBuilder.Build();
            var right = HowToPlayModelBuilder.Build();
            Assert.That(right.Heading, Is.EqualTo(left.Heading));
            Assert.That(right.PurposeText, Is.EqualTo(left.PurposeText));
            Assert.That(right.StepsText, Is.EqualTo(left.StepsText));
            Assert.That(right.RulesText, Is.EqualTo(left.RulesText));
            Assert.That(right.KeysText, Is.EqualTo(left.KeysText));
            Assert.That(right.ContinuePrompt, Is.EqualTo(left.ContinuePrompt));
            Assert.That(right.LegendEntries.Count, Is.EqualTo(left.LegendEntries.Count));
        }
    }
}
