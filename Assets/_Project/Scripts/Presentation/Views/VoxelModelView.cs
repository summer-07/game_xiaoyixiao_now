using System.Collections.Generic;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Voxels;
using UnityEngine;

namespace SheepStrike.Presentation.Views
{
    /// <summary>
    /// Renders the animal. Only voxels touching empty space get a cube; inner cubes appear as the shell is peeled,
    /// which keeps the object count low enough for mini-game WebGL.
    /// </summary>
    public sealed class VoxelModelView : MonoBehaviour
    {
        public const float CellSize = 0.25f;
        private const float CubeScale = 0.92f;
        private const float PopGrowSeconds = 0.06f;
        private const float PopSeconds = 0.18f;
        private const float PopPeakScale = 1.4f;

        private sealed class PoppingCube
        {
            public GameObject Cube;
            public float Elapsed;
        }

        private readonly Dictionary<VoxelPos, GameObject> _cubes = new Dictionary<VoxelPos, GameObject>();
        private readonly Stack<GameObject> _pool = new Stack<GameObject>();
        private readonly List<PoppingCube> _popping = new List<PoppingCube>();
        private RenderKit _kit;
        private VoxelModel _model;
        private Vector3 _center;

        public float Radius { get; private set; }

        public void Init(RenderKit kit) => _kit = kit;

        public void Show(VoxelModel model)
        {
            Clear();
            _model = model;
            transform.localRotation = Quaternion.Euler(0f, 25f, 0f);
            ScaleAnimator.On(this).PopIn();
            ComputeBounds(model);

            foreach (var pair in model.All)
            {
                if (model.IsExposed(pair.Key)) Spawn(pair.Key, pair.Value);
            }
        }

        public Vector3 WorldPositionOf(VoxelPos pos) => transform.TransformPoint(LocalPositionOf(pos));

        /// <summary>Direction towards the camera in voxel coordinates, which share axes with this transform's local space.</summary>
        public ViewDirection ViewFrom(Camera camera)
        {
            var towardsCamera = transform.InverseTransformDirection(-camera.transform.forward);
            return new ViewDirection(towardsCamera.x, towardsCamera.y, towardsCamera.z);
        }

        /// <summary>Called when a wool ball lands. The domain already removed the voxel; this updates the visuals.</summary>
        public void ResolveHit(VoxelPos pos)
        {
            if (_cubes.TryGetValue(pos, out var cube))
            {
                _cubes.Remove(pos);
                StartPop(cube);
            }

            foreach (var direction in VoxelPos.FaceDirections)
            {
                var neighbour = pos + direction;
                if (!_cubes.ContainsKey(neighbour) && _model.TryGetColor(neighbour, out var color))
                {
                    Spawn(neighbour, color);
                }
            }
        }

        private void Update()
        {
            for (int i = _popping.Count - 1; i >= 0; i--)
            {
                var popping = _popping[i];
                popping.Elapsed += Time.deltaTime;
                if (popping.Elapsed >= PopSeconds)
                {
                    Recycle(popping.Cube);
                    _popping.RemoveAt(i);
                    continue;
                }
                popping.Cube.transform.localScale = Vector3.one * (CellSize * CubeScale * PopScaleAt(popping.Elapsed));
            }
        }

        /// <summary>The hit cube flashes white, swells, then shrinks away while the debris bursts out of it.</summary>
        private void StartPop(GameObject cube)
        {
            cube.GetComponent<MeshRenderer>().sharedMaterial = _kit.MaterialOf(GameColor.White);
            _popping.Add(new PoppingCube { Cube = cube });
        }

        private static float PopScaleAt(float elapsed)
        {
            if (elapsed < PopGrowSeconds) return Mathf.Lerp(1f, PopPeakScale, elapsed / PopGrowSeconds);
            return Mathf.Lerp(PopPeakScale, 0f, (elapsed - PopGrowSeconds) / (PopSeconds - PopGrowSeconds));
        }

        private void Recycle(GameObject cube)
        {
            cube.SetActive(false);
            _pool.Push(cube);
        }

        private void Spawn(VoxelPos pos, GameColor color)
        {
            var cube = _pool.Count > 0 ? _pool.Pop() : _kit.CreateRenderer("Voxel", _kit.CubeMesh, transform);
            cube.SetActive(true);
            cube.transform.localPosition = LocalPositionOf(pos);
            cube.transform.localScale = Vector3.one * (CellSize * CubeScale);
            cube.GetComponent<MeshRenderer>().sharedMaterial = _kit.MaterialOf(color);
            _cubes[pos] = cube;
        }

        private Vector3 LocalPositionOf(VoxelPos pos) =>
            (new Vector3(pos.X, pos.Y, pos.Z) - _center) * CellSize;

        private void ComputeBounds(VoxelModel model)
        {
            bool first = true;
            var min = Vector3.zero;
            var max = Vector3.zero;
            foreach (var pair in model.All)
            {
                var p = new Vector3(pair.Key.X, pair.Key.Y, pair.Key.Z);
                if (first)
                {
                    min = max = p;
                    first = false;
                    continue;
                }
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
            _center = (min + max) * 0.5f;
            Radius = ((max - min).magnitude * 0.5f + 1f) * CellSize;
        }

        private void Clear()
        {
            foreach (var cube in _cubes.Values) Recycle(cube);
            _cubes.Clear();
            foreach (var popping in _popping) Recycle(popping.Cube);
            _popping.Clear();
        }
    }
}
