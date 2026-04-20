using UnityTeamAssistant.Core;
using UnityTeamAssistant.UI;

namespace UnityTeamAssistant.Systems
{
    public static class DashboardAutoPuller
    {
        public static void PullIfEnabled()
        {
            if (!ConfigManager.LoadConfig().AutoPullDashboard) return;
            if (!GitService.IsGitRepository()) return;

            try
            {
                GitService.Pull();
                // Reload locks after pull
                LockManager.LoadLocks(); // forces reload next time
                Notifications.ShowInfo("Dashboard auto-pulled latest changes.");
            }
            catch (System.Exception e)
            {
                Notifications.ShowError($"Auto-pull failed: {e.Message}");
            }
        }
    }
}