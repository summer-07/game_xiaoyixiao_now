using UnityEngine;
using UnityEngine.EventSystems;

namespace SheepStrike.Presentation.Views
{
    /// <summary>Squashes a button while it is held and lets it spring back on release.</summary>
    public sealed class PressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const float PressedScale = 0.88f;
        private const float ReleasePunch = 0.12f;

        private ScaleAnimator _animator;

        private void Awake() => _animator = ScaleAnimator.On(this);

        public void OnPointerDown(PointerEventData eventData) => _animator.Hold(PressedScale);

        public void OnPointerUp(PointerEventData eventData) => _animator.Release(ReleasePunch);
    }
}
