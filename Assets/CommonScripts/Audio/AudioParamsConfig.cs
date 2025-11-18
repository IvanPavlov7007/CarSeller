using System.Collections;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

[System.Serializable]
public sealed class AudioParamsConfig : SerializedDictionary<string, SerializedAudio>
{
}

[System.Serializable]
public class SerializedAudio
{
    public bool useClips = false;
    [ShowIf("useClips")]
    public AudioClip[] clips;
    [HideIf("useClips")]
    public string name;
    public float volume = 1.0f;
    [FoldoutGroup("Parameters"),Toggle("Enabled")]
    public AudioParams.Repetition repetition = new AudioParams.Repetition(0.5f);
    [FoldoutGroup("Parameters"), Toggle("Enabled")]
    public AudioParams.PitchVariation pitchVariation = new AudioParams.PitchVariation();
    [FoldoutGroup("Parameters"), Toggle("Enabled")]
    public AudioParams.Randomization randomization = new AudioParams.Randomization();
    [FoldoutGroup("Parameters"), Toggle("Enabled")]
    public AudioParams.Distortion distortion = new AudioParams.Distortion();
}