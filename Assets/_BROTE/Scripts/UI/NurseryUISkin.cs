using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "BROTE/UI/Nursery Skin")]
public sealed class NurseryUISkin : ScriptableObject
{
    public Sprite panelBackground;
    [Tooltip("Fallback 9-slice borders in source pixels: left, bottom, right, top. Used only when sprite borders are zero.")]
    public Vector4 panelSliceBorders = new Vector4(180, 100, 180, 100);
    public Vector4 cardSliceBorders = new Vector4(90, 70, 90, 70);
    public Sprite primaryNormal, primaryPressed, primaryDisabled;
    public Sprite buyAvailable, buyDisabled, buyMax;
    public Sprite iconMinka, iconSeedling, iconIrrigation, iconCapacity, iconSoilPreparation, iconExpansion;
    public Sprite productionTabActive, productionTabInactive;
    public Sprite upgradesTabActive, upgradesTabInactive;
    [HideInInspector] public Sprite tabActive, tabInactive; // Legacy skin compatibility.
    public Vector4 progressInsets = new Vector4(14, 8, 14, 8); // left, top, right, bottom
    public float panelSliceScale = .3f, cardSliceScale = .5f;
    public Sprite produceButtonBackground, buyButtonBackground, closeButtonBackground;
    public Sprite progressBackground, progressFill;
    public Sprite upgradeCardBackground;
    [Tooltip("Optional styling for hover, focus and disabled states.")]
    public StyleSheet additionalStyles;

    // Preserve original sprite colors; empty slots retain temporary functional colors.
    public static void Apply(VisualElement element, Sprite sprite, Color placeholder)
    {
        element.style.backgroundImage = sprite != null ? new StyleBackground(sprite) : new StyleBackground(StyleKeyword.None);
        element.style.backgroundColor = sprite != null ? Color.clear : placeholder;
        element.style.unityBackgroundImageTintColor = Color.white;
    }
}
