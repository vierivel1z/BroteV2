using UnityEngine;

[System.Serializable]
public sealed class BroteVFXSettings
{
    [Tooltip("Leaf variants sharing one texture. Empty uses the procedural fallback.")]
    public Sprite[] leafSprites = new Sprite[0];
    [Tooltip("Dust variants sharing one texture. Empty uses the procedural fallback.")]
    public Sprite[] dustSprites = new Sprite[0];
    public Sprite pulseSprite;
    [Tooltip("Optional accent. Reuses the dust system only when dustCount is zero.")]
    public Sprite sparkSprite;
    [Range(0,2)] public int sparkCount;
    [Range(0,10)] public int dustCount=4,leafCount=3;
    [Range(.2f,.45f)] public float popDuration=.26f;
    [Range(.8f,1f)] public float startScale=.85f;
    [Range(1f,1.1f)] public float peakScale=1.05f;
    [Range(.2f,.46f)] public float leafLifetime=.38f;
    [Range(.03f,.15f)] public float leafMinSize=.065f,leafMaxSize=.11f;
    [Range(.2f,1.2f)] public float velocityMultiplier=1f;
    public bool emitPulse;
    [Range(.3f,1.2f)] public float pulseSize=.7f;
    [Range(.2f,.46f)] public float pulseLifetime=.38f;
}
