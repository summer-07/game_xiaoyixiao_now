using SheepStrike.Domain.Session;
using SheepStrike.Presentation.Audio;
using SheepStrike.Presentation.Theme;
using SheepStrike.Presentation.Views;
using SheepStrike.UseCases.Interactors;
using SheepStrike.UseCases.Ports;
using UnityEngine;

namespace SheepStrike.Presentation.Presenters
{
    /// <summary>
    /// Bridges use cases and views: forwards player input to interactors, drains domain events every frame
    /// and turns them into animations and HUD updates.
    /// </summary>
    public sealed class GamePresenter
    {
        private const float ResultDelaySeconds = 0.8f;

        public sealed class ViewSet
        {
            public HudView Hud;
            public VoxelModelView Model;
            public ModelRotator Rotator;
            public WoolProjectiles Projectiles;
            public DebrisBurst Debris;
            public CameraRig Camera;
            public SoundBoard Sound;
        }

        private readonly PlayContext _context;
        private readonly StartLevel _startLevel;
        private readonly DeploySheep _deploySheep;
        private readonly UseBooster _useBooster;
        private readonly FinishLevel _finishLevel;
        private readonly IPlatformService _platform;
        private readonly ViewSet _views;

        private int _nextLevel;
        private float _resultDelay = -1f;
        private int _comboTier;

        public GamePresenter(
            PlayContext context,
            StartLevel startLevel,
            DeploySheep deploySheep,
            UseBooster useBooster,
            FinishLevel finishLevel,
            IPlatformService platform,
            ViewSet views)
        {
            _context = context;
            _startLevel = startLevel;
            _deploySheep = deploySheep;
            _useBooster = useBooster;
            _finishLevel = finishLevel;
            _platform = platform;
            _views = views;

            var hud = views.Hud;
            hud.ButtonPressed = views.Sound.Tap;
            hud.SheepTapped = OnSheepTapped;
            hud.BoosterTapped = OnBoosterTapped;
            hud.NextTapped = () => Begin(_nextLevel);
            hud.RetryTapped = () => Begin(Session.Level.Number);
            hud.ShareTapped = () => _platform.Share($"{Texts.GameTitle}：我在{Texts.Level(Session.Level.Number)}打败了{Texts.Animal(Session.Level.TargetId)}！");
            hud.RescueTapped = () => OnBoosterTapped(BoosterType.ExtraPen);
            hud.GiveUpTapped = () => Session.GiveUp();
        }

        private GameSession Session => _context.Session;

        public void BeginFromProgress()
        {
            var session = _startLevel.ContinueFromProgress();
            Present(session);
        }

        public void Begin(int levelNumber) => Present(_startLevel.Execute(levelNumber));

        public void Tick(float deltaSeconds)
        {
            if (Session == null) return;

            Session.SetView(_views.Model.ViewFrom(_views.Camera.Camera));
            Session.Tick(deltaSeconds);
            foreach (var gameEvent in Session.DrainEvents()) Handle(gameEvent);

            _views.Hud.SetTimer(Session.TimeLeft);
            _views.Hud.SetRemaining(Session.Model.Count);
            _views.Hud.Boosters.SetHighlighted(BoosterType.RamCharge, Session.IsRamChargeActive);

            if (_resultDelay >= 0f)
            {
                _resultDelay -= deltaSeconds;
                if (_resultDelay < 0f) ShowResult();
            }
        }

        private void Present(GameSession session)
        {
            _views.Projectiles.CompleteAll();
            _resultDelay = -1f;
            _comboTier = 0;

            var hud = _views.Hud;
            hud.HideOverlays();
            hud.Flights.CancelAll();
            hud.SetLevel(session.Level.Number, session.Level.TargetId);
            hud.SetCombo(0, 0);
            hud.Queue.Build(session.Queue.ColumnCount);
            hud.Slots.ClearAll();
            RefreshQueueAndPad();
            RefreshBoosters();

            _views.Model.Show(session.Model);
            _views.Rotator.Bind(_views.Model.transform);
            Canvas.ForceUpdateCanvases();
            _views.Camera.Frame(_views.Model.transform.position, _views.Model.Radius, hud.TopBarBottomScreenY(), hud.Slots.TopScreenY());
            if (session.IsAssisted) hud.Toast(Texts.Assisted);
        }

        private void OnSheepTapped(int column, int row)
        {
            if (_deploySheep.Execute(column, row)) return;

            if (Session.State != SessionState.Playing) return;
            _views.Sound.Denied();
            _views.Hud.Toast(Session.Pad.IsFull ? Texts.PadFull : Texts.FrontOnly);
        }

        private void OnBoosterTapped(BoosterType booster)
        {
            _useBooster.Execute(booster, outcome =>
            {
                if (outcome == BoosterOutcome.NotApplicable) _views.Hud.Toast(Texts.NotNow);
                else if (outcome == BoosterOutcome.AdNotCompleted) _views.Hud.Toast(Texts.AdNotFinished);
                RefreshBoosters();
            });
        }

