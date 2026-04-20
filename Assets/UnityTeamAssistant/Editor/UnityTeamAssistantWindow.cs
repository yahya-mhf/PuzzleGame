using UnityEditor;
using UnityEngine;

public class UnityTeamAssistantWindow : EditorWindow
{
    [MenuItem("Tools/Unity Team Assistant")]
    public static void ShowWindow()
    {
        GetWindow<UnityTeamAssistantWindow>("Team Assistant");
    }

    private void OnGUI()
    {
        DrawHeader();

        GUILayout.Space(10);

        DrawLocksSection();
        GUILayout.Space(10);

        DrawGitSection();
        GUILayout.Space(10);

        DrawTeamSection();
    }

    // ---------- HEADER ----------
    private void DrawHeader()
    {
        GUILayout.Label("Unity Team Assistant", EditorStyles.boldLabel);
        GUILayout.Label("Simple collaboration for Unity teams", EditorStyles.miniLabel);
    }

    // ---------- LOCKS ----------
    private void DrawLocksSection()
    {
        GUILayout.BeginVertical("box");

        GUILayout.Label("🔒 File Locks", EditorStyles.boldLabel);

        var data = LockManager.LoadLocks();

        if (data.locks.Count == 0)
        {
            GUILayout.Label("No active locks");
        }
        else
        {
            foreach (var l in data.locks)
            {
                GUILayout.BeginVertical("box");

                GUILayout.Label("📄 " + l.filePath);
                GUILayout.Label("👤 Locked by: " + l.lockedBy);
                GUILayout.Label("🕒 " + l.timestamp);

                string currentUser = System.Environment.UserName;

                if (l.lockedBy == currentUser)
                {
                    if (GUILayout.Button("Unlock"))
                    {
                        LockManager.Unlock(l.filePath, currentUser);
                    }
                }
                else
                {
                    GUILayout.Label("🔒 Locked (read-only)");
                }

                GUILayout.EndVertical();
            }
        }

        GUILayout.Space(10);

        GUILayout.Label("Current Selection:");

        string path = GetSelectedAssetPath();

        if (string.IsNullOrEmpty(path))
        {
            GUILayout.Label("No file selected");
        }
        else
        {
            GUILayout.Label(path);

            if (LockManager.IsLocked(path))
            {
                GUILayout.Label("Already locked");
            }
            else
            {
                if (GUILayout.Button("Lock Selected File"))
                {
                    LockManager.Lock(path, System.Environment.UserName);
                }
            }
        }

        GUILayout.EndVertical();
    }

    private string GetSelectedAssetPath()
    {
        var obj = UnityEditor.Selection.activeObject;
        if (obj == null) return null;

        return UnityEditor.AssetDatabase.GetAssetPath(obj);
    }

    // ---------- GIT ----------
    private void DrawGitSection()
    {
        GUILayout.BeginVertical("box");

        GUILayout.Label("🔁 Git Actions", EditorStyles.boldLabel);

        GUILayout.Space(5);

        GUILayout.Label("Commit Message:");
        string commitMessage = "Unity Team Update";

        if (GUILayout.Button("Save Work (Commit + Push)"))
        {
            GitManager.SaveWork(commitMessage);
        }

        if (GUILayout.Button("Update Project (Pull)"))
        {
            GitManager.UpdateProject();
        }

        GUILayout.EndVertical();
    }

    // ---------- TEAM ----------
    private void DrawTeamSection()
    {
        GUILayout.BeginVertical("box");

        GUILayout.Label("👥 Team Status", EditorStyles.boldLabel);
        GUILayout.Label("No active users");

        GUILayout.EndVertical();
    }

    private void OnEnable()
    {
        EditorApplication.update += RefreshWindow;
    }

    private void OnDisable()
    {
        EditorApplication.update -= RefreshWindow;
    }

    private void RefreshWindow()
    {
        Repaint();
    }
}