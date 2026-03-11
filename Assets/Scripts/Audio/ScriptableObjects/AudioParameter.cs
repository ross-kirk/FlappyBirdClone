using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioParameter", menuName = "Audio/Audio Parameter")]
public class AudioParameter : ScriptableObject
{
    public float minValue = 0f;
    public float maxValue = 1f;
    public float defaultValue = 0f;
}
