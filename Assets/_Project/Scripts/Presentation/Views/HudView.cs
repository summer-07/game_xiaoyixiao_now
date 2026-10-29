using System;
using SheepStrike.Domain.Session;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// Passive view: renders what the presenter tells it and forwards taps through the callbacks.
    /// Contains no game rules.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        private const float TopBarHeight = 230f;
        private const float PillsX = 250f;
        private const float GearSize = 100f;
        private const float ToastSeconds = 1.6f;
        private const float ToastFadeSeconds = 0.4f;
        private const int ComboVisibleFrom = 3;
        private const float ComboPunch = 0.22f;
        private const float ComboGrowthPerTier = 0.1f;
        private const float RainbowSpeed = 0.6f;

        private static readonly Color[] ComboTierColors =
        {
            Palette.Combo,
            new Color32(0xFF, 0x9A, 0x2E, 0xFF),
            new Color32(0xFF, 0x5C, 0xC8, 0xFF),
            new Color32(0x4F, 0xE3, 0xFF, 0xFF)
        };

        public Action<int, int> SheepTapped;
        public Action<BoosterType> BoosterTapped;
        public Action NextTapped;
        public Action RetryTapped;
        public Action ShareTapped;
        public Action RescueTapped;
        public Action GiveUpTapped;

        /// <summary>Raised for every HUD button press except the sheep, which the presenter voices itself.</summary>
        public Action ButtonPressed;
        public Action SettingsTapped;

        private ArtLibrary _art;
        private RectTransform _topBar;
        private Text _level;
        private Text _target;
        private Image _targetIcon;
        private Text _remaining;
        private Text _timer;
        private Text _combo;
        private ScaleAnimator _comboScale;
        private int _comboTier;
        private bool _comboEnabled = true;
        private Text _toast;
        private Image _toastPill;
        private float _toastLeft;
        private RectTransform _resultPanel;
        private Text _resultText;
        private GameObject _nextButton;
        private RectTransform _stuckPanel;

        public SlotRowView Slots { get; private set; }
        public SheepQueueView Queue { get; private set; }
        public BoosterBarView Boosters { get; private set; }
        public ComboFxView ComboFx { get; private set; }
        public DeployFlightView Flights { get; private set; }

        public void Build(ArtLibrary art, int maxSlots)
        {
            _art = art;
            var root = (RectTransform)transform;

            BuildTopBar(root);
            Boosters = new BoosterBarView(root, art, b => BoosterTapped?.Invoke(b));
            Queue = new SheepQueueView(root, BoosterBarView.Height + 40f, art, (c, r) => SheepTapped?.Invoke(c, r));
            Slots = new SlotRowView(root, BoosterBarView.Height + 40f + SheepQueueView.VisibleRows * 180f + 50f, art, maxSlots,
                () => BoosterTapped?.Invoke(BoosterType.ExtraPen));

            _combo = UiFactory.AddLabel(UiFactory.Rect("Combo", root, new Vector2(0.5f, 1f), new Vector2(190f, -TopBarHeight - 70f), new Vector2(560f, 110f)), art.Font, 84, TextAnchor.MiddleCenter, Palette.Combo);
            _comboScale = ScaleAnimator.On(_combo);
            Flights = new DeployFlightView(root, art);
            ComboFx = new ComboFxView(root, art);
            BuildToast(root);

            BuildResultPanel(root);
            BuildStuckPanel(root);
            HideOverlays();
            SetCombo(0, 0);
            HookButtonSounds(root);
        }

        /// <summary>Hides the level HUD (top bar, pen, queue, boosters) behind the home screen; toasts still show.</summary>
        public void SetGameplayVisible(bool visible)
        {
            _topBar.gameObject.SetActive(visible);
            Slots.Root.gameObject.SetActive(visible);
            Queue.Root.gameObject.SetActive(visible);
            Boosters.Root.gameObject.SetActive(visible);
            if (!visible) _combo.gameObject.SetActive(false);
        }

        public void SetLevel(int number, string targetId)
        {
            _level.text = Texts.Level(number);
            _target.text = Texts.Animal(targetId);
            _targetIcon.sprite = _art.Target(targetId);
            _targetIcon.enabled = _targetIcon.sprite != null;
            ScaleAnimator.On(_targetIcon).PopIn();
        }

        public void SetRemaining(int voxels) => _remaining.text = Texts.Remaining(voxels);

        public void SetTimer(float seconds) => _timer.text = Texts.Timer(seconds);

        /// <param name="tier">Cosmetic tier from <see cref="ComboTier"/>; grows and recolours the counter.</param>
        /// <summary>The player can hide the combo counter in settings.</summary>
        public void SetComboEnabled(bool enabled)
        {
            _comboEnabled = enabled;
            if (!enabled) _combo.gameObject.SetActive(false);
        }

        public void SetCombo(int combo, int tier)
        {
            bool visible = _comboEnabled && combo >= ComboVisibleFrom;
            _combo.gameObject.SetActive(visible);
            if (!visible) return;
            _combo.text = Texts.Combo(combo);
            _comboTier = tier;
            _combo.color = ComboTierColors[Mathf.Min(tier, ComboTierColors.Length - 1)];
            _comboScale.BaseScale = 1f + tier * ComboGrowthPerTier;
            _comboScale.Punch(ComboPunch);
        }

        public void Toast(string message)
        {
            _toast.text = message;
            _toastLeft = ToastSeconds;
            _toastPill.gameObject.SetActive(true);
            ScaleAnimator.On(_toastPill).PopIn();
        }

        public void ShowResult(bool won, string message)
        {
            HideOverlays();
            _resultText.text = message;
            _nextButton.SetActive(won);
            ShowOverlay(_resultPanel);
        }

        public void ShowStuck()
        {
            HideOverlays();
            ShowOverlay(_stuckPanel);
        }

        public void HideOverlays()
        {
            _resultPanel.gameObject.SetActive(false);
            _stuckPanel.gameObject.SetActive(false);
        }

        /// <summary>Screen-space y (pixels) of the lower edge of the top bar.</summary>
        public float TopBarBottomScreenY()
        {
            var corners = new Vector3[4];
            _topBar.GetWorldCorners(corners);
            return corners[0].y;
        }

        private void Update()
        {
            ComboFx.Tick(Time.deltaTime);
            Flights.Tick(Time.deltaTime);
            if (_comboTier >= ComboTierColors.Length && _combo.gameObject.activeSelf)
            {
                _combo.color = Color.HSVToRGB(Mathf.Repeat(Time.time * RainbowSpeed, 1f), 0.65f, 1f);
            }

            if (_toastLeft <= 0f) return;
            _toastLeft -= Time.deltaTime;
            float alpha = Mathf.Clamp01(_toastLeft / ToastFadeSeconds);
            _toast.color = new Color(1f, 1f, 1f, alpha);
            _toastPill.color = new Color(Palette.PanelDark.r, Palette.PanelDark.g, Palette.PanelDark.b, Palette.PanelDark.a * alpha);
            if (_toastLeft <= 0f) _toastPill.gameObject.SetActive(false);
        }

        private void BuildTopBar(RectTransform root)
        {
            _topBar = UiFactory.Rect("TopBar", root, new Vector2(0.5f, 1f), new Vector2(0f, -TopBarHeight * 0.5f), new Vector2(1080f, TopBarHeight));
            BuildSettingsButton();

            var levelPill = UiFactory.Rect("LevelPill", _topBar, new Vector2(0f, 0.5f), new Vector2(PillsX, 40f), new Vector2(250f, 90f));
            UiFactory.AddBox(levelPill, _art.FlatBox, Palette.PanelDark);
            _level = UiFactory.AddLabel(UiFactory.Stretch("Level", levelPill), _art.Font, 52, TextAnchor.MiddleCenter);

            var timerPill = UiFactory.Rect("TimerPill", _topBar, new Vector2(0f, 0.5f), new Vector2(PillsX, -48f), new Vector2(190f, 64f));
            UiFactory.AddBox(timerPill, _art.FlatBox, Palette.PillLight);
            _timer = UiFactory.AddLabel(UiFactory.Stretch("Timer", timerPill), _art.Font, 42, TextAnchor.MiddleCenter);

            var hint = UiFactory.AddLabel(UiFactory.Rect("RotateHint", _topBar, new Vector2(0f, 0.5f), new Vector2(PillsX, -110f), new Vector2(260f, 44f)), _art.Font, 32, TextAnchor.MiddleCenter);
            hint.text = Texts.RotateHint;

            var targetRect = UiFactory.Rect("TargetIcon", _topBar, new Vector2(0.5f, 0.5f), new Vector2(0f, 24f), new Vector2(140f, 140f));
            _targetIcon = UiFactory.AddImage(targetRect, null, Color.white);
            _target = UiFactory.AddLabel(UiFactory.Rect("Target", _topBar, new Vector2(0.5f, 0.5f), new Vector2(0f, -72f), new Vector2(400f, 54f)), _art.Font, 40, TextAnchor.MiddleCenter);

            BuildRemainingPill();
        }

        private void BuildSettingsButton()
        {
            var rect = UiFactory.Rect("Settings", _topBar, new Vector2(0f, 0.5f), new Vector2(70f, 40f), Vector2.one * GearSize);
            UiFactory.AddImage(rect, _art.Disc, Palette.PanelDark, true);
            var gear = UiFactory.Rect("Gear", rect, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * (GearSize * 0.78f));
            UiFactory.AddImage(gear, _art.Gear, Palette.MenuIcon);
            UiFactory.AddButton(rect, () => SettingsTapped?.Invoke());
        }

        private void BuildRemainingPill()
        {
            var pill = UiFactory.Rect("RemainingPill", _topBar, new Vector2(1f, 0.5f), new Vector2(-170f, 30f), new Vector2(260f, 120f));
            UiFactory.AddBox(pill, _art.BevelBox, Palette.PillLight);
            var caption = UiFactory.AddLabel(UiFactory.Rect("Caption", pill, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(260f, 40f)), _art.Font, 30, TextAnchor.MiddleCenter, Palette.PillLightText);
            RemoveOutlines(caption);
            caption.text = Texts.RemainingLabel;
            _remaining = UiFactory.AddLabel(UiFactory.Rect("Remaining", pill, new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(260f, 60f)), _art.Font, 52, TextAnchor.MiddleCenter, Palette.PillLightText);
            RemoveOutlines(_remaining);
        }

        private void BuildToast(RectTransform root)
        {
            var pill = UiFactory.Rect("Toast", root, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(820f, 100f));
            _toastPill = UiFactory.AddBox(pill, _art.FlatBox, Palette.PanelDark);
            _toast = UiFactory.AddLabel(UiFactory.Stretch("Text", pill), _art.Font, 44, TextAnchor.MiddleCenter);
            pill.gameObject.SetActive(false);
        }

        private void BuildResultPanel(RectTransform root)
        {
            _resultPanel = CreateOverlay(root, "Result");
            var box = CreateDialog(_resultPanel, new Vector2(860f, 900f));
            _resultText = UiFactory.AddLabel(UiFactory.Rect("Message", box, new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(800f, 320f)), _art.Font, 64, TextAnchor.MiddleCenter);
            _resultText.lineSpacing = 1.15f;

            _nextButton = UiFactory.TextButton(box, "Next", new Vector2(0f, -30f), new Vector2(580f, 140f), _art, Texts.Next, Palette.ButtonGreen, () => NextTapped?.Invoke()).gameObject;
            UiFactory.TextButton(box, "Retry", new Vector2(0f, -200f), new Vector2(580f, 140f), _art, Texts.Retry, Palette.ButtonOrange, () => RetryTapped?.Invoke());
            UiFactory.TextButton(box, "Share", new Vector2(0f, -355f), new Vector2(580f, 120f), _art, Texts.Share, Palette.ButtonBlue, () => ShareTapped?.Invoke());
        }

        private void BuildStuckPanel(RectTransform root)
        {
            _stuckPanel = CreateOverlay(root, "Stuck");
            var box = CreateDialog(_stuckPanel, new Vector2(860f, 760f));

            var title = UiFactory.AddLabel(UiFactory.Rect("Title", box, new Vector2(0.5f, 0.5f), new Vector2(0f, 255f), new Vector2(800f, 100f)), _art.Font, 72, TextAnchor.MiddleCenter, Palette.Combo);
            title.text = Texts.StuckTitle;
            var body = UiFactory.AddLabel(UiFactory.Rect("Body", box, new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(800f, 160f)), _art.Font, 46, TextAnchor.MiddleCenter);
            body.text = Texts.StuckBody;

            UiFactory.TextButton(box, "Rescue", new Vector2(0f, -100f), new Vector2(620f, 140f), _art, Texts.Rescue, Palette.ButtonGreen, () => RescueTapped?.Invoke());
            UiFactory.TextButton(box, "GiveUp", new Vector2(0f, -265f), new Vector2(400f, 116f), _art, Texts.GiveUp, Palette.ButtonGrey, () => GiveUpTapped?.Invoke());
        }

        private RectTransform CreateDialog(RectTransform overlay, Vector2 size)
        {
            var box = UiFactory.Rect("Box", overlay, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            UiFactory.AddBox(box, _art.BevelBox, Palette.Panel);
            var inner = UiFactory.Rect("Inner", box, new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), size - new Vector2(36f, 52f));
            UiFactory.AddBox(inner, _art.FlatBox, new Color(1f, 1f, 1f, 0.06f));
            return box;
        }

        private static void ShowOverlay(RectTransform overlay)
        {
            overlay.gameObject.SetActive(true);
            ScaleAnimator.On(overlay.Find("Box")).PopIn();
        }

        private void HookButtonSounds(RectTransform root)
        {
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                button.onClick.AddListener(() => ButtonPressed?.Invoke());
            }
        }

        private static void RemoveOutlines(Text text)
        {
            foreach (var outline in text.GetComponents<Outline>()) Destroy(outline);
        }

        private static RectTransform CreateOverlay(RectTransform root, string name)
        {
            var overlay = UiFactory.Stretch(name, root);
            UiFactory.AddImage(overlay, null, Palette.Dim, true);
            return overlay;
        }
    }
}
