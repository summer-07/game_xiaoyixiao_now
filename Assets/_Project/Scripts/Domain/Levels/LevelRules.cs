using System.Collections.Generic;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Levels
{
    public static class LevelRules
    {
        private const int MinFailuresBeforeAssist = 3;
        private const int AssistThresholdSpread = 3;

        /// <summary>
        /// After this many losses on a level the next attempt is assisted, so nobody is stuck forever.
        /// Varies between 3 and 5 by level so it doesn't feel mechanical.
        /// </summary>
        public static int FailuresBeforeAssist(int levelNumber) =>
            MinFailuresBeforeAssist + (levelNumber % AssistThresholdSpread + AssistThresholdSpread) % AssistThresholdSpread;

        /// <summary>A level is winnable only if every colour has exactly as much wool as voxels.</summary>
        public static bool IsBalanced(LevelDefinition level)
        {
            var balance = new Dictionary<GameColor, int>();
            foreach (var color in level.Voxels.Values)
            {
                balance.TryGetValue(color, out int value);
                balance[color] = value + 1;
            }
            foreach (var column in level.SheepColumns)
            {
                foreach (var sheep in column)
                {
                    balance.TryGetValue(sheep.Color, out int value);
                    balance[sheep.Color] = value - sheep.Ammo;
                }
            }
            foreach (var value in balance.Values)
            {
                if (value != 0) return false;
            }
            return true;
        }
    }
}
