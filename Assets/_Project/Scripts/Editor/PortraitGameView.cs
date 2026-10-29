using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SheepStrike.EditorTools
{
    /// <summary>
    /// Puts the Game view on a 1080×1920 portrait size. The HUD is laid out for portrait, and in a landscape
    /// Game view the queue covers the animal. Unity has no public API for this, so it goes through reflection
    /// and only logs a warning if a future editor renames the internals.
    /// </summary>
    [InitializeOnLoad]
    public static class PortraitGameView
    {
        private const int Width = 1080;
        private const int Height = 1920;
        private const string SizeName = "SheepStrike Portrait";

        static PortraitGameView()
        {
            EditorApplication.delayCall += Apply;
        }

        [MenuItem("SheepStrike/Portrait Game View (1080x1920)")]
        public static void Apply()
        {
            try
            {
                var group = CurrentSizeGroup();
                int index = FindOrAddSize(group);
                SelectSize(index);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SheepStrike] Could not set the Game view to portrait, set 1080x1920 by hand: " + e.Message);
            }
        }

        private static object CurrentSizeGroup()
        {
            var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes = singleton.GetProperty("instance").GetValue(null);
            return sizesType.GetProperty("currentGroup").GetValue(sizes);
        }

        private static int FindOrAddSize(object group)
        {
            var groupType = group.GetType();
            var displayTexts = (string[])groupType.GetMethod("GetDisplayTexts").Invoke(group, null);
            for (int i = 0; i < displayTexts.Length; i++)
            {
                if (displayTexts[i].StartsWith(SizeName, StringComparison.Ordinal)) return i;
            }

            var sizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var kindType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
            var fixedResolution = Enum.Parse(kindType, "FixedResolution");
            var size = Activator.CreateInstance(sizeType, fixedResolution, Width, Height, SizeName);
            groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
            return (int)groupType.GetMethod("GetTotalCount").Invoke(group, null) - 1;
        }

        private static void SelectSize(int index)
        {
            var gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            gameViewType.GetProperty("selectedSizeIndex", flags).SetValue(gameView, index);
            gameView.Repaint();
        }
    }
}
