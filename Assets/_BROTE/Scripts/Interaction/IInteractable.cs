using UnityEngine;
public interface IInteractable
{
    bool CanInteract { get; }
    Vector2 InteractionPosition { get; }
    string InteractionPrompt { get; }
    void Interact(PlayerInteraction player);
}
