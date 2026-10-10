using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Sorts from this Transform's world-space support point (feet / trunk base).</summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class YSort2D : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float multiplier = 100f;
    [SerializeField] private int offset;

    private SpriteRenderer spriteRenderer;
    private SortingGroup sortingGroup;
    private int lastOrder = int.MinValue;

    private void OnEnable()
    {
        // A group takes priority for objects made of several sprites.
        sortingGroup = GetComponent<SortingGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastOrder = int.MinValue;
        ApplySorting();
    }

    private void LateUpdate() => ApplySorting();

    private void ApplySorting()
    {
        if (sortingGroup == null && spriteRenderer == null) return;
        // Unity's sortingOrder is limited to a signed 16-bit value.
        int order = CalculateOrder(transform.position.y, multiplier, offset);
        if (order == lastOrder) return;
        lastOrder = order;
        if (sortingGroup != null) sortingGroup.sortingOrder = order;
        else spriteRenderer.sortingOrder = order;
    }

    public static int CalculateOrder(float y, float multiplier, int offset)
    {
        float calculated = Mathf.Round(-y * multiplier) + offset;
        return Mathf.RoundToInt(Mathf.Clamp(calculated, short.MinValue, short.MaxValue));
    }

    private void OnValidate()
    {
        multiplier = Mathf.Max(0.01f, multiplier);
        lastOrder = int.MinValue;
    }
}

