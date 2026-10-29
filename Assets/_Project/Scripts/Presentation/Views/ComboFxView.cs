using System.Collections.Generic;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>Combo milestone celebration: a banner that pops in and a burst of confetti over the HUD.</summary>
    public sealed class ComboFxView
    {
        private const float BannerSeconds = 1.3f;
        private const float BannerFadeSeconds = 0.35f;
        private const int ConfettiPerTier = 30;
        private const int MaxConfetti = 160;
        private const float ConfettiLife = 1.8f;
        private const float ConfettiGravity = -1600f;
        private const float ConfettiSpeed = 1300f;
        private const float ConfettiSpin = 540f;
        private static readonly Vector2 ConfettiSize = new Vector2(22f, 12f);
        private static readonly Vector2 BannerPosition = new Vector2(0f, 420f);

        private static readonly Color[] ConfettiColors =
        {
            new Color32(0xFF, 0xD6, 0x1F, 0xFF), new Color32(0xF5, 0x3D, 0xC8, 0xFF), new Color32(0x4F, 0xE3, 0xFF, 0xFF),
            new Color32(0x6B, 0xE0, 0x5A, 0xFF), new Color32(0xFF, 0x8A, 0x1F, 0xFF), Color.white
        };

        private sealed class Piece
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Velocity;
            public float Spin;
            public float Life;
        }

        private readonly RectTransform _root;
        private readonly ArtLibrary _art;
        private readonly Text _banner;
        private readonly List<Piece> _active = new List<Piece>();
        private readonly Stack<Piece> _pool = new Stack<Piece>();
        private float _bannerLeft;

        public ComboFxView(RectTransform parent, ArtLibrary art)
        {
            _art = art;
            _root = UiFactory.Stretch("ComboFx", parent);
            _banner = UiFactory.AddLabel(UiFactory.Rect("Banner", _root, new Vector2(0.5f, 0.5f), BannerPosition, new Vector2(1000f, 160f)), art.Font, 120, TextAnchor.MiddleCenter, Palette.Combo);
            _banner.gameObject.SetActive(false);
        }

        /// <param name="tier">1..ComboTier.Count; higher tiers throw more confetti.</param>
        public void Celebrate(int tier)
        {
            _banner.text = Texts.ComboBanner(tier);
            _banner.color = Palette.Combo;
            _banner.gameObject.SetActive(true);
            _bannerLeft = BannerSeconds;
            ScaleAnimator.On(_banner).PopIn();
            Burst(ConfettiPerTier * tier);
        }

        public void Tick(float dt)
        {
            TickBanner(dt);
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var piece = _active[i];
                piece.Life -= dt;
                if (piece.Life <= 0f)
                {
                    piece.Rect.gameObject.SetActive(false);
                    _active.RemoveAt(i);
                    _pool.Push(piece);
                    continue;
                }
                piece.Velocity.y += ConfettiGravity * dt;
                piece.Velocity.x *= 1f - dt;
                piece.Rect.anchoredPosition += piece.Velocity * dt;
                piece.Rect.Rotate(0f, 0f, piece.Spin * dt);
            }
        }

        private void TickBanner(float dt)
        {
            if (_bannerLeft <= 0f) return;
            _bannerLeft -= dt;
            var color = _banner.color;
            color.a = Mathf.Clamp01(_bannerLeft / BannerFadeSeconds);
            _banner.color = color;
            if (_bannerLeft <= 0f) _banner.gameObject.SetActive(false);
        }

        private void Burst(int count)
        {
            for (int i = 0; i < count && _active.Count < MaxConfetti; i++)
            {
                var piece = _pool.Count > 0 ? _pool.Pop() : CreatePiece();
                piece.Rect.gameObject.SetActive(true);
                piece.Rect.anchoredPosition = BannerPosition;
                piece.Rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                piece.Image.color = ConfettiColors[Random.Range(0, ConfettiColors.Length)];
                float angle = Random.Range(20f, 160f) * Mathf.Deg2Rad;
                piece.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(0.4f, 1f) * ConfettiSpeed;
                piece.Spin = Random.Range(-ConfettiSpin, ConfettiSpin);
                piece.Life = ConfettiLife * Random.Range(0.7f, 1f);
                _active.Add(piece);
            }
        }

        private Piece CreatePiece()
        {
            var rect = UiFactory.Rect("Confetti", _root, new Vector2(0.5f, 0.5f), Vector2.zero, ConfettiSize);
            var image = UiFactory.AddBox(rect, _art.FlatBox, Color.white);
            image.pixelsPerUnitMultiplier = 6f;
            return new Piece { Rect = rect, Image = image };
        }
    }
}
