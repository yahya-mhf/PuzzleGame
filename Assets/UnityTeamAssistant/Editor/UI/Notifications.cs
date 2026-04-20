using UnityEditor;
using UnityEngine;

namespace UnityTeamAssistant.UI
{
    public static class Notifications
    {
        public static void ShowInfo(string message)
        {
            Debug.Log($"[UTA] {message}");
            EditorUtility.DisplayDialog("Unity Team Assistant", message, "OK");
        }

        public static void ShowWarning(string message)
        {
            Debug.LogWarning($"[UTA] {message}");
            EditorUtility.DisplayDialog("Unity Team Assistant - Warning", message, "OK");
        }

        public static void ShowError(string message)
        {
            Debug.LogError($"[UTA] {message}");
            EditorUtility.DisplayDialog("Unity Team Assistant - Error", message, "OK");
        }

        public static bool ShowConfirmation(string message, string ok = "Yes", string cancel = "No")
        {
            return EditorUtility.DisplayDialog("Unity Team Assistant", message, ok, cancel);
        }
    }
}