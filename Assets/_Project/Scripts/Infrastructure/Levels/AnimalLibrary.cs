using System;
using System.Collections.Generic;
using SheepStrike.Domain.Core;
using SheepStrike.Domain.Voxels;

namespace SheepStrike.Infrastructure.Levels
{
    /// <summary>
    /// Hand-made voxel animals (the sheep's enemies). Axes: X right, Y up, Z away from the camera (front face is the lowest Z).
    /// </summary>
    public static class AnimalLibrary
    {
        public const string Wolf = "wolf";
        public const string Fox = "fox";
        public const string Bear = "bear";
        public const string Boar = "boar";
        public const string AlphaWolf = "alpha_wolf";

        public static readonly IReadOnlyList<string> Rotation = new[] { Wolf, Fox, Bear, Boar, AlphaWolf };

        public static IReadOnlyDictionary<VoxelPos, GameColor> Build(string animalId)
        {
            switch (animalId)
            {
                case Wolf: return BuildWolf(GameColor.Yellow, false);
                case AlphaWolf: return BuildWolf(GameColor.Red, true);
                case Fox: return BuildFox();
                case Bear: return BuildBear();
                case Boar: return BuildBoar();
                default: throw new ArgumentException($"Unknown animal '{animalId}'.", nameof(animalId));
            }
        }

        private static IReadOnlyDictionary<VoxelPos, GameColor> Snapshot(VoxelModelBuilder builder)
        {
            var model = builder.Build();
            var cells = new Dictionary<VoxelPos, GameColor>(model.Count);
            foreach (var pair in model.All) cells.Add(pair.Key, pair.Value);
            return cells;
        }

        private static IReadOnlyDictionary<VoxelPos, GameColor> BuildWolf(GameColor eyeColor, bool alpha)
        {
            var b = new VoxelModelBuilder()
                .Box(2, 0, 3, 8, 6, 6, GameColor.Grey)          // body
                .Paint(4, 0, 3, 4, 5, 1, GameColor.Cream)       // chest fluff
                .Box(1, 6, 1, 10, 8, 8, GameColor.Grey)         // head
                .Box(3, 6, -2, 6, 3, 3, GameColor.Cream)        // muzzle
                .Box(5, 8, -3, 2, 1, 1, GameColor.Black)        // nose
                .Paint(3, 6, -2, 6, 1, 1, GameColor.Black)      // mouth line
                .Paint(2, 10, 1, 2, 2, 1, eyeColor)             // eyes
                .Paint(8, 10, 1, 2, 2, 1, eyeColor)
                .Paint(2, 12, 1, 3, 1, 1, GameColor.Black)      // angry brows
                .Paint(7, 12, 1, 3, 1, 1, GameColor.Black)
                .Box(1, 14, 3, 3, 4, 3, GameColor.Grey)         // ears
                .Box(8, 14, 3, 3, 4, 3, GameColor.Grey)
                .Paint(2, 14, 3, 1, 3, 1, GameColor.Pink)
                .Paint(9, 14, 3, 1, 3, 1, GameColor.Pink)
                .Box(10, 1, 7, 4, 2, 2, GameColor.Grey)         // tail
                .Paint(13, 1, 7, 1, 2, 2, GameColor.White)
                .Box(2, 0, 2, 2, 2, 1, GameColor.Grey)          // front paws
                .Box(8, 0, 2, 2, 2, 1, GameColor.Grey);

            if (alpha)
            {
                b.Paint(1, 13, 1, 10, 1, 8, GameColor.Black)    // dark mane
                 .Paint(2, 5, 3, 8, 1, 6, GameColor.Black)
                 .Box(0, 7, 4, 1, 4, 3, GameColor.Black)        // scarred cheeks
                 .Box(11, 7, 4, 1, 4, 3, GameColor.Black);
            }
            return Snapshot(b);
        }

