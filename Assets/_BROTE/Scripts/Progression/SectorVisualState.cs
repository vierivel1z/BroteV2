using System;
using UnityEngine;

public sealed class SectorVisualState : MonoBehaviour
{
    [Serializable]
    public sealed class ThresholdGroup
    {
        [Range(0f, 100f)] public float minimumPercentage;
        public GameObject[] objects = Array.Empty<GameObject>();
    }

    [SerializeField] private RestorationManager restorationManager;
    [SerializeField] private ThresholdGroup[] thresholds = Array.Empty<ThresholdGroup>();

    private void OnEnable()
    {
        if (restorationManager != null)
        {
            restorationManager.RestorationChanged += Apply;
            Apply(restorationManager.RestorationPercentage);
        }
        else Apply(0f);
    }

    private void OnDisable()
    {
        if (restorationManager != null) restorationManager.RestorationChanged -= Apply;
    }

    private void Apply(float percentage)
    {
        foreach (ThresholdGroup group in thresholds)
        {
            if (group == null || group.objects == null) continue;
            foreach (GameObject target in group.objects)
            {
                // Keep the controller and its ancestors active so it can receive future events.
                if (target == null || transform.IsChildOf(target.transform)) continue;
                bool active = percentage >= group.minimumPercentage;
                if (target.activeSelf != active) target.SetActive(active);
            }
        }
    }
}
