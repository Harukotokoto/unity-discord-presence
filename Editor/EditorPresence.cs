using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiscordEditorPresence
{
    [InitializeOnLoad]
    internal static class EditorPresence
    {
        private const string PreferencesPath = "Preferences/Discord Editor Presence";
        private const string ApplicationIdKey = "DiscordEditorPresence.ApplicationId";
        private const string DefaultApplicationId = "1574295036399583312";
        private const string SessionKey = "DiscordEditorPresence.SessionStart";
        // Store sharing preferences separately for each project.
        private static readonly string ProjectKey = "DiscordEditorPresence." + Application.dataPath + ".";

        private static bool enabled = EditorPrefs.GetBool(ProjectKey + "Enabled", false);
        private static bool showProject = EditorPrefs.GetBool(ProjectKey + "ShowProject", true);
        private static bool showScene = EditorPrefs.GetBool(ProjectKey + "ShowScene", true);
        private static string customApplicationId = EditorPrefs.GetString(ApplicationIdKey, "");
        private static DiscordPipe connection;
        private static double nextUpdate;
        private static long sessionStart;

        static EditorPresence()
        {
            if (Application.isBatchMode || !IsWindows)
                return;

            EditorApplication.update += Update;
            EditorApplication.quitting += Disconnect;
            AssemblyReloadEvents.beforeAssemblyReload += Disconnect;
        }

        private static bool IsWindows => Application.platform == RuntimePlatform.WindowsEditor;

        [MenuItem("Window/Discord Editor Presence")]
        private static void OpenPreferences()
        {
            SettingsService.OpenUserPreferences(PreferencesPath);
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettings()
        {
            return new SettingsProvider(PreferencesPath, SettingsScope.User)
            {
                guiHandler = DrawSettings
            };
        }

        private static void DrawSettings(string searchContext)
        {
            if (!IsWindows)
            {
                EditorGUILayout.HelpBox("Windows版のUnity Editorに対応しています。", MessageType.Info);
                return;
            }

            EditorGUI.BeginChangeCheck();
            bool newEnabled = EditorGUILayout.ToggleLeft("このプロジェクトのアクティビティを共有する", enabled);
            bool newShowProject = EditorGUILayout.ToggleLeft("プロジェクト名を表示", showProject);
            bool newShowScene = EditorGUILayout.ToggleLeft("アクティブシーン名を表示", showScene);
            var idLabel = new GUIContent("独自の Application ID", "空欄にすると配布版の Application ID を使用します。");
            string newId = EditorGUILayout.DelayedTextField(idLabel, customApplicationId).Trim();
            if (EditorGUI.EndChangeCheck())
            {
                if (!newEnabled || newId != customApplicationId ||
                    newShowProject != showProject || newShowScene != showScene)
                    Disconnect();

                enabled = newEnabled;
                customApplicationId = newId;
                showProject = newShowProject;
                showScene = newShowScene;
                EditorPrefs.SetBool(ProjectKey + "Enabled", enabled);
                EditorPrefs.SetString(ApplicationIdKey, customApplicationId);
                EditorPrefs.SetBool(ProjectKey + "ShowProject", showProject);
                EditorPrefs.SetBool(ProjectKey + "ShowScene", showScene);

                if (!enabled)
                    SessionState.EraseString(SessionKey);
                nextUpdate = 0;
                Update();
            }

            string status = "共有はオフです。";
            if (enabled)
                status = ValidId ? connection?.Status ?? "接続準備中…" : "独自の Application ID を確認してください。";
            EditorGUILayout.HelpBox(status, MessageType.Info);
        }

        private static string ApplicationId =>
            string.IsNullOrEmpty(customApplicationId) ? DefaultApplicationId : customApplicationId;

        private static bool ValidId =>
            ulong.TryParse(ApplicationId, NumberStyles.None, CultureInfo.InvariantCulture, out ulong id) && id > 0;

        private static void Update()
        {
            if (!enabled || !ValidId || !IsWindows || Application.isBatchMode ||
                EditorApplication.timeSinceStartup < nextUpdate)
                return;

            nextUpdate = EditorApplication.timeSinceStartup + 0.5;
            if (connection == null)
            {
                if (!long.TryParse(SessionState.GetString(SessionKey, ""), out sessionStart))
                {
                    sessionStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    SessionState.SetString(SessionKey, sessionStart.ToString(CultureInfo.InvariantCulture));
                }

                using (Process process = Process.GetCurrentProcess())
                    connection = new DiscordPipe(ApplicationId, process.Id);
            }

            Scene scene = SceneManager.GetActiveScene();
            string mode = "編集中";
            if (EditorApplication.isPlaying)
                mode = EditorApplication.isPaused ? "一時停止中" : "再生中";

            string sceneName = string.IsNullOrWhiteSpace(scene.name) ? "未保存のシーン" : scene.name;
            string projectName = Directory.GetParent(Application.dataPath).Name;
            string details = showProject ? "プロジェクト " + projectName : "Unity Editor";
            string state = mode + " · Unity " + Application.unityVersion;
            if (showScene)
                state += " · " + sceneName;

            connection.SetActivity(LimitText(details), LimitText(state), sessionStart);
        }

        private static string LimitText(string text)
        {
            const int limit = 128;
            if (Encoding.UTF8.GetByteCount(text) <= limit)
                return text;

            int length = Math.Min(text.Length, limit - 3);
            while (Encoding.UTF8.GetByteCount(text, 0, length) > limit - 3)
                length--;
            if (char.IsHighSurrogate(text[length - 1]))
                length--;

            return text.Substring(0, length) + "…";
        }

        private static void Disconnect()
        {
            connection?.Dispose();
            connection = null;
        }
    }
}
