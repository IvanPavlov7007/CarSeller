using UnityEngine;

public abstract partial class GameMain
{
    public class CitySceneMain : ISceneMain
    {
        public void Enter()
        {
            G.GameFlowController.SetCity();
            CitySceneManager.Instance.SetCurrentProfile((G.GameState));
            CitySceneManager.Instance.InitializeCity();
            setRootActive(true);
            CameraHelper.SetCurrentPositionAtCar();
        }

        public void Exit()
        {
            setRootActive(false);
        }

        void setRootActive(bool active)
        {
            if (G.CityRoot != null)
            {
                G.CityRoot.SetActive(active);
            }
            else
            {
                Debug.LogWarning("CityRoot is not set");
            }
        }
    }
}