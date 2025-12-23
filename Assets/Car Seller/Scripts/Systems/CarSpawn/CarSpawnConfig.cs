using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarSpawnConfig", menuName = "Configs/Economy/CarSpawnConfig")]
public class CarSpawnConfig : ScriptableObject
{
    public int CarsToSpawnCount = 15;
    public CarSpawnEntry[] carSpawnEntries;

    public CarSpawnEntry[] GetRandomCarSpawnEntriesWithPuttingBack()
    {
        List<CarSpawnEntry> selectedEntries = new List<CarSpawnEntry>();
        for (int i = 0; i < CarsToSpawnCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, carSpawnEntries.Length);
            selectedEntries.Add(carSpawnEntries[randomIndex]);
        }
        return selectedEntries.ToArray();
    }

    [System.Serializable]
    public class CarSpawnEntry
    {
        public CarBaseConfig CarBaseConfig;
        public CarVariantConfig CarVariantConfig;
    }
}