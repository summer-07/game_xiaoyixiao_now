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
        private const float TopBarHeight = 220f;
        private const float ToastSeconds = 1.6f;
        private const int ComboVisibleFrom = 3;

        public Action<int, int> SheepTapped;
        public Action<BoosterType> BoosterTapped;
        public Action NextTapped;
        public Action RetryTapped;
        public Action ShareTapped;
        public Action RescueTapped;
        public Action GiveUpTapped;

        private ArtLibrary _art;
        private RectTransform _topBar;
        private Text _level;
        private Text _target;
        private Image _targetIcon;
        private Text _remaining;
        private Text _timer;
        private Text _combo;
        private Text _toast;
        private float _toastLeft;
        private float _comboPunch;
        private RectTransform _resultPanel;
        private Text _resultText;
        private GameObject _nextButton;
        private RectTransform _stuckPanel;

        public SlotRowView Slots { get; private set; }
        public SheepQueueView Queue { get; private set; }
        public BoosterBarView Boosters { get; private set; }

        public void Build(ArtLibrary art, int maxSlots)
        {
            _art = art;
            var root = (RectTransform)transform;

            BuildTopBar(root);
            Boosters = new BoosterBarView(root, art, b => BoosterTapped?.Invoke(b));
            Queue = new SheepQueueView(root, BoosterBarView.Height + 10f, art, (c, r) => SheepTapped?.Invoke(c, r));
            Slots = new SlotRowView(root, BoosterBarView.Height + 10f + SheepQueueView.VisibleRows * 180f + 70f, art, maxSlots,
                () => BoosterTapped?.Invoke(BoosterType.ExtraPen));

            _combo = UiFactory.AddLabel(UiFactory.Rect("Combo", root, new Vector2(0.5f, 1f), new Vector2(180f, -TopBarHeight - 60f), new Vector2(500f, 100f)), art.Font, 76, TextAnchor.MiddleCenter, Palette.Combo);
            _toast = UiFactory.AddLabel(UiFactory.Rect("Toast", root, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1000f, 90f)), art.Font, 46, TextAnchor.MiddleCenter);

            var hint = UiFactory.AddLabel(UiFactory.Rect("RotateHint", root, new Vector2(0f, 1f), new Vector2(150f, -TopBarHeight - 40f), new Vector2(260f, 50f)), art.Font, 32, TextAnchor.MiddleCenter);
            hint.text = Texts.RotateHint;

            BuildResultPanel(root);
            BuildStuckPanel(root);
            HideOverlays();
            SetCombo(0);
        }

        public void SetLevel(int number, string targetId)
        {
            _level.text = Texts.Level(number);
            _target.text = Texts.Animal(targetId);
            _targetIcon.sprite = _art.Target(targetId);
            _targetIcon.enabled = _targetIcon.sprite != null;
        }

        public void SetRemaining(int voxels) => _remaining.text = Texts.Remaining(voxels);

        public void SetTimer(float seconds) => _timer.text = Texts.Timer(seconds);

        public void SetCombo(int combo)
        {
            bool visible = combo >= ComboVisibleFrom;
            _combo.gameObject.SetActive(visible);
            if (!visible) return;
            _combo.text = Texts.Combo(combo);
            _comboPunch = 1f;
        }

        public void Toast(string message)
        {
            _toast.text = message;
            _toastLeft = ToastSeconds;
        }

        public void ShowResult(bool won, string message)
        {
            HideOverlays();
            _resultText.text = message;
            _nextButton.SetActive(won);
            _resultPanel.gameObject.SetActive(true);
        }

        public void ShowStuck()
        {
            HideOverlays();
            _stuckPanel.gameObject.SetActive(true);
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
            if (_toastLeft > 0f)
            {
                _toastLeft -= Time.deltaTime;
                var color = _toast.color;
                color.a = Mathf.Clamp01(_toastLeft / 0.4f);
                _toast.color = color;
            }

            if (_comboPunch > 0f)
            {
                _comboPunch = Mathf.Max(0f, _comboPunch - Time.deltaTime * 6f);
                _combo.transform.localScale = Vector3.one * (1f + 0.25f * _comboPunch);
            }
        }

        private void BuildTopBar(RectTransform root)
        {
            _topBar = UiFactory.Rect("TopBar", root, new Vector2(0.5f, 1f), new Vector2(0f, -TopBarHeight * 0.5f), new Vector2(1080f, TopBarHeight));

            _level = UiFactory.AddLabel(UiFactory.Rect("Level", _topBar, new Vector2(0f, 0.5f), new Vector2(150f, 30f), new Vector2(260f, 80f)), _art.Font, 56, TextAnchor.MiddleCenter);
            _timer = UiFactory.AddLabel(UiFactory.Rect("Timer", _topBar, new Vector2(0f, 0.5f), new Vector2(150f, -45f), new Vector2(260f, 60f)), _art.Font, 44, TextAnchor.MiddleCenter);

            var targetRect = UiFactory.Rect("TargetIcon", _topBar, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(130f, 130f));
            _targetIcon = UiFactory.AddImage(targetRect, null, Color.white);
            _target = UiFactory.AddLabel(UiFactory.Rect("Target", _topBar, new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(400f, 50f)), _art.Font, 38, TextAnchor.MiddleCenter);

            _remaining = UiFactory.AddLabel(UiFactory.Rect("Remaining", _topBar, new Vector2(1f, 0.5f), new Vector2(-160f, 0f), new Vector2(300f, 80f)), _art.Font, 46, TextAnchor.MiddleCenter);
        }

        private void BuildResultPanel(RectTransform root)
        {
            _resultPanel = CreateOverlay(root, "Result");
            var box = UiFactory.Rect("Box", _resultPanel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 860f));
            UiFactory.AddImage(box, null, Palette.Panel);
            _resultText = UiFactory.AddLabel(UiFactory.Rect("Message", box, new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(800f, 320f)), _art.Font, 60, TextAnchor.MiddleCenter);

            _nextButton = UiFactory.TextButton(box, "Next", new Vector2(0f, -40f), new Vector2(560f, 130f), _art.Font, Texts.Next, new Color32(0x4C, 0xC0, 0x5A, 0xFF), () => NextTapped?.Invoke()).gameObject;
            UiFactory.TextButton(box, "Retry", new Vector2(0f, -200f), new Vector2(560f, 130f), _art.Font, Texts.Retry, new Color32(0xF0, 0xA0, 0x30, 0xFF), () => RetryTapped?.Invoke());
            UiFactory.TextButton(box, "Share", new Vector2(0f, -350f), new Vector2(560f, 110f), _art.Font, Texts.Share, new Color32(0x3B, 0x7B, 0xD9, 0xFF), () => ShareTapped?.Invoke());
        }

        private void BuildStuckPanel(RectTransform root)
        {
            _stuckPanel = CreateOverlay(root, "Stuck");
            var box = UiFactory.Rect("Box", _stuckPanel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 720f));
            UiFactory.AddImage(box, null, Palette.Panel);

            var title = UiFactory.AddLabel(UiFactory.Rect("Title", box, new Vector2(0.5f, 0.5f), new Vector2(0f, 240f), new Vector2(800f, 100f)), _art.Font, 66, TextAnchor.MiddleCenter, Palette.Combo);
            title.text = Texts.StuckTitle;
            var body = UiFactory.AddLabel(UiFactory.Rect("Body", box, new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(800f, 160f)), _art.Font, 44, TextAnchor.MiddleCenter);
            body.text = Texts.StuckBody;

            UiFactory.TextButton(box, "Rescue", new Vector2(0f, -110f), new Vector2(600f, 130f), _art.Font, Texts.Rescue, new Color32(0x4C, 0xC0, 0x5A, 0xFF), () => RescueTapped?.Invoke());
            UiFactory.TextButton(box, "GiveUp", new Vector2(0f, -260f), new Vector2(400f, 110f), _art.Font, Texts.GiveUp, new Color32(0x8A, 0x86, 0x9E, 0xFF), () => GiveUpTapped?.Invoke());
        }

        private static RectTransform CreateOverlay(RectTransform root, string name)
        {
            var overlay = UiFactory.Stretch(name, root);
            UiFactory.AddImage(overlay, null, new Color(0f, 0f, 0f, 0.55f), true);
            return overlay;
        }
    }
}
