using System;
using SheepStrike.Presentation.Audio;
using SheepStrike.Presentation.Theme;
using SheepStrike.Presentation.Views;
using SheepStrike.UseCases.Interactors;
using SheepStrike.UseCases.Ports;

namespace SheepStrike.Presentation.Presenters
{
    /// <summary>
    /// Everything around a level: the home screen with the next animal on display, the settings / pause dialog,
    /// the collection, the leaderboard and add-to-desktop. Starting or leaving a level goes through <see cref="GamePresenter"/>.
    /// </summary>
    public sealed class MenuPresenter
    {
        public sealed class ViewSet
        {
            public HomeView Home;
            public SettingsPanelView Settings;
            public CollectionPanelView Collection;
            public LeaderboardPanelView Leaderboard;
            public AgeTipsPanelView AgeTips;
            public HudView Hud;
            public SoundBoard Sound;
        }

        private readonly GamePresenter _game;
        private readonly ChangeSettings _settings;
        private readonly SkipLevel _skipLevel;
        private readonly BrowseCollection _collection;
        private readonly IPlatformService _platform;
        private readonly ViewSet _views;
        private int _collectionPage;

        public MenuPresenter(
            GamePresenter game,
            ChangeSettings settings,
            SkipLevel skipLevel,
            BrowseCollection collection,
            IPlatformService platform,
            ViewSet views)
        {
            _game = game;
            _settings = settings;
            _skipLevel = skipLevel;
            _collection = collection;
            _platform = platform;
            _views = views;
            WireHome();
            WireSettings();
            _views.Hud.SettingsTapped = () => OpenSettings(inGame: true);
            ApplyAllSettings();
        }

        public void ShowHome()
        {
            CloseDialogs();
            int level = _collection.CurrentLevel;
            _game.Preview(level);
            _views.Hud.SetGameplayVisible(false);
            _views.Home.Show(level);
        }

        private void WireHome()
        {
            var home = _views.Home;
            home.StartTapped = Voiced(StartFromHome);
            home.SettingsTapped = Voiced(() => OpenSettings(inGame: false));
            home.CollectionTapped = Voiced(OpenCollection);
            home.LeaderboardTapped = Voiced(OpenLeaderboard);
            home.AgeTipsTapped = Voiced(_views.AgeTips.Show);
            _views.Leaderboard.CloseTapped = Voiced(_views.Leaderboard.Hide);
            _views.Leaderboard.TabTapped = friends =>
            {
                _views.Sound.Tap();
                RenderLeaderboard(friends);
            };
            home.AddToDesktopTapped = Voiced(() => _platform.AddToDesktop(added =>
            {
                if (!added) _views.Hud.Toast(Texts.DesktopUnavailable);
            }));
        }

        private void WireSettings()
        {
            var settings = _views.Settings;
            settings.ToggleTapped = kind =>
            {
                _views.Sound.Tap();
                _settings.Toggle(kind);
                Apply(kind);
            };
            settings.ContinueTapped = Voiced(CloseSettings);
            settings.HomeTapped = Voiced(ShowHome);
            settings.RestartTapped = Voiced(() => PlayLevel(_game.CurrentLevel));
            settings.SkipTapped = Voiced(() => _skipLevel.Execute(_game.CurrentLevel, next =>
            {
                if (next.HasValue) PlayLevel(next.Value);
                else _views.Hud.Toast(Texts.AdNotFinished);
            }));
        }

        private void StartFromHome()
        {
            _views.Home.Hide();
            _views.Hud.SetGameplayVisible(true);
            _game.IsPaused = false;
        }

        private void PlayLevel(int level)
        {
            CloseDialogs();
            _views.Home.Hide();
            _views.Hud.SetGameplayVisible(true);
            _game.Begin(level);
        }

        private void OpenSettings(bool inGame)
        {
            if (inGame) _game.IsPaused = true;
            _views.Settings.Show(inGame);
        }

        /// <summary>Back to wherever the dialog was opened from; a level resumes only if the home screen isn't up.</summary>
        private void CloseSettings()
        {
            _views.Settings.Hide();
            if (!_views.Home.IsVisible) _game.IsPaused = false;
        }

        private void OpenCollection()
        {
            _collectionPage = 0;
            RenderCollection();
            _views.Collection.Show();
        }

        private void RenderCollection()
        {
            int pages = Math.Max(1, _collection.PageCount);
            _collectionPage = Math.Max(0, Math.Min(_collectionPage, pages - 1));
            _views.Collection.Render(_collection.UnlockedCount, _collectionPage, pages, _collection.Page(_collectionPage));
        }

        /// <summary>The collection view is built before this presenter, so the composition root routes its buttons here.</summary>
        public void OnCollectionPageRequested(int page)
        {
            _views.Sound.Tap();
            _collectionPage = page;
            RenderCollection();
        }

        public void OnCollectionReplay(int level)
        {
            _views.Sound.Tap();
            PlayLevel(level);
        }

        public void CloseAgeTips()
        {
            _views.Sound.Tap();
            _views.AgeTips.Hide();
        }

        public void OnCollectionClosed()
        {
            _views.Sound.Tap();
            _views.Collection.Hide();
        }

        /// <summary>The host's own ranking when there is one; otherwise our panel with the player's own record.</summary>
        private void OpenLeaderboard()
        {
            if (_platform.TryShowLeaderboard(_collection.CurrentLevel)) return;
            RenderLeaderboard(friends: true);
            _views.Leaderboard.Show();
        }

        /// <summary>
        /// Offline there is no ranking service, so both tabs list only the player; nothing is made up.
        /// The note tells them where the real ranking appears.
        /// </summary>
        private void RenderLeaderboard(bool friends)
        {
            var rows = new[] { new LeaderboardRow(1, Texts.MyBest, _collection.UnlockedCount, true) };
            _views.Leaderboard.Render(friends, rows, Texts.LeaderboardOffline);
        }

        private void CloseDialogs()
        {
            _views.Settings.Hide();
            _views.Collection.Hide();
            _views.Leaderboard.Hide();
            _views.AgeTips.Hide();
        }

        private void ApplyAllSettings()
        {
            foreach (SettingKind kind in Enum.GetValues(typeof(SettingKind))) Apply(kind);
        }

        private void Apply(SettingKind kind)
        {
            bool on = _settings.Current.IsOn(kind);
            _views.Settings.SetToggle(kind, on);
            switch (kind)
            {
                case SettingKind.Music:
                    _views.Sound.SetMusicEnabled(on);
                    break;
                case SettingKind.Sound:
                    _views.Sound.SetSoundEnabled(on);
                    break;
                case SettingKind.Vibration:
                    _game.VibrationEnabled = on;
                    break;
                case SettingKind.ComboDisplay:
                    _views.Hud.SetComboEnabled(on);
                    break;
            }
        }

        private Action Voiced(Action action) => () =>
        {
            _views.Sound.Tap();
            action();
        };
    }
}
