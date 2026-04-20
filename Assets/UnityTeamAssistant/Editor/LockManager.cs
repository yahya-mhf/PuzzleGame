using System.IO;
using UnityEngine;

public static class LockManager
{
    private static string LockFilePath => Application.dataPath + "/UnityTeamAssistant/Resources/locks.json";

    public static LockFile LoadLocks()
    {
        if (!File.Exists(LockFilePath))
        {
            return new LockFile();
        }

        string json = File.ReadAllText(LockFilePath);
        return JsonUtility.FromJson<LockFile>(json);
    }

    public static void SaveLocks(LockFile lockFile)
    {
        string json = JsonUtility.ToJson(lockFile, true);
        File.WriteAllText(LockFilePath, json);
    }

    public static bool IsLocked(string path)
    {
        var data = LoadLocks();
        return data.locks.Exists(l => l.filePath == path);
    }

    public static string GetLocker(string path)
    {
        var data = LoadLocks();
        var entry = data.locks.Find(l => l.filePath == path);
        return entry != null ? entry.lockedBy : null;
    }

    public static void Lock(string path, string user)
    {
        var data = LoadLocks();

        if (data.locks.Exists(l => l.filePath == path))
        {
            Debug.LogWarning("File already locked!");
            return;
        }

        data.locks.Add(new LockEntry
        {
            filePath = path,
            lockedBy = user,
            timestamp = System.DateTime.Now.ToString()
        });

        SaveLocks(data);
    }

    public static void Unlock(string path, string user)
    {
        var data = LoadLocks();

        var entry = data.locks.Find(l => l.filePath == path);
        if (entry == null) return;

        // Simple safety: only owner can unlock (for now)
        if (entry.lockedBy != user)
        {
            Debug.LogWarning("You cannot unlock a file locked by someone else.");
            return;
        }

        data.locks.Remove(entry);
        SaveLocks(data);
    }
}