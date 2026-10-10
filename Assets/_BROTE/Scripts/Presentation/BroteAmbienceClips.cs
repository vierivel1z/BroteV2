using UnityEngine;

// Shared official clips; AmbientZone remains the sole ambience playback component.
[CreateAssetMenu(menuName="BROTE/Audio/Ambience Clips")]
public sealed class BroteAmbienceClips : ScriptableObject
{
    public AudioClip wind, birds, river, leaves; // leaves = grass / vegetation rustle
}
