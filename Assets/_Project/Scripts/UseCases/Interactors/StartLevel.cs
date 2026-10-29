using System.Collections.Generic;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Session;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.UseCases.Interactors
{
    public sealed class StartLevel
    {
        private static readonly Dictionary<BoosterType, int> FreeChargesPerLevel = new Dictionary<BoosterType, int>
        {
            { BoosterType.RamCharge, 1 }
        };

        private readonly ILevelCatalog _catalog;
        private readonly IProgressStore _progress;
        private readonly PlayContext _context;

        public StartLevel(ILevelCatalog catalog, IProgressStore progress, PlayContext context)
        {
            _catalog = catalog;
            _progress = progress;
            _context = context;
        }

        public GameSession ContinueFromProgress() => Execute(_progress.HighestUnlockedLevel);

        public GameSession Execute(int levelNumber)
        {
            var level = _catalog.Load(levelNumber);
            _context.Session = new GameSession(level, new SeededRandom(level.Seed));
            _context.Wallet = new BoosterWallet(FreeChargesPerLevel);
            return _context.Session;
        }
    }
}
