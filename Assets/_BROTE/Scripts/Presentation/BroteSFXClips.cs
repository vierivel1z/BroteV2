using UnityEngine;

// Shared authored references only; existing managers retain playback and volume ownership.
[CreateAssetMenu(menuName="BROTE/Audio/Official SFX Clips")]
public sealed class BroteSFXClips : ScriptableObject
{
    public AudioClip plant, seedlingReady, growth, upgrade, sectorRestored;
}
