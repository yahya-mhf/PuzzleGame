using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityTeamAssistant.Core
{
    [Serializable]
    public class LockEntry
    {
        public string Path;
        public string Owner;
        public string Timestamp;
    }

    [Serializable]
    public class LockData
    {
        public List<LockEntry> Locks = new List<LockEntry>();
    }

    public static class LockManager
    {
        private const string RESOURCE_PATH = "locks";
        private static LockData cachedLockData;
        private static string fullJsonPath;

        static LockManager()
        {
            fullJsonPath = Path.Combine(Application.dataPath, "UnityTeamAssistant/Resources/locks.json");
        }

        private static void EnsureDirectoryExists()
        {
            string dir = Path.GetDirectoryName(fullJsonPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public static LockData LoadLocks()
        {
            if (cachedLockData != null)
                return cachedLockData;

            EnsureDirectoryExists();
            if (File.Exists(fullJsonPath))
            {
                string json = File.ReadAllText(fullJsonPath);
                cachedLockData = JsonUtility.FromJson<LockData>(json) ?? new LockData();
            }
            else
            {
                cachedLockData = new LockData();
                SaveLocks();
            }
            return cachedLockData;
        }

        public static void SaveLocks()
        {
            EnsureDirectoryExists();
            string json = JsonUtility.ToJson(cachedLockData ?? new LockData(), true);
            File.WriteAllText(fullJsonPath, json);
            AssetDatabase.Refresh();
        }

        public static bool IsLocked(string assetPath)
        {
            var data = LoadLocks();
            return data.Locks.Any(l => l.Path == assetPath);
        }

        public static bool TryGetLockOwner(string assetPath, out string owner)
        {
            owner = null;
            var data = LoadLocks();
            var entry = data.Locks.FirstOrDefault(l => l.Path == assetPath);
            if (entry != null)
            {
                owner = entry.Owner;
                return true;
            }
            return false;
        }

        public static bool TryGetLockTimestamp(string assetPath, out string timestamp)
        {
            timestamp = null;
            var data = LoadLocks();
            var entry = data.Locks.FirstOrDefault(l => l.Path == assetPath);
            if (entry != null)
            {
                timestamp = entry.Timestamp;
                return true;
            }
            return false;
        }

        public static bool LockAsset(string assetPath, string userName)
        {
            if (string.IsNullOrEmpty(assetPath)) return false;
            if (IsLocked(assetPath)) return false;

            var data = LoadLocks();
            data.Locks.Add(new LockEntry
            {
                Path = assetPath,
                Owner = userName,
                Timestamp = DateTime.UtcNow.ToString("o")
            });
            SaveLocks();
            cachedLockData = data;
            return true;
        }

        public static bool UnlockAsset(string assetPath, string userName)
        {
            var data = LoadLocks();
            var entry = data.Locks.FirstOrDefault(l => l.Path == assetPath);
            if (entry == null) return false;
            if (entry.Owner != userName) return false;

            data.Locks.Remove(entry);
            SaveLocks();
            cachedLockData = data;
            return true;
        }

        public static List<LockEntry> GetAllActiveLocks()
        {
            return LoadLocks().Locks.ToList();
        }

        public static bool IsStale(LockEntry entry, int staleDays = 7)
        {
            if (DateTime.TryParse(entry.Timestamp, out var lockTime))
            {
                return (DateTime.UtcNow - lockTime).TotalDays > staleDays;
            }
            return false;
        }

        public static void ReleaseAllLocksForUser(string userName)
        {
            var data = LoadLocks();
            data.Locks.RemoveAll(l => l.Owner == userName);
            SaveLocks();
            cachedLockData = data;
        }
    }
}