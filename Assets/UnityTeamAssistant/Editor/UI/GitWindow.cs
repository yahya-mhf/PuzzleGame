using UnityEditor;
using UnityEngine;
using UnityTeamAssistant.Core;
using UnityTeamAssistant.Safety;
using UnityTeamAssistant.UI;

namespace UnityTeamAssistant.UI
{
    public class GitWindow : EditorWindow
    {
        private string commitMessage = "";
        private bool showAdvanced = false;
        private bool forceCommit = false;
        private Vector2 statusScroll;

        [MenuItem("Tools/Unity Team Assistant/Git Operations", false, 101)]
        public static void ShowWindow()
        {
            GetWindow<GitWindow>("Git Assistant");
        }

        private void OnGUI()
        {
            if (!GitService.IsGitRepository())
            {
                EditorGUILayout.HelpBox("Not a Git repository.", MessageType.Error);
                return;
            }

            string branch = GitService.GetCurrentBranch();
            GUILayout.Label($"Branch: {branch}", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            // Save Work
            GUILayout.Label("Save Work", EditorStyles.boldLabel);
            commitMessage = EditorGUILayout.TextField("Commit Message", commitMessage);
            forceCommit = EditorGUILayout.Toggle("Force Commit (ignore locks)", forceCommit);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Work (Add, Commit, Push)", GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(commitMessage))
                {
                    Notifications.ShowWarning("Please enter a commit message.");
                    return;
                }

                bool success = ConflictAwareGitOps.SaveWork(commitMessage, forceCommit);
                if (success)
                {
                    commitMessage = "";
                    forceCommit = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Update Project
            GUILayout.Label("Update Project", EditorStyles.boldLabel);
            if (GUILayout.Button("Update Project (Pull)", GUILayout.Height(30)))
            {
                ConflictAwareGitOps.UpdateProject();
            }

            EditorGUILayout.Space();

            // Status
            GUILayout.Label("Git Status", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh Status"))
            {
                // Just repaint
            }
            statusScroll = EditorGUILayout.BeginScrollView(statusScroll, GUILayout.Height(120));
            string status = GitService.GetStatus();
            if (string.IsNullOrEmpty(status))
                GUILayout.Label("Working tree clean.");
            else
                GUILayout.Label(status);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Advanced
            showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced");
            if (showAdvanced)
            {
                EditorGUI.indentLevel++;
                if (GUILayout.Button("Amend Last Commit (Add + Amend)"))
                {
                    if (Notifications.ShowConfirmation("Amend last commit? This rewrites history."))
                    {
                        GitService.StageAll();
                        GitService.AmendCommit();
                        Notifications.ShowInfo("Commit amended.");
                    }
                }
                if (GUILayout.Button("Push"))
                {
                    GitService.Push();
                    Notifications.ShowInfo("Pushed.");
                }
                if (GUILayout.Button("Pull"))
                {
                    ConflictAwareGitOps.UpdateProject();
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}