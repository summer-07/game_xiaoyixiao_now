using System.Collections.Generic;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Voxels
{
    /// <summary>
    /// The 3D animal the sheep are attacking. A voxel is exposed while at least one of its six faces touches
    /// empty space; once a view is set, it can only be hit while it is also visible from that view.
    /// </summary>
    public sealed class VoxelModel
    {
        private readonly Dictionary<VoxelPos, GameColor> _voxels;
        private readonly Dictionary<GameColor, HashSet<VoxelPos>> _byColor = new Dictionary<GameColor, HashSet<VoxelPos>>();
        private readonly HashSet<VoxelPos> _visible = new HashSet<VoxelPos>();
        private readonly VisibleSurface _surface = new VisibleSurface();
        private readonly List<VoxelPos> _candidates = new List<VoxelPos>();
        private ViewDirection? _view;
        private bool _visibilityStale;

        public VoxelModel(IReadOnlyDictionary<VoxelPos, GameColor> voxels)
        {
            _voxels = new Dictionary<VoxelPos, GameColor>(voxels.Count);
            foreach (var pair in voxels)
            {
                _voxels.Add(pair.Key, pair.Value);
                SetOf(pair.Value).Add(pair.Key);
            }
            InitialCount = _voxels.Count;
        }

        public int InitialCount { get; }

        public int Count => _voxels.Count;

        public bool IsCleared => _voxels.Count == 0;

        public IEnumerable<KeyValuePair<VoxelPos, GameColor>> All => _voxels;

        public bool Contains(VoxelPos pos) => _voxels.ContainsKey(pos);

        public bool TryGetColor(VoxelPos pos, out GameColor color) => _voxels.TryGetValue(pos, out color);

        public bool IsExposed(VoxelPos pos)
        {
            foreach (var direction in VoxelPos.FaceDirections)
            {
                if (!_voxels.ContainsKey(pos + direction))
                {
                    return true;
                }
            }
            return false;
        }

        public int CountOf(GameColor color) => _byColor.TryGetValue(color, out var set) ? set.Count : 0;

        public Dictionary<GameColor, int> CountByColor()
        {
            var result = new Dictionary<GameColor, int>();
            foreach (var pair in _byColor)
            {
                if (pair.Value.Count > 0)
                {
                    result[pair.Key] = pair.Value.Count;
                }
            }
            return result;
        }

        public bool HasExposed(GameColor color) => TryFindExposed(color, out _);

        public void SetView(ViewDirection view)
        {
            _view = view;
            _visibilityStale = true;
        }

        /// <summary>
        /// Recomputes what the viewer sees, if anything changed. Called once per tick rather than per shot,
        /// so a voxel uncovered mid-tick becomes a target on the next tick.
        /// </summary>
        public void RefreshVisibility()
        {
            if (!_view.HasValue || !_visibilityStale) return;
            _surface.Compute(_voxels.Keys, _view.Value, _visible);
            _visibilityStale = false;
        }

        /// <summary>
        /// Picks a random voxel of <paramref name="color"/> among those the viewer can see, so the animal
        /// crumbles all over instead of being peeled in a fixed order. Without a view, every exposed voxel counts.
        /// </summary>
        public bool TryFindVisible(GameColor color, IRandom random, out VoxelPos target)
        {
            target = default;
            if (!_byColor.TryGetValue(color, out var all)) return false;

            _candidates.Clear();
            foreach (var pos in all)
            {
                if (IsExposed(pos) && (!_view.HasValue || _visible.Contains(pos))) _candidates.Add(pos);
            }
            if (_candidates.Count == 0) return false;

            target = _candidates[random.Next(_candidates.Count)];
            return true;
        }

        /// <summary>
        /// Picks the exposed voxel of <paramref name="color"/> that the sheep should hit next.
        /// The model is peeled top-down, then front-to-back, so destruction reads clearly on screen.
        /// </summary>
        public bool TryFindExposed(GameColor color, out VoxelPos target)
        {
            target = default;
            if (!_byColor.TryGetValue(color, out var candidates))
            {
                return false;
            }

            bool found = false;
            foreach (var pos in candidates)
            {
                if (!IsExposed(pos))
                {
                    continue;
                }
                if (!found || IsHigherPriority(pos, target))
                {
                    target = pos;
                    found = true;
                }
            }
            return found;
        }

        public bool Remove(VoxelPos pos)
        {
            if (!_voxels.TryGetValue(pos, out var color))
            {
                return false;
            }
            _voxels.Remove(pos);
            _byColor[color].Remove(pos);
            _visible.Remove(pos);
            _visibilityStale = true;
            return true;
        }

        private static bool IsHigherPriority(VoxelPos candidate, VoxelPos current)
        {
            if (candidate.Y != current.Y) return candidate.Y > current.Y;
            if (candidate.Z != current.Z) return candidate.Z < current.Z;
            return candidate.X < current.X;
        }

        private HashSet<VoxelPos> SetOf(GameColor color)
        {
            if (!_byColor.TryGetValue(color, out var set))
            {
                set = new HashSet<VoxelPos>();
                _byColor.Add(color, set);
            }
            return set;
        }
    }
}
