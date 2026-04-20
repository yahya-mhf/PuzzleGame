using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityTeamAssistant.Core;
using UnityTeamAssistant.Systems;

namespace UnityTeamAssistant.UI
{
    public class LockingWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private string selectedAssetPath;

        [MenuItem("Tools/Unity Team Assistant/Lock Assets", false, 100)]
        public static void ShowWindow()
        {
            GetWindow<LockingWindow>("Lock Assets");
        }

        private void OnGUI()
        {
            if (!GitService.IsGitRepository())
            {
                EditorGUILayout.HelpBox("Not a Git repository. Locking disabled.", MessageType.Warning);
                return;
            }

            GUILayout.Label("Select an asset to lock/unlock", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            // Show current selection
            if (Selection.activeObject != null)
            {
                selectedAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                GUILayout.Label($"Selected: {selectedAssetPath}");
            }
            else
            {
                selectedAssetPath = null;
                GUILayout.Label("No asset selected.");
            }

            EditorGUILayout.Space();

            string currentUser = ConfigManager.GetUserName();
            bool isLocked = !string.IsNullOrEmpty(selectedAssetPath) && LockManager.IsLocked(selectedAssetPath);
            string lockOwner = "";
            if (isLocked)
                LockManager.TryGetLockOwner(selectedAssetPath, out lockOwner);

            GUI.enabled = !string.IsNullOrEmpty(selectedAssetPath) && (!isLocked || lockOwner == currentUser);

            if (!isLocked)
            {
                if (GUILayout.Button("Lock Asset"))
                {
                    MetaFileResolver.LockWithMetaIfNeeded(selectedAssetPath, currentUser);
                    Notifications.ShowInfo($"Locked: {selectedAssetPath}");
                    AutoCommitHandler.CommitLocksFile();
                    DashboardWindow.RefreshIfOpen();
                }
            }
            else
            {
                if (lockOwner == currentUser)
                {
                    GUI.color = Color.yellow;
                    if (GUILayout.Button("Unlock Asset"))
                    {
                        MetaFileResolver.UnlockWithMetaIfNeeded(selectedAssetPath, currentUser);
                        Notifications.ShowInfo($"Unlocked: {selectedAssetPath}");
                        AutoCommitHandler.CommitLocksFile();
                        DashboardWindow.RefreshIfOpen();
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    EditorGUILayout.HelpBox($"Locked by {lockOwner}", MessageType.Info);
                }
            }

            GUI.enabled = true;

            EditorGUILayout.Space();

            // Show all locks for current user
            GUILayout.Label("Your Locks", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));
            var myLocks = LockManager.GetAllActiveLocks().Where(l => l.Owner == currentUser).ToList();
            if (myLocks.Count == 0)
            {
                GUILayout.Label("No active locks.");
            }
            else
            {
                foreach (var l in myLocks)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(l.Path);
                    if (GUILayout.Button("Unlock", GUILayout.Width(60)))
                    {
                        MetaFileResolver.UnlockWithMetaIfNeeded(l.Path, currentUser);
                        Notifications.ShowInfo($"Unlocked: {l.Path}");
                        AutoCommitHandler.CommitLocksFile();
                        DashboardWindow.RefreshIfOpen();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Release All My Locks"))
            {
                if (Notifications.ShowConfirmation("Release all your locks?"))
                {
                    LockManager.ReleaseAllLocksForUser(currentUser);
                    AutoCommitHandler.CommitLocksFile();
                    DashboardWindow.RefreshIfOpen();
                }
            }
        }

        private void OnSelectionChange()
        {
            Repaint();
        }
    }
}