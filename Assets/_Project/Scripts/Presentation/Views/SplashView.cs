using System;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// Launch splash: the key art fills the screen (cropped, never letterboxed), the title bounces in over the
    /// empty sky, a loading bar fills at the bottom, then everything fades away to reveal the home screen.
    /// </summary>
    public sealed class SplashView : MonoBehaviour
    {
        private const float HoldSeconds = 1.6f;
        private const float TitleDelay = 0.25f;
        private const float BarWidth = 620f;
        private const float BarHeight = 34f;
        private const float BarInset = 6f;
        private const float BarBottom = 300f;
        private const float NoticeBottom = 40f;
        private static readonly Color NoticeShade = new Color(0f, 0f, 0f, 0.45f);

        private Image _fill;
        private RectTransform _title;
        private UiTransition _fade;
        private Action _onFinished;
        private float _elapsed = -1f;
        private bool _titleShown;

        public void Build(ArtLibrary art)
        {
            var root = (RectTransform)transform;
            BuildArt(root, art);

            var safe = UiFactory.Stretch("SafeArea", root);
            safe.gameObject.AddComponent<SafeAreaFitter>();
            _title = BuildTitle(safe, art);
            _fill = BuildLoadingBar(safe, art);
            BuildNotice(root, safe, art);
            _fade = UiTransition.On(root, UiTransition.Motion.Fade);
        }

        /// <param name="onFinished">Called as the splash starts fading, so the home screen can animate in underneath.</param>
        public void Play(Action onFinished)
        {
            _onFinished = onFinished;
            _elapsed = 0f;
            _title.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_elapsed < 0f) return;
            _elapsed += Time.unscaledDeltaTime;
            _fill.fillAmount = Mathf.SmoothStep(0f, 1f, _elapsed / HoldSeconds);

            if (!_titleShown && _elapsed >= TitleDelay)
            {
                _titleShown = true;
                _title.gameObject.SetActive(true);
                ScaleAnimator.On(_title).PopIn();
            }
            if (_elapsed < HoldSeconds) return;

            _elapsed = -1f;
            _fade.Hide();
            _onFinished?.Invoke();
        }

        private static void BuildArt(RectTransform root, ArtLibrary art)
        {
            UiFactory.AddImage(root, null, Palette.Background, true);
            if (art.Splash == null) return;
            var image = UiFactory.AddImage(UiFactory.Stretch("Art", root), art.Splash, Color.white);
            image.preserveAspect = false;
            var fitter = image.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = art.Splash.rect.width / art.Splash.rect.height;
        }

        private static RectTransform BuildTitle(RectTransform parent, ArtLibrary art)
        {
            var rect = UiFactory.Rect("Title", parent, new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(1000f, 220f));
            var title = UiFactory.AddLabel(rect, art.Font, 190, TextAnchor.MiddleCenter, Color.white);
            title.text = Texts.GameTitle;
            foreach (var outline in title.GetComponents<Outline>()) outline.effectColor = new Color32(0x4A, 0x2A, 0x8C, 0xFF);
            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
            shadow.effectDistance = new Vector2(0f, -14f);
            return rect;
        }

        /// <summary>
        /// 《健康游戏忠告》 and the registration numbers, required on the start screen. A soft shade behind
        /// them keeps the small text readable over the meadow.
        /// </summary>
        private static void BuildNotice(RectTransform root, RectTransform safe, ArtLibrary art)
        {
            var shade = UiFactory.Rect("NoticeShade", root, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(1080f, 0f));
            shade.anchorMin = new Vector2(0f, 0f);
            shade.anchorMax = new Vector2(1f, 0f);
            shade.pivot = new Vector2(0.5f, 0f);
            shade.sizeDelta = new Vector2(0f, BarBottom + NoticeBottom);
            UiFactory.AddImage(shade, null, NoticeShade);

            var lines = new System.Collections.Generic.List<string>(ComplianceInfo.Lines());
            string registration = string.Join("    ", lines);
            var notice = UiFactory.AddLabel(UiFactory.Rect("Notice", safe, new Vector2(0.5f, 0f), new Vector2(0f, NoticeBottom + 100f), new Vector2(1040f, 200f)), art.Font, 32, TextAnchor.LowerCenter);
            notice.text = registration.Length > 0 ? Texts.HealthAdvice + "\n" + registration : Texts.HealthAdvice;
            notice.lineSpacing = 1.2f;
        }

        private static Image BuildLoadingBar(RectTransform parent, ArtLibrary art)
        {
            var track = UiFactory.Rect("LoadingBar", parent, new Vector2(0.5f, 0f), new Vector2(0f, BarBottom), new Vector2(BarWidth, BarHeight));
            UiFactory.AddBox(track, art.FlatBox, Palette.PanelDark);
            var fillRect = UiFactory.Stretch("Fill", track);
            fillRect.offsetMin = Vector2.one * BarInset;
            fillRect.offsetMax = -Vector2.one * BarInset;
            var fill = UiFactory.AddImage(fillRect, art.FlatBox, Palette.ButtonGreen);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0f;
            return fill;
        }
    }
}
