public interface IInteractionManager
{
    void OnProductViewClick(Interactable interactable);
    void OnDragStart(Interactable interactable);
    void OnDragEnd(Interactable interactable);
    void OnTriggerEntered(ContentProvider trigger, ContentProvider triggerCause);
    void OnGameStateChanged(GameStateChangeEventData data);
}