        private void Handle(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case SheepDeployed deployed:
                    HopIntoPen(deployed);
                    RefreshQueueAndPad();
                    RefreshBoosters();
                    break;

                case WoolFired fired:
                    LaunchWool(fired);
                    break;

                case SheepRetired retired:
                    _views.Hud.Slots.ClearSlot(retired.Slot);
                    break;

                case ComboChanged combo:
                    OnComboChanged(combo.Combo);
                    break;

                case BoosterApplied applied:
                    _views.Sound.Booster();
                    _views.Hud.Toast(applied.Booster == BoosterType.ShepherdDog ? Texts.DogReady : Texts.Booster(applied.Booster) + "！");
                    if (applied.Booster == BoosterType.ExtraPen) _views.Hud.HideOverlays();
                    RefreshQueueAndPad();
                    RefreshBoosters();
                    break;

                case StateChanged changed:
                    OnStateChanged(changed.State);
                    break;
            }
        }

        /// <summary>The sheep leaves its queue cell and hops into the pen; it shows in the slot only once it lands.</summary>
        private void HopIntoPen(SheepDeployed deployed)
        {
            var sheep = Session.Pad.At(deployed.Slot);
            if (sheep == null) return;

            var hud = _views.Hud;
            var hop = new DeployFlightView.Hop
            {
                FromScreen = hud.Queue.ScreenPositionOf(deployed.Column, Mathf.Min(deployed.Row, SheepQueueView.VisibleRows - 1)),
                ToScreen = hud.Slots.ScreenPositionOf(deployed.Slot),
                FromSize = hud.Queue.CellSize,
                ToSize = hud.Slots.SheepSize,
                Seconds = GameSession.DeployTravelSeconds
            };
            _views.Sound.Hop();
            hud.Flights.Fly(hop, Palette.Of(sheep.Color), sheep.Ammo, () =>
            {
                hud.Slots.SetSheep(deployed.Slot, sheep.Color, sheep.Ammo);
                _views.Sound.Land();
            });
        }

        private void OnComboChanged(int combo)
        {
            int tier = ComboTier.Of(combo);
            if (tier > _comboTier)
            {
                _views.Hud.ComboFx.Celebrate(tier);
                _views.Sound.Fanfare(tier);
            }
            _comboTier = tier;
            _views.Hud.SetCombo(combo, tier);
        }

        private void LaunchWool(WoolFired fired)
        {
            var target = fired.Target;
            var origin = _views.Camera.ScreenToWorld(_views.Hud.Slots.ScreenPositionOf(fired.Slot), _views.Camera.Distance - _views.Model.Radius);
            _views.Hud.Slots.SetAmmo(fired.Slot, fired.AmmoLeft);
            _views.Sound.Fire();
            int tier = _comboTier;
            _views.Projectiles.Launch(origin, () => _views.Model.WorldPositionOf(target), fired.Color, () =>
            {
                _views.Debris.Emit(_views.Model.WorldPositionOf(target), fired.Color, tier);
                _views.Model.ResolveHit(target);
                _views.Sound.Hit(tier);
            });
        }

        private void OnStateChanged(SessionState state)
        {
            switch (state)
            {
                case SessionState.Stuck:
                    _views.Hud.ShowStuck();
                    break;
                case SessionState.Playing:
                    _views.Hud.HideOverlays();
                    break;
                case SessionState.Won:
                case SessionState.Lost:
                    _nextLevel = _finishLevel.Execute();
                    _resultDelay = ResultDelaySeconds;
                    break;
            }
        }

        private void ShowResult()
        {
            _views.Projectiles.CompleteAll();
            string animal = Texts.Animal(Session.Level.TargetId);
            bool won = Session.State == SessionState.Won;
            _views.Hud.ShowResult(won, won ? Texts.Won(animal, Session.BestCombo) : Texts.Lost(animal));
            if (won) _views.Sound.Win();
            else _views.Sound.Lose();
            if (won) _platform.VibrateShort();
        }

        private void RefreshQueueAndPad()
        {
            _views.Hud.Queue.Render(Session.Queue, Session.IsShepherdDogActive);
            _views.Hud.Slots.SetCapacity(Session.Pad.Capacity);
        }

        private void RefreshBoosters()
        {
            foreach (BoosterType booster in System.Enum.GetValues(typeof(BoosterType)))
            {
                _views.Hud.Boosters.SetCharges(booster, _context.Wallet.ChargesOf(booster));
                _views.Hud.Boosters.SetAvailable(booster, Session.CanApply(booster));
            }
            _views.Hud.Boosters.SetHighlighted(BoosterType.RamCharge, Session.IsRamChargeActive);
            _views.Hud.Boosters.SetHighlighted(BoosterType.ShepherdDog, Session.IsShepherdDogActive);
        }
    }
}
