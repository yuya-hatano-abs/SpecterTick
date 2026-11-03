using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpecterTick.Presentation
{
    public sealed class HowToPlayLegendEntry
    {
        public HowToPlayLegendEntry(string title, string description, IReadOnlyList<string> spriteKeys)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("Title is required.", nameof(title));
            }

            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (spriteKeys == null)
            {
                throw new ArgumentNullException(nameof(spriteKeys));
            }

            if (spriteKeys.Count == 0)
            {
                throw new ArgumentException("At least one sprite key is required.", nameof(spriteKeys));
            }

            Title = title;
            Description = description;
            SpriteKeys = CopyKeys(spriteKeys);
        }

        public string Title { get; }

        public string Description { get; }

        public ReadOnlyCollection<string> SpriteKeys { get; }

        private static ReadOnlyCollection<string> CopyKeys(IReadOnlyList<string> spriteKeys)
        {
            var copy = new string[spriteKeys.Count];
            for (var i = 0; i < spriteKeys.Count; i++)
            {
                copy[i] = spriteKeys[i];
            }

            return new ReadOnlyCollection<string>(copy);
        }
    }

    public sealed class HowToPlayModel
    {
        public HowToPlayModel(
            string heading,
            string purposeText,
            string stepsText,
            string rulesText,
            string keysText,
            string continuePrompt,
            IReadOnlyList<HowToPlayLegendEntry> legendEntries)
        {
            if (string.IsNullOrEmpty(heading))
            {
                throw new ArgumentException("Heading is required.", nameof(heading));
            }

            if (purposeText == null)
            {
                throw new ArgumentNullException(nameof(purposeText));
            }

            if (stepsText == null)
            {
                throw new ArgumentNullException(nameof(stepsText));
            }

            if (rulesText == null)
            {
                throw new ArgumentNullException(nameof(rulesText));
            }

            if (keysText == null)
            {
                throw new ArgumentNullException(nameof(keysText));
            }

            if (string.IsNullOrEmpty(continuePrompt))
            {
                throw new ArgumentException("Continue prompt is required.", nameof(continuePrompt));
            }

            if (legendEntries == null)
            {
                throw new ArgumentNullException(nameof(legendEntries));
            }

            if (legendEntries.Count == 0)
            {
                throw new ArgumentException("Legend entries are required.", nameof(legendEntries));
            }

            Heading = heading;
            PurposeText = purposeText;
            StepsText = stepsText;
            RulesText = rulesText;
            KeysText = keysText;
            ContinuePrompt = continuePrompt;
            LegendEntries = CopyLegend(legendEntries);
        }

        public string Heading { get; }

        public string PurposeText { get; }

        public string StepsText { get; }

        public string RulesText { get; }

        public string KeysText { get; }

        public string ContinuePrompt { get; }

        public ReadOnlyCollection<HowToPlayLegendEntry> LegendEntries { get; }

        private static ReadOnlyCollection<HowToPlayLegendEntry> CopyLegend(
            IReadOnlyList<HowToPlayLegendEntry> legendEntries)
        {
            var copy = new HowToPlayLegendEntry[legendEntries.Count];
            for (var i = 0; i < legendEntries.Count; i++)
            {
                copy[i] = legendEntries[i];
            }

            return new ReadOnlyCollection<HowToPlayLegendEntry>(copy);
        }
    }
}
