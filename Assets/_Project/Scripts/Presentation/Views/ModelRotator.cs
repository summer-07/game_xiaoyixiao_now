using UnityEngine;
using UnityEngine.EventSystems;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// Drag sideways anywhere outside the UI to spin the animal; it drifts slowly when idle.
    /// Vertical drags are ignored: the animal stays upright and never tilts up or down.
    /// </summary>
    public sealed class ModelRotator : MonoBehaviour
    {
        private const float DegreesPerPixel = 0.35f;
        private const float IdleSpinSpeed = 8f;

        private Transform _target;
        private Vector3 _lastPointer;
        private bool _dragging;
        private float _yaw;
        private float _yawVelocity;

        public void Bind(Transform target)
        {
            _target = target;
            _yaw = target.localEulerAngles.y;
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

            if (_dragging)
            {
                var delta = Input.mousePosition - _lastPointer;
                _lastPointer = Input.mousePosition;
                _yawVelocity = -delta.x * DegreesPerPixel / Mathf.Max(Time.deltaTime, 0.001f);
            }
            else
            {
                _yawVelocity = Mathf.Lerp(_yawVelocity, IdleSpinSpeed, Time.deltaTime * 2f);
            }

            _yaw += _yawVelocity * Time.deltaTime;
            _target.localRotation = Quaternion.Euler(0f, _yaw, 0f);
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
