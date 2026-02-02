using System;
using UnityEngine;

public class MissionMonoBehaviourHelper : MonoBehaviour
{
    public event Action<float> OnUpdateEvent;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        OnUpdateEvent?.Invoke(Time.deltaTime);
    }
}