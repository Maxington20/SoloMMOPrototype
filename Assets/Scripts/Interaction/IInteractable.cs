using UnityEngine;

public interface IInteractable
{
    string InteractionName { get; }

    bool CanInteract(Transform interactor);

    void Interact(Transform interactor);
}