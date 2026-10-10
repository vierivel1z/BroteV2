using UnityEngine;
using UnityEngine.Serialization;
[RequireComponent(typeof(Camera))]
public sealed class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [FormerlySerializedAs("offset")]
    [SerializeField] private Vector2 framingOffset;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.18f;
    [Header("Camera center bounds (world coordinates)")]
    [SerializeField] private bool useBounds;
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 6f;
    [Header("Keep viewport inside Ground horizontally and below")]
    [SerializeField] private bool constrainGroundViewport;
    [SerializeField] private Vector2 groundMin = new Vector2(-20f, -15f);
    [SerializeField] private Vector2 groundMax = new Vector2(20f, 15f);
    private Vector2 velocity;
    private Camera view;
    private void Awake() => view = GetComponent<Camera>();
    private void LateUpdate()
    {
        if (target == null) return;
        Vector2 desired = ConstrainPosition((Vector2)target.position + framingOffset);
        Vector2 p = Vector2.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        p = ConstrainPosition(p);
        transform.position = new Vector3(p.x, p.y, transform.position.z);
    }
    private Vector2 ConstrainPosition(Vector2 position)
    {
        if (!useBounds) return position;
        if (view == null) view = GetComponent<Camera>();
        Rect bounds = CalculateCenterBounds(view.aspect, view.orthographicSize, view.orthographic);
        return new Vector2(Mathf.Clamp(position.x, bounds.xMin, bounds.xMax), Mathf.Clamp(position.y, bounds.yMin, bounds.yMax));
    }
    public Rect CalculateCenterBounds(float aspect, float orthographicSize, bool orthographic = true)
    {
        float left = Mathf.Min(minX, maxX), right = Mathf.Max(minX, maxX);
        float bottom = Mathf.Min(minY, maxY), top = Mathf.Max(minY, maxY);
        if (constrainGroundViewport && orthographic)
        {
            float halfHeight = orthographicSize, halfWidth = halfHeight * aspect;
            left = Mathf.Max(left, groundMin.x + halfWidth);
            right = Mathf.Min(right, groundMax.x - halfWidth);
            bottom = Mathf.Max(bottom, groundMin.y + halfHeight);
        }
        if (left > right) left = right = (groundMin.x + groundMax.x) * 0.5f;
        if (bottom > top) bottom = top;
        return Rect.MinMaxRect(left, bottom, right, top);
    }
    private void OnDisable() => velocity = Vector2.zero;
}
