using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace UnityTeamAssistant.Core
{
    public static class GitService
    {
        private static string WorkingDirectory => Application.dataPath + "/..";

        private static string RunCommand(string arguments, bool captureOutput = true)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = WorkingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = captureOutput,
                    RedirectStandardError = captureOutput,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            var output = new StringBuilder();
            process.OutputDataReceived += (sender, e) => output.AppendLine(e.Data);
            process.ErrorDataReceived += (sender, e) => output.AppendLine(e.Data);

            process.Start();
            if (captureOutput)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            process.WaitForExit();

            if (process.ExitCode != 0 && captureOutput)
            {
                UnityEngine.Debug.LogError($"Git error ({arguments}):\n{output}");
                throw new Exception($"Git command failed: {arguments}\n{output}");
            }

            return output.ToString();
        }

        public static bool IsGitRepository()
        {
            try
            {
                RunCommand("rev-parse --is-inside-work-tree");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetStatus()
        {
            return RunCommand("status --porcelain");
        }

        public static void StageAll()
        {
            RunCommand("add -A");
        }

        public static void Commit(string message)
        {
            RunCommand($"commit -m \"{message.Replace("\"", "\\\"")}\"");
        }

        public static void Push()
        {
            RunCommand("push");
        }

        public static void Pull()
        {
            RunCommand("pull --no-edit");
        }

        public static string GetCurrentBranch()
        {
            return RunCommand("branch --show-current").Trim();
        }

        public static string GetUserName()
        {
            try
            {
                return RunCommand("config user.name").Trim();
            }
            catch
            {
                return Environment.UserName;
            }
        }

        public static void AmendCommit(string newMessage = null)
        {
            string args = "commit --amend --no-edit";
            if (!string.IsNullOrEmpty(newMessage))
                args = $"commit --amend -m \"{newMessage.Replace("\"", "\\\"")}\"";
            RunCommand(args);
        }

        public static bool HasUncommittedChanges()
        {
            return !string.IsNullOrEmpty(GetStatus().Trim());
        }
    }
}