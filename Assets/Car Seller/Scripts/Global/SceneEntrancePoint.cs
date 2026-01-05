using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEntrancePoint : MonoBehaviour
{
    public GameFlowController.GameSceneType gameSceneType;
    public string specificName()
    {
        return SceneManager.GetActiveScene().name;
    }
}