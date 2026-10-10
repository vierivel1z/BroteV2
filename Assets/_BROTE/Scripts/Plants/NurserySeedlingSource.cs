using UnityEngine;

public sealed class NurserySeedlingSource : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip pickupClip;
    private void Awake()
    {
        var clips=Resources.Load<BroteSFXClips>("Audio/BroteSFXClips");
        if(pickupClip==null && clips!=null)pickupClip=clips.seedlingReady;
    }
    private int lastPickupFrame = -1;
    public bool CanInteract => isActiveAndEnabled;
    public Vector2 InteractionPosition => transform.position;
    public string InteractionPrompt => "[E] Gestionar vivero";

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract || player == null || lastPickupFrame == Time.frameCount) return;
        lastPickupFrame=Time.frameCount;
        NurseryManagementPanel.OpenFor(player);
    }
    public void ProductionFeedback() => BroteAudioFeedback.PlayOptional(pickupClip,transform.position);
}
