using UnityTeamAssistant.Core;
using UnityTeamAssistant.UI;

namespace UnityTeamAssistant.Systems
{
    public static class AutoCommitHandler
    {
        public static void CommitLocksFile()
        {
            if (!ConfigManager.LoadConfig().AutoCommitUnlock) return;
            if (!GitService.IsGitRepository()) return;

            try
            {
                // Only commit if locks.json is actually changed
                string status = GitService.GetStatus();
                if (status.Contains("locks.json"))
                {
                    GitService.StageAll();
                    GitService.Commit($"{ConfigManager.LoadConfig().CommitMessagePrefix} Update lock state");
                    GitService.Push();
                    Notifications.ShowInfo("Auto-committed lock changes.");
                }
            }
            catch (System.Exception e)
            {
                Notifications.ShowError($"Auto-commit failed: {e.Message}");
            }
        }
    }
}