        private static IReadOnlyDictionary<VoxelPos, GameColor> BuildFox()
        {
            var b = new VoxelModelBuilder()
                .Box(2, 0, 3, 7, 6, 5, GameColor.Orange)        // body
                .Paint(4, 0, 3, 3, 5, 1, GameColor.White)       // chest
                .Box(1, 6, 1, 9, 7, 7, GameColor.Orange)        // head
                .Paint(1, 6, 1, 3, 3, 1, GameColor.White)       // cheeks
                .Paint(7, 6, 1, 3, 3, 1, GameColor.White)
                .Box(3, 6, -2, 5, 3, 3, GameColor.White)        // pointed muzzle
                .Box(4, 8, -3, 3, 1, 1, GameColor.Black)        // nose
                .Paint(2, 10, 1, 2, 2, 1, GameColor.Black)      // eyes
                .Paint(7, 10, 1, 2, 2, 1, GameColor.Black)
                .Box(1, 13, 3, 3, 5, 2, GameColor.Orange)       // tall ears
                .Box(7, 13, 3, 3, 5, 2, GameColor.Orange)
                .Paint(1, 16, 3, 3, 2, 2, GameColor.Black)
                .Paint(7, 16, 3, 3, 2, 2, GameColor.Black)
                .Box(8, 0, 6, 5, 4, 4, GameColor.Orange)        // bushy tail
                .Box(12, 2, 6, 3, 4, 4, GameColor.Orange)
                .Paint(14, 2, 6, 1, 4, 4, GameColor.White);
            return Snapshot(b);
        }

        private static IReadOnlyDictionary<VoxelPos, GameColor> BuildBear()
        {
            var b = new VoxelModelBuilder()
                .Box(1, 0, 3, 11, 7, 7, GameColor.Brown)        // round body
                .Paint(4, 1, 3, 5, 5, 1, GameColor.Tan)         // belly
                .Box(1, 7, 1, 11, 9, 9, GameColor.Brown)        // big head
                .Box(4, 7, -1, 5, 4, 2, GameColor.Tan)          // muzzle
                .Box(5, 10, -2, 3, 2, 1, GameColor.Black)       // nose
                .Paint(3, 12, 1, 2, 2, 1, GameColor.Black)      // eyes
                .Paint(8, 12, 1, 2, 2, 1, GameColor.Black)
                .Box(0, 16, 4, 4, 3, 3, GameColor.Brown)        // round ears
                .Box(9, 16, 4, 4, 3, 3, GameColor.Brown)
                .Paint(1, 16, 4, 2, 2, 1, GameColor.Tan)
                .Paint(10, 16, 4, 2, 2, 1, GameColor.Tan)
                .Box(1, 0, 1, 3, 3, 2, GameColor.Brown)         // paws
                .Box(9, 0, 1, 3, 3, 2, GameColor.Brown)
                .Paint(1, 0, 1, 3, 1, 1, GameColor.Black)
                .Paint(9, 0, 1, 3, 1, 1, GameColor.Black);
            return Snapshot(b);
        }

        private static IReadOnlyDictionary<VoxelPos, GameColor> BuildBoar()
        {
            var b = new VoxelModelBuilder()
                .Box(1, 0, 2, 12, 7, 9, GameColor.Brown)        // stocky body
                .Paint(1, 6, 2, 12, 1, 9, GameColor.Black)      // bristly back
                .Box(2, 2, -3, 10, 8, 6, GameColor.Brown)       // head
                .Box(4, 3, -5, 6, 4, 2, GameColor.Pink)         // snout
                .Paint(5, 4, -5, 1, 2, 1, GameColor.Black)      // nostrils
                .Paint(8, 4, -5, 1, 2, 1, GameColor.Black)
                .Box(3, 2, -5, 1, 4, 1, GameColor.White)        // tusks
                .Box(10, 2, -5, 1, 4, 1, GameColor.White)
                .Paint(3, 7, -3, 2, 2, 1, GameColor.Red)        // angry eyes
                .Paint(9, 7, -3, 2, 2, 1, GameColor.Red)
                .Box(2, 10, -1, 3, 3, 2, GameColor.Black)       // ears
                .Box(9, 10, -1, 3, 3, 2, GameColor.Black)
                .Box(1, -3, 2, 3, 3, 3, GameColor.Black)        // hooves
                .Box(10, -3, 2, 3, 3, 3, GameColor.Black)
                .Box(1, -3, 8, 3, 3, 3, GameColor.Black)
                .Box(10, -3, 8, 3, 3, 3, GameColor.Black);
            return Snapshot(b);
        }
    }
}
