using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class GitManager
{
    private static string repoPath => System.IO.Directory.GetParent(Application.dataPath).FullName;

    private static Task<string> RunGitAsync(string arguments, string operationName)
    {
        return Task.Run(() =>
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = info;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                    Debug.LogError($"❌ Git {operationName} ERROR:\n{error}");

                if (!string.IsNullOrWhiteSpace(output))
                    Debug.Log($"📄 Git {operationName}:\n{output}");

                return error; // important for error tracking
            }
        });
    }

    public static async void SaveWork(string message)
    {
        Debug.Log("⏳ Saving work...");

        string add = await RunGitAsync("add -A", "add");

        string commit = await RunGitAsync($"commit -m \"{message}\"", "commit");

        if (commit.Contains("nothing to commit"))
        {
            Debug.LogWarning("⚠️ Nothing to commit (no changes staged)");
            return;
        }

        string push = await RunGitAsync("push", "push");

        Debug.Log("✅ Save Work finished");
    }

    public static async void UpdateProject()
    {
        Debug.Log("⏳ Pulling latest changes...");

        await RunGitAsync("push", "push");

        Debug.Log("⬇️ Project updated");
    }
}