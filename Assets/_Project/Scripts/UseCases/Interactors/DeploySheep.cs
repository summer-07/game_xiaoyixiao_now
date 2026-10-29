using SheepStrike.UseCases.Ports;

namespace SheepStrike.UseCases.Interactors
{
    public sealed class DeploySheep
    {
        private readonly PlayContext _context;
        private readonly IPlatformService _platform;

        public DeploySheep(PlayContext context, IPlatformService platform)
        {
            _context = context;
            _platform = platform;
        }

        public bool Execute(int column, int row)
        {
            if (!_context.HasSession) return false;
            bool deployed = _context.Session.TryDeploy(column, row);
            if (deployed) _platform.VibrateShort();
            return deployed;
        }
    }
}
