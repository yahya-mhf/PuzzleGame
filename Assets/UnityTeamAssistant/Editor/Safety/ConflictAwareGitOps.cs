using System.Collections.Generic;
using System.Linq;
using UnityTeamAssistant.Core;
using UnityTeamAssistant.Systems;
using UnityTeamAssistant.UI;
using UnityEngine;

namespace UnityTeamAssistant.Safety
{
    public static class ConflictAwareGitOps
    {
        public static bool SaveWork(string commitMessage, bool force = false)
        {
            if (!GitService.IsGitRepository())
            {
                Notifications.ShowError("Not a Git repository.");
                return false;
            }

            var config = ConfigManager.LoadConfig();
            string currentUser = ConfigManager.GetUserName();

            // Get changed files
            string statusOutput = GitService.GetStatus();
            var changedFiles = ParseChangedFiles(statusOutput);

            var violations = CommitGuard.ScanForViolations(changedFiles, currentUser);

            // Determine if commit is allowed
            string blockMessage = string.Empty;
            bool allowed;

            if (force)
            {
                allowed = true;
                blockMessage = "Force commit enabled – bypassing lock checks.";
            }
            else
            {
                allowed = CommitGuard.IsCommitAllowed(violations, config.StrictMode, out blockMessage);
            }

            if (!allowed)
            {
                Notifications.ShowError(blockMessage);
                DashboardWindow.ShowWindow(); // Show locks
                return false;
            }

            // Proceed with commit
            try
            {
                string fullMessage = config.CommitMessagePrefix + " " + commitMessage;
                GitService.StageAll();
                GitService.Commit(fullMessage);
                GitService.Push();
                Notifications.ShowInfo($"Committed and pushed: {fullMessage}");
                return true;
            }
            catch (System.Exception e)
            {
                Notifications.ShowError($"Git operation failed: {e.Message}");
                return false;
            }
        }

        public static bool UpdateProject()
        {
            if (!GitService.IsGitRepository())
            {
                Notifications.ShowError("Not a Git repository.");
                return false;
            }

            try
            {
                GitService.Pull();
                Notifications.ShowInfo("Project updated (git pull successful).");
                DashboardWindow.RefreshIfOpen();
                return true;
            }
            catch (System.Exception e)
            {
                Notifications.ShowError($"Pull failed: {e.Message}");
                return false;
            }
        }

        private static List<string> ParseChangedFiles(string statusOutput)
        {
            var files = new List<string>();
            foreach (var line in statusOutput.Split('\n'))
            {
                if (line.Length > 3)
                {
                    string filePath = line.Substring(3).Trim();
                    if (!string.IsNullOrEmpty(filePath))
                        files.Add(filePath);
                }
            }
            return files;
        }
    }
}