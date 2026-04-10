using Pixelplacement;
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