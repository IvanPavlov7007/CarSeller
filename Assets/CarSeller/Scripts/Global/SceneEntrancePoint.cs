using Pixelplacement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneEntrancePoint : Singleton<SceneEntrancePoint>
{
    public GameFlowController.GameSceneType gameSceneType;
    public string specificName()
    {
        return SceneManager.GetActiveScene().name;
    }
}