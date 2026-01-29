using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Pseudo;
public class ProcessRunner : RoutinedObject
{
    public bool Run(IProcess process)
    {
        return TryStartRoutine(process.Run());
    }
}
public interface IProcess
{
    public IEnumerator Run();
}