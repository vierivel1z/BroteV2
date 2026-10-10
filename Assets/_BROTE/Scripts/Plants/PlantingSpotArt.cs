using UnityEngine;

// Shared art references only; save/state transitions remain in PlantingSpot.
[CreateAssetMenu(menuName="BROTE/Art/Planting Spot Art")]
public sealed class PlantingSpotArt : ScriptableObject
{
    public Sprite locked, available, prepared;
    public Sprite ForState(PlantingSpot.SpotState state)
    {
        switch(state)
        {
            case PlantingSpot.SpotState.LOCKED:return locked;
            case PlantingSpot.SpotState.AVAILABLE:return available;
            case PlantingSpot.SpotState.PREPARED:return prepared;
            default:return null; // Planted terrain has no visible spot artwork.
        }
    }
}
