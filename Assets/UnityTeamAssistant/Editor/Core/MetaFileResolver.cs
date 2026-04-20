using System.IO;
using UnityEngine;

namespace UnityTeamAssistant.Core
{
    public static class MetaFileResolver
    {
        public static string GetMetaPath(string assetPath)
        {
            return assetPath + ".meta";
        }

        public static bool ShouldLockMeta(string assetPath)
        {
            string ext = Path.GetExtension(assetPath).ToLower();
            return ext == ".unity" || ext == ".prefab";
        }

        public static void LockWithMetaIfNeeded(string assetPath, string userName)
        {
            LockManager.LockAsset(assetPath, userName);
            if (ShouldLockMeta(assetPath))
            {
                LockManager.LockAsset(GetMetaPath(assetPath), userName);
            }
        }

        public static void UnlockWithMetaIfNeeded(string assetPath, string userName)
        {
            LockManager.UnlockAsset(assetPath, userName);
            if (ShouldLockMeta(assetPath))
            {
                LockManager.UnlockAsset(GetMetaPath(assetPath), userName);
            }
        }
    }
}