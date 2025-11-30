using UnityEngine;


/// <summary>
/// Interactable that can be dragged and provides drag direction
/// </summary>
///
public class DragInteractable : Interactable, IDirectionProvider
{
    bool dragging = false;

    public Vector2 ProvidedDirection { get; private set; }

    private void LateUpdate()
    {
        if (dragging)
        {
            ProvidedDirection = (GameCursor.Instance.transform.position - transform.position);
        }
    }

    protected override void OnCursorDragStart()
    {
        base.OnCursorDragStart();
        ProvidedDirection = Vector2.zero;
        dragging = true;

    }
    protected override void OnCursorDragEnd()
    {
        base.OnCursorDragEnd();
        dragging = false;
    }
}