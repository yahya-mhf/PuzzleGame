using System.Collections.Generic;
using System.Linq;
using UnityTeamAssistant.Core;
using UnityEngine;

namespace UnityTeamAssistant.Safety
{
    public class LockViolation
    {
        public string FilePath;
        public string LockedBy;
        public bool IsStale;
    }

    public static class CommitGuard
    {
        public static List<LockViolation> ScanForViolations(List<string> changedFiles, string currentUser)
        {
            var violations = new List<LockViolation>();
            var config = ConfigManager.LoadConfig();

            foreach (var file in changedFiles)
            {
                if (LockManager.TryGetLockOwner(file, out string owner))
                {
                    if (owner != currentUser)
                    {
                        if (LockManager.TryGetLockTimestamp(file, out string ts))
                        {
                            var entry = new LockEntry { Path = file, Owner = owner, Timestamp = ts };
                            violations.Add(new LockViolation
                            {
                                FilePath = file,
                                LockedBy = owner,
                                IsStale = LockManager.IsStale(entry, config.StaleTimeoutDays)
                            });
                        }
                    }
                }
            }
            return violations;
        }

        public static bool IsCommitAllowed(List<LockViolation> violations, bool strictMode, out string message)
        {
            message = "";
            if (violations.Count == 0) return true;

            var nonStaleViolations = violations.Where(v => !v.IsStale).ToList();
            if (nonStaleViolations.Count == 0)
            {
                message = "All locked files are stale (inactive >7 days). Proceed with caution.";
                return true; // Allow commit with warning
            }

            message = "Commit blocked: The following files are locked by others:\n";
            foreach (var v in nonStaleViolations)
            {
                message += $"- {v.FilePath} (locked by {v.LockedBy})\n";
            }

            if (!strictMode)
                message += "\nStrict mode is OFF. You may proceed anyway.";

            return !strictMode;
        }
    }
}