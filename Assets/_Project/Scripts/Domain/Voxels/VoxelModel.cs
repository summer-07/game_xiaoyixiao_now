using System.Collections.Generic;
using SheepStrike.Domain.Core;

namespace SheepStrike.Domain.Voxels
{
    /// <summary>
    /// The 3D animal the sheep are attacking. A voxel can be hit only while it is exposed,
    /// i.e. at least one of its six faces touches empty space.
    /// </summary>
    public sealed class VoxelModel
    {
        private readonly Dictionary<VoxelPos, GameColor> _voxels;
        private readonly Dictionary<GameColor, HashSet<VoxelPos>> _byColor = new Dictionary<GameColor, HashSet<VoxelPos>>();

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
