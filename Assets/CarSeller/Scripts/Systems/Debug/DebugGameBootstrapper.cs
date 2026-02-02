using Pixelplacement;
using UnityEditor;
using UnityEngine;
public class DebugGameBootstrapper : Singleton<DebugGameBootstrapper>
{
    private void Awake()
    {
        if(Singleton<DebugGameBootstrapper>.Instance != this)
        {
            return;
        }
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

    }
}