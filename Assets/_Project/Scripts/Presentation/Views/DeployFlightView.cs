using System;
using System.Collections.Generic;
using SheepStrike.Presentation.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// A deployed sheep hops from its queue cell to its pen slot along an arc, squashing on take-off and
    /// stretching mid-air. The flight lasts as long as the domain's walk-in delay, so it lands as it starts firing.
    /// </summary>
    public sealed class DeployFlightView
    {
        private const float HopHeight = 260f;
        private const float StretchAmount = 0.18f;
        private const float SpinDegrees = 12f;

        private sealed class Flight
        {
            public RectTransform Rect;
            public Image Sheep;
            public Text Ammo;
            public Vector2 From;
            public Vector2 To;
            public Vector2 FromSize;
            public Vector2 ToSize;
            public float Elapsed;
            public float Duration;
            public Action OnLanded;
        }

        private readonly RectTransform _root;
        private readonly ArtLibrary _art;
        private readonly List<Flight> _active = new List<Flight>();
        private readonly Stack<Flight> _pool = new Stack<Flight>();

        public DeployFlightView(RectTransform parent, ArtLibrary art)
        {
            _art = art;
            _root = UiFactory.Stretch("DeployFlights", parent);
        }

        public struct Hop
        {
            public Vector2 FromScreen;
            public Vector2 ToScreen;
            public Vector2 FromSize;
            public Vector2 ToSize;
            public float Seconds;
        }

        public void Fly(Hop hop, Color color, int ammo, Action onLanded)
        {
            var flight = _pool.Count > 0 ? _pool.Pop() : CreateFlight();
            flight.From = ToLocal(hop.FromScreen);
            flight.To = ToLocal(hop.ToScreen);
            flight.FromSize = hop.FromSize;
            flight.ToSize = hop.ToSize;
            flight.Duration = Mathf.Max(0.01f, hop.Seconds);
            flight.Elapsed = 0f;
            flight.OnLanded = onLanded;
            flight.Sheep.color = color;
            flight.Ammo.text = ammo.ToString();
            flight.Rect.gameObject.SetActive(true);
            flight.Rect.SetAsLastSibling();
            Place(flight, 0f);
            _active.Add(flight);
        }

        /// <summary>Drops every sheep still in the air, e.g. when a new level starts.</summary>
        public void CancelAll()
        {
            foreach (var flight in _active) Recycle(flight);
            _active.Clear();
        }

        public void Tick(float dt)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var flight = _active[i];
                flight.Elapsed += dt;
                float t = Mathf.Clamp01(flight.Elapsed / flight.Duration);
                Place(flight, t);
                if (t < 1f) continue;

                _active.RemoveAt(i);
                var onLanded = flight.OnLanded;
                Recycle(flight);
                onLanded?.Invoke();
            }
        }

        private static void Place(Flight flight, float t)
        {
            float eased = t * t * (3f - 2f * t);
            var position = Vector2.Lerp(flight.From, flight.To, eased);
            position.y += Mathf.Sin(t * Mathf.PI) * HopHeight;
            flight.Rect.anchoredPosition = position;
            flight.Rect.sizeDelta = Vector2.Lerp(flight.FromSize, flight.ToSize, eased);

            float stretch = Mathf.Sin(t * Mathf.PI) * StretchAmount;
            flight.Rect.localScale = new Vector3(1f - stretch * 0.5f, 1f + stretch, 1f);
            flight.Rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 2f) * SpinDegrees);
        }

        private Vector2 ToLocal(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, null, out var local);
            return local;
        }

        private Flight CreateFlight()
        {
            var rect = UiFactory.Rect("FlyingSheep", _root, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * 100f);
            var sheep = UiFactory.AddImage(rect, _art.SheepIcon, Color.white);
            var ammo = UiFactory.AddLabel(UiFactory.Stretch("Ammo", rect), _art.Font, 56, TextAnchor.MiddleCenter);
            return new Flight { Rect = rect, Sheep = sheep, Ammo = ammo };
        }

        private void Recycle(Flight flight)
        {
            flight.Rect.gameObject.SetActive(false);
            _pool.Push(flight);
        }
    }
}
