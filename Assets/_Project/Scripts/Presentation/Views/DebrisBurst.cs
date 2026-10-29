using System.Collections.Generic;
using SheepStrike.Domain.Core;
using UnityEngine;

namespace SheepStrike.Presentation.Views
{
    /// <summary>Cube shards that burst out when a voxel is destroyed. Simulated by hand, no physics engine needed.</summary>
    public sealed class DebrisBurst : MonoBehaviour
    {
        private const int ShardsPerHit = 10;
        private const float LifeSeconds = 0.8f;
        private const float Gravity = 12f;
        private const float BurstSpeed = 4.5f;
        private const float UpwardKick = 2.5f;
        private const float MinShardSize = 0.07f;
        private const float MaxShardSize = 0.15f;
        private const float SpinDegreesPerSecond = 540f;
        private const float ShrinkPerSecond = 1.2f;
        private const int MaxShards = 400;

        private sealed class Shard
        {
            public GameObject Go;
            public Vector3 Velocity;
            public Vector3 SpinAxis;
            public float Life;
        }

        private readonly List<Shard> _active = new List<Shard>();
        private readonly Stack<Shard> _pool = new Stack<Shard>();
        private RenderKit _kit;

        public void Init(RenderKit kit) => _kit = kit;

        public void Emit(Vector3 position, GameColor color)
        {
            for (int i = 0; i < ShardsPerHit && _active.Count < MaxShards; i++)
            {
                var shard = _pool.Count > 0
                    ? _pool.Pop()
                    : new Shard { Go = _kit.CreateRenderer("Shard", _kit.CubeMesh, transform) };
                shard.Go.SetActive(true);
                shard.Go.transform.position = position;
                shard.Go.transform.localScale = Vector3.one * Random.Range(MinShardSize, MaxShardSize);
                shard.Go.transform.rotation = Random.rotation;
                shard.Go.GetComponent<MeshRenderer>().sharedMaterial = _kit.MaterialOf(color);
                shard.Velocity = Random.onUnitSphere * Random.Range(0.5f, 1f) * BurstSpeed + Vector3.up * UpwardKick;
                shard.SpinAxis = Random.onUnitSphere;
                shard.Life = LifeSeconds;
                _active.Add(shard);
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var shard = _active[i];
                shard.Life -= dt;
                if (shard.Life <= 0f)
                {
                    shard.Go.SetActive(false);
                    _active.RemoveAt(i);
                    _pool.Push(shard);
                    continue;
                }
                shard.Velocity.y -= Gravity * dt;
                var t = shard.Go.transform;
                t.position += shard.Velocity * dt;
                t.Rotate(shard.SpinAxis, SpinDegreesPerSecond * dt, Space.World);
                t.localScale *= 1f - dt * ShrinkPerSecond;
            }
        }
    }
}
