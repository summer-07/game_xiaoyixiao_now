using System.Collections.Generic;
using SheepStrike.Domain.Core;
using SheepStrike.Presentation.Theme;
using UnityEngine;

namespace SheepStrike.Presentation.Views
{
    /// <summary>Shared meshes and one material per colour, so cubes of the same colour batch together.</summary>
    public sealed class RenderKit
    {
        private const string ShaderName = "SheepStrike/VoxelToon";
        // Built-in and always included in player builds, so trails work in WebGL without a material asset.
        private const string TrailShaderName = "Sprites/Default";

        private readonly Shader _shader;
        private readonly Dictionary<GameColor, Material> _materials = new Dictionary<GameColor, Material>();

        public RenderKit()
        {
            _shader = Shader.Find(ShaderName) ?? Shader.Find("Unlit/Color");
            CubeMesh = ExtractMesh(PrimitiveType.Cube);
            SphereMesh = ExtractMesh(PrimitiveType.Sphere);
            TrailMaterial = new Material(Shader.Find(TrailShaderName) ?? _shader) { name = "WoolTrail" };
        }

        public Mesh CubeMesh { get; }

        public Mesh SphereMesh { get; }

        /// <summary>Vertex-coloured, alpha-blended material; each trail tints it through its own gradient.</summary>
        public Material TrailMaterial { get; }

        public Material MaterialOf(GameColor color)
        {
            if (_materials.TryGetValue(color, out var material)) return material;
            material = new Material(_shader) { name = "Voxel_" + color, enableInstancing = true };
            material.color = Palette.Of(color);
            _materials.Add(color, material);
            return material;
        }

        public GameObject CreateRenderer(string name, Mesh mesh, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go;
        }

        private static Mesh ExtractMesh(PrimitiveType type)
        {
            var temp = GameObject.CreatePrimitive(type);
            var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(temp);
            return mesh;
        }
    }
}
