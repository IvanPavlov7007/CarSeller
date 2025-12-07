#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class DefaultSceneLoader
{
    private const string PrefsEnabledKey = "DefaultSceneLoader.Enabled";
    private const string PrefsScenePathKey = "DefaultSceneLoader.ScenePath";

    static DefaultSceneLoader()
    {
        EditorApplication.playModeStateChanged += LoadDefaultScene;
    }

    [MenuItem("Tools/Default Scene Loader/Toggle Enabled %#d")] // Ctrl/Cmd + Shift + D
    private static void ToggleEnabled()
    {
        bool current = IsEnabled();
        SetEnabled(!current);
        Debug.Log($"[Default Scene Loader] Enabled = {!current}");
    }

    [MenuItem("Tools/Default Scene Loader/Select Scene...")]
    private static void OpenSelector()
    {
        DefaultSceneLoaderWindow.ShowWindow();
    }

    [MenuItem("Tools/Default Scene Loader/Enabled", true)]
    private static bool ValidateEnabledItem()
    {
        Menu.SetChecked("Tools/Default Scene Loader/Enabled", IsEnabled());
        return true;
    }

    [MenuItem("Tools/Default Scene Loader/Enabled")]
    private static void EnabledMenuItem()
    {
        ToggleEnabled();
    }

    private static void LoadDefaultScene(PlayModeStateChange state)
    {
        // Prepare start scene before entering play mode
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (!IsEnabled())
            {
                EditorSceneManager.playModeStartScene = null; // Use current scene
                return;
            }

            // Persist current scenes if needed (only allowed in edit mode)
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // User declined saving; keep current scene and do not override start scene
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            // Resolve desired scene from selection or fallback to first enabled build scene
            var selectedPath = EditorPrefs.GetString(PrefsScenePathKey, string.Empty);
            var scenes = EditorBuildSettings.scenes;
            string pathToUse = null;

            if (!string.IsNullOrEmpty(selectedPath))
            {
                var sceneInBuild = scenes.FirstOrDefault(s => s.path == selectedPath && s.enabled);
                if (sceneInBuild != null)
                {
                    pathToUse = selectedPath;
                }
                else
                {
                    Debug.LogWarning($"[Default Scene Loader] Selected scene not in Build Settings or disabled: {selectedPath}. Falling back to index 0.");
                }
            }

            if (pathToUse == null)
            {
                var firstEnabled = scenes.FirstOrDefault(s => s.enabled);
                if (firstEnabled != null) pathToUse = firstEnabled.path;
            }

            if (string.IsNullOrEmpty(pathToUse))
            {
                Debug.LogWarning("[Default Scene Loader] No enabled scenes found in Build Settings. Using current scene.");
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            // Load the SceneAsset for playModeStartScene
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathToUse);
            if (sceneAsset == null)
            {
                Debug.LogWarning($"[Default Scene Loader] Could not load SceneAsset at path: {pathToUse}. Using current scene.");
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            EditorSceneManager.playModeStartScene = sceneAsset;
            // NOTE: Do not open the scene here; Unity will start play in this scene.
        }

        // Clear start scene after play ends to avoid unintended future runs
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorSceneManager.playModeStartScene = null;
        }

        // Never save or change scenes during EnteredPlayMode
        // (Unity throws InvalidOperationException if attempted)
    }

    private static bool IsEnabled() => EditorPrefs.GetBool(PrefsEnabledKey, true);
    private static void SetEnabled(bool enabled) => EditorPrefs.SetBool(PrefsEnabledKey, enabled);

    internal static void SetScenePath(string path)
    {
        EditorPrefs.SetString(PrefsScenePathKey, path ?? string.Empty);
        Debug.Log($"[Default Scene Loader] Selected scene: {path}");
    }

    internal static string GetScenePath() => EditorPrefs.GetString(PrefsScenePathKey, string.Empty);
}

public class DefaultSceneLoaderWindow : EditorWindow
{
    private string[] _sceneNames = new string[0];
    private string[] _scenePaths = new string[0];
    private int _selectedIndex = -1;

    public static void ShowWindow()
    {
        var window = GetWindow<DefaultSceneLoaderWindow>("Default Scene Loader");
        window.minSize = new Vector2(420, 120);
        window.Show();
    }

    private void OnEnable()
    {
        RefreshScenes();
        // Preselect currently stored scene path
        var currentPath = DefaultSceneLoader.GetScenePath();
        _selectedIndex = System.Array.IndexOf(_scenePaths, currentPath);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Default Scene Loader", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Enabled", GUILayout.Width(60));
            bool enabled = EditorPrefs.GetBool("DefaultSceneLoader.Enabled", true);
            bool newEnabled = EditorGUILayout.Toggle(enabled);
            if (newEnabled != enabled)
            {
                EditorPrefs.SetBool("DefaultSceneLoader.Enabled", newEnabled);
                Repaint();
            }
        }

        EditorGUILayout.Space();

        if (_sceneNames.Length == 0)
        {
            EditorGUILayout.HelpBox("No enabled scenes in Build Settings.\nAdd scenes via File > Build Settings.", MessageType.Warning);
            if (GUILayout.Button("Open Build Settings"))
            {
                EditorWindow.GetWindow(typeof(BuildPlayerWindow));
            }
            return;
        }

        EditorGUILayout.LabelField("Scene to load on Enter Play Mode:");
        int newIndex = EditorGUILayout.Popup(_selectedIndex < 0 ? 0 : _selectedIndex, _sceneNames);
        if (newIndex != _selectedIndex)
        {
            _selectedIndex = newIndex;
        }

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Refresh"))
            {
                RefreshScenes();
                // Keep selection consistent after refresh
                var currentPath = DefaultSceneLoader.GetScenePath();
                _selectedIndex = System.Array.IndexOf(_scenePaths, currentPath);
            }

            GUI.enabled = _selectedIndex >= 0;
            if (GUILayout.Button("Save Selection"))
            {
                DefaultSceneLoader.SetScenePath(_scenePaths[_selectedIndex]);
            }
            GUI.enabled = true;
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Current selection:", EditorStyles.miniBoldLabel);
        var current = DefaultSceneLoader.GetScenePath();
        EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(current) ? "(none)" : current, EditorStyles.textField, GUILayout.Height(18));
    }

    private void RefreshScenes()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
        _sceneNames = scenes.Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path)).ToArray();
        _scenePaths = scenes.Select(s => s.path).ToArray();

        // If there was no selection before, default to first scene
        if (_selectedIndex < 0 && _scenePaths.Length > 0)
        {
            var currentPath = DefaultSceneLoader.GetScenePath();
            _selectedIndex = string.IsNullOrEmpty(currentPath)
                ? 0
                : System.Array.IndexOf(_scenePaths, currentPath);
            if (_selectedIndex < 0) _selectedIndex = 0;
        }
    }
}
#endif