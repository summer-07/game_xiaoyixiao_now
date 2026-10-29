using System;
using SheepStrike.Domain.Session;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.UseCases.Interactors
{
    public sealed class FinishLevel
    {
        private readonly PlayContext _context;
        private readonly IProgressStore _progress;
        private readonly IPlatformService _platform;

        public FinishLevel(PlayContext context, IProgressStore progress, IPlatformService platform)
        {
            _context = context;
            _progress = progress;
            _platform = platform;
        }

        /// <returns>The level number the player should play next.</returns>
        public int Execute()
        {
            var session = _context.Session;
            if (session == null) throw new InvalidOperationException("No level is being played.");

            bool won = session.State == SessionState.Won;
            int current = session.Level.Number;
            float secondsUsed = session.Level.TimeLimitSeconds - session.TimeLeft;
            _platform.ReportLevelResult(current, won, secondsUsed);

            if (!won) return current;

            int next = current + 1;
            if (next > _progress.HighestUnlockedLevel) _progress.SaveHighestUnlockedLevel(next);
            return next;
        }
    }
}
