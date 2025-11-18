using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildIncrementor : IPreprocessBuildWithReport
{
    public int callbackOrder => 1;

    public void OnPreprocessBuild(BuildReport report)
    {
        string assetPath = "Assets/Resources/Build.asset";
        BuildScriptableObject buildScriptableObject = AssetDatabase.LoadAssetAtPath<BuildScriptableObject>(assetPath);

        if (buildScriptableObject == null)
        {
            buildScriptableObject = ScriptableObject.CreateInstance<BuildScriptableObject>();
            buildScriptableObject.IncrementBuildNumber(); // Initialize first version
            AssetDatabase.CreateAsset(buildScriptableObject, assetPath);
        }
        else
        {
            buildScriptableObject.IncrementBuildNumber(); // Increment for existing asset
            EditorUtility.SetDirty(buildScriptableObject); // Mark as dirty for saving
        }

        PlayerSettings.bundleVersion = buildScriptableObject.GetVersionString();
        AssetDatabase.SaveAssets();
    }
}
