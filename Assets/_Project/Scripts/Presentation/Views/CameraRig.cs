using UnityEngine;

namespace SheepStrike.Presentation.Views
{
    /// <summary>Frames the animal inside the free screen band between the top bar and the launch slots.</summary>
    public sealed class CameraRig
    {
        // Above 1: the bounding sphere is conservative, so the animal itself still fits while filling the band like the reference.
        private const float Fill = 1.15f;

        private readonly Camera _camera;

        public CameraRig(Camera camera)
        {
            _camera = camera;
        }

        public Camera Camera => _camera;

        /// <param name="bandTopPx">Screen-space y (pixels, origin bottom) of the band's top edge.</param>
        /// <param name="bandBottomPx">Screen-space y of the band's bottom edge.</param>
        public void Frame(Vector3 subject, float subjectRadius, float bandTopPx, float bandBottomPx)
        {
            float tanHalf = Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float bandPx = Mathf.Max(1f, Mathf.Min(bandTopPx - bandBottomPx, Screen.width));
            float distance = subjectRadius * 2f * Screen.height / (Fill * bandPx * 2f * tanHalf);
            float unitsPerPx = 2f * distance * tanHalf / Screen.height;
            float bandCenterOffsetPx = (bandTopPx + bandBottomPx) * 0.5f - Screen.height * 0.5f;

            _camera.transform.rotation = Quaternion.identity;
            _camera.transform.position = subject + new Vector3(0f, -bandCenterOffsetPx * unitsPerPx, -distance);
            _camera.farClipPlane = Mathf.Max(100f, distance * 4f);
            Distance = distance;
        }

        public float Distance { get; private set; }

        /// <summary>World point under a screen position, at the given distance from the camera.</summary>
        public Vector3 ScreenToWorld(Vector2 screen, float depth) =>
            _camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
    }
}
