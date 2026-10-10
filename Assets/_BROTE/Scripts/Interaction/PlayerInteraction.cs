using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public sealed class PlayerInteraction : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    private readonly List<Collider2D> hits = new List<Collider2D>(16);
    private readonly List<MonoBehaviour> behaviours = new List<MonoBehaviour>(16);
    private IInteractable current;
    public event System.Action<string> FeedbackRequested;
    private string lastFeedback;
    private float lastFeedbackTime = float.NegativeInfinity;
    public void ShowFeedback(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (message == lastFeedback && Time.unscaledTime - lastFeedbackTime < 0.8f) return;
        lastFeedback = message; lastFeedbackTime = Time.unscaledTime;
        BroteEvents.Notify(FeedbackRequested, message, this);
    }
    public IInteractable CurrentInteractable => current;
    public float InteractionRadius => interactionRadius;
    public bool IsWithinRange(IInteractable target)
    {
        return target!=null && (!(target is Object obj)||obj!=null) && target.CanInteract && ((Vector2)transform.position-target.InteractionPosition).sqrMagnitude<=interactionRadius*interactionRadius;
    }
    public static bool ShouldSwitch(float currentDistance,float candidateDistance,float margin=.08f)=>candidateDistance+margin<currentDistance;
    private void Update()
    {
        if(NurseryManagementPanel.IsOpen){current=null;return;}
        RefreshTarget();
        if(BrotePauseMenu.IsPaused)return;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && current != null && current.CanInteract)
            current.Interact(this);
    }
    private void RefreshTarget()
    {
        if(BrotePauseMenu.IsPaused){current=null;return;}
        bool currentPresent=false;
        var filter = new ContactFilter2D();
        filter.SetLayerMask(interactionLayers);
        filter.useTriggers = true;
        Physics2D.OverlapCircle(transform.position, interactionRadius, filter, hits);
        IInteractable nearest = null;
        float best = interactionRadius * interactionRadius;
        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            behaviours.Clear();
            hit.GetComponentsInParent(false, behaviours);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null || !behaviour.isActiveAndEnabled || !(behaviour is IInteractable candidate) || !candidate.CanInteract) continue;
                if(ReferenceEquals(candidate,current))currentPresent=true;
                float distance = ((Vector2)transform.position - candidate.InteractionPosition).sqrMagnitude;
                if (distance > best || (distance == best && nearest != null)) continue;
                best = distance;
                nearest = candidate;
            }
        }
        if(currentPresent&&IsWithinRange(current)&&nearest!=null&&!ReferenceEquals(current,nearest))
        {
            float previousDistance=Vector2.Distance(transform.position,current.InteractionPosition);
            if(!ShouldSwitch(previousDistance,Mathf.Sqrt(best)))nearest=current;
        }
        if (!ReferenceEquals(current, nearest))
        {
            current = nearest;
        }
    }
    private void OnDisable() => current = null;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
