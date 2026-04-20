using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityTeamAssistant.Core;
using UnityTeamAssistant.Systems;

namespace UnityTeamAssistant.UI
{
    public class DashboardWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private bool showStale = true;

        [MenuItem("Tools/Unity Team Assistant/Team Dashboard", false, 102)]
        public static void ShowWindow()
        {
            var window = GetWindow<DashboardWindow>("Team Dashboard");
            window.minSize = new Vector2(400, 300);
            DashboardAutoPuller.PullIfEnabled();
        }

        public static void RefreshIfOpen()
        {
            var window = GetWindow<DashboardWindow>(false);
            if (window != null) window.Repaint();
        }

        private void OnEnable()
        {
            DashboardAutoPuller.PullIfEnabled();
        }

        private void OnGUI()
        {
            if (!GitService.IsGitRepository())
            {
                EditorGUILayout.HelpBox("Not a Git repository.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Team Locks", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                DashboardAutoPuller.PullIfEnabled();
                LockManager.LoadLocks(); // force reload
            }
            EditorGUILayout.EndHorizontal();

            showStale = EditorGUILayout.Toggle("Show stale locks", showStale);

            var locks = LockManager.GetAllActiveLocks();
            var config = ConfigManager.LoadConfig();
            string currentUser = ConfigManager.GetUserName();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var l in locks.OrderBy(l => l.Path))
            {
                bool isStale = LockManager.IsStale(l, config.StaleTimeoutDays);
                if (!showStale && isStale) continue;

                EditorGUILayout.BeginHorizontal();

                // Icon
                if (l.Owner == currentUser)
                    GUI.color = Color.green;
                else
                    GUI.color = Color.red;
                GUILayout.Label("🔒", GUILayout.Width(20));
                GUI.color = Color.white;

                // Path
                GUILayout.Label(l.Path, GUILayout.MinWidth(150));

                // Owner
                GUILayout.Label(l.Owner, GUILayout.Width(80));

                // Timestamp / Stale
                if (DateTime.TryParse(l.Timestamp, out var dt))
                {
                    string timeAgo = GetTimeAgo(dt);
                    if (isStale)
                    {
                        GUI.color = Color.yellow;
                        GUILayout.Label($"⚠ {timeAgo}", GUILayout.Width(80));
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUILayout.Label(timeAgo, GUILayout.Width(80));
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (locks.Count == 0)
            {
                GUILayout.Label("No active locks.");
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Summary
            var otherLocks = locks.Where(l => l.Owner != currentUser).ToList();
            if (otherLocks.Count > 0)
            {
                EditorGUILayout.HelpBox($"{otherLocks.Count} asset(s) locked by teammates.", MessageType.Info);
            }

            if (GUILayout.Button("Open Locking Window"))
            {
                LockingWindow.ShowWindow();
            }
        }

        private string GetTimeAgo(DateTime dt)
        {
            var span = DateTime.UtcNow - dt;
            if (span.TotalDays >= 1) return $"{(int)span.TotalDays}d ago";
            if (span.TotalHours >= 1) return $"{(int)span.TotalHours}h ago";
            if (span.TotalMinutes >= 1) return $"{(int)span.TotalMinutes}m ago";
            return "just now";
        }
    }
}