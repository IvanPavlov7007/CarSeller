public class CarStealGameMain : GameMain
{
    public override void AfterSceneLoad(ISceneMain sceneMain)
    {
        base.AfterSceneLoad(sceneMain);
        if(sceneMain is CitySceneMain)
            G.CarSpawnManager.CheckAndRefill();
    }
}