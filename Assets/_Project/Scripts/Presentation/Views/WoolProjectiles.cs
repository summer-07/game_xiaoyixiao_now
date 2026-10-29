using System;
using System.Collections.Generic;
using SheepStrike.Domain.Core;
using UnityEngine;

namespace SheepStrike.Presentation.Views
{
    /// <summary>Pooled wool balls flying on a short arc from a launch slot to a voxel.</summary>
    public sealed class WoolProjectiles : MonoBehaviour
    {
        private const float FlightSeconds = 0.28f;
        private const float ArcHeight = 0.6f;
        private const float BallSize = 0.18f;

        private sealed class Flight
        {
            public GameObject Ball;
            public Vector3 From;
            public Func<Vector3> To;
            public float Elapsed;
            public Action OnArrive;
        }

        private readonly List<Flight> _active = new List<Flight>();
        private readonly Stack<GameObject> _pool = new Stack<GameObject>();
        private RenderKit _kit;

        public int InFlight => _active.Count;

        public void Init(RenderKit kit) => _kit = kit;

        /// <param name="to">Evaluated every frame because the target spins with the model.</param>
        public void Launch(Vector3 from, Func<Vector3> to, GameColor color, Action onArrive)
        {
            var ball = _pool.Count > 0 ? _pool.Pop() : _kit.CreateRenderer("Wool", _kit.SphereMesh, transform);
            ball.SetActive(true);
            ball.transform.position = from;
            ball.transform.localScale = Vector3.one * BallSize;
            ball.GetComponent<MeshRenderer>().sharedMaterial = _kit.MaterialOf(color);
            _active.Add(new Flight { Ball = ball, From = from, To = to, OnArrive = onArrive });
        }

        public void CompleteAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--) Land(i);
        }

        private void Update()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var flight = _active[i];
                flight.Elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(flight.Elapsed / FlightSeconds);
                var position = Vector3.Lerp(flight.From, flight.To(), t);
                position.y += Mathf.Sin(t * Mathf.PI) * ArcHeight;
                flight.Ball.transform.position = position;
                if (t >= 1f) Land(i);
            }
        }

        private void Land(int index)
        {
            var flight = _active[index];
            _active.RemoveAt(index);
            flight.Ball.SetActive(false);
            _pool.Push(flight.Ball);
            flight.OnArrive?.Invoke();
        }
    }
}
