using System.IO;
using UnityEditor;
using UnityEngine;
using UnityTeamAssistant.Core;

namespace UnityTeamAssistant.UI
{
    [InitializeOnLoad]
    public static class ProjectIconOverlay
    {
        private static readonly Texture2D lockIcon;
        private static readonly GUIContent content;

        static ProjectIconOverlay()
        {
            // Create a simple lock icon texture
            lockIcon = new Texture2D(16, 16);
            var pixels = lockIcon.GetPixels();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            lockIcon.SetPixels(pixels);
            // Draw a simple lock shape (optional)
            lockIcon.Apply();

            content = new GUIContent();
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath)) return;
            if (assetPath.EndsWith(".meta")) return;

            if (LockManager.IsLocked(assetPath))
            {
                Rect iconRect = selectionRect;
                iconRect.x += iconRect.width - 20;
                iconRect.width = 16;
                iconRect.height = 16;

                if (LockManager.TryGetLockOwner(assetPath, out string owner))
                {
                    content.tooltip = $"Locked by {owner}";
                    if (LockManager.TryGetLockTimestamp(assetPath, out string ts))
                    {
                        content.tooltip += $"\nSince {ts}";
                    }
                }

                // Draw a red circle with L or just a text label
                GUI.Label(iconRect, "🔒", new GUIStyle(GUI.skin.label) { fontSize = 12 });
            }
        }
    }
}