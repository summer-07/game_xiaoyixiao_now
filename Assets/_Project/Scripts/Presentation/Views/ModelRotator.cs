using UnityEngine;
using UnityEngine.EventSystems;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// Drag sideways anywhere outside the UI to turn the animal: it turns exactly as far as the finger moves and
    /// stops the moment the finger stops or lifts, with no momentum. Vertical drags are ignored so it stays upright.
    /// After a few seconds untouched it starts drifting slowly so the far side comes round.
    /// </summary>
    public sealed class ModelRotator : MonoBehaviour
    {
        private const float DegreesPerPixel = 0.35f;
        private const float IdleDelaySeconds = 3f;
        private const float IdleSpinSpeed = 6f;
        private const float IdleEaseInPerSecond = 0.5f;

        private Transform _target;
        private Vector3 _lastPointer;
        private bool _dragging;
        private float _yaw;
        private float _idleSeconds;
        private float _idleBlend;

        public void Bind(Transform target)
        {
            _target = target;
            _yaw = target.localEulerAngles.y;
            _idleSeconds = 0f;
            _idleBlend = 0f;
        }

        private void Update()
        {
            if (_target == null) return;

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
            {
                _dragging = true;
                _lastPointer = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0)) _dragging = false;

            if (_dragging) FollowFinger();
            else Drift();

            _target.localRotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        private void FollowFinger()
        {
            var delta = Input.mousePosition - _lastPointer;
            _lastPointer = Input.mousePosition;
            _yaw -= delta.x * DegreesPerPixel;
            _idleSeconds = 0f;
            _idleBlend = 0f;
        }

        private void Drift()
        {
            _idleSeconds += Time.deltaTime;
            if (_idleSeconds < IdleDelaySeconds) return;
            _idleBlend = Mathf.Min(1f, _idleBlend + IdleEaseInPerSecond * Time.deltaTime);
            _yaw += IdleSpinSpeed * _idleBlend * Time.deltaTime;
        }

        private static bool IsPointerOverUi()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            if (Input.touchCount > 0) return eventSystem.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            return eventSystem.IsPointerOverGameObject();
        }
    }
}
