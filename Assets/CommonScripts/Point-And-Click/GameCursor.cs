using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class GameCursor : Singleton<GameCursor>
{
    public Interactable currentInteractable { get; private set; }
    public Interactable draggedInteractabe { get; private set; }

    [SerializeField]
    private LayerMask interactableLayers;

    public bool use3dPhysics = false;

    private bool dragStarted = false;

    private void Start()
    {
        DirectInput.Instance.onClick += onClick;
        DirectInput.Instance.onPoint += onPoint;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        updateWorldPosition();
        processDrag();
    }

    private void processDrag()
    {
        if (draggedInteractabe != null)
        {
            
            if (!dragStarted)// TODO: check the time
            {
                //start Drag
                draggedInteractabe.DragStart();
                dragStarted = true;
            }
            else
            {
                //Drag
                draggedInteractabe.Drag();
            }
        }
    }

    private void endDrag()
    {
        if(draggedInteractabe == null)
        {
            Debug.Log("Cursor endDrag while draggedInteractable is not set");
            return;
        }
        draggedInteractabe.DragEnd();
        draggedInteractabe = null;
    }

    private void initializeDrag()
    {
        Debug.Assert(draggedInteractabe == null);

        dragStarted = false;
        draggedInteractabe = currentInteractable;
    }


    private void onClick(bool pressed)
    {
        if(pressed)
        {
            if (currentInteractable != null)
            {
                currentInteractable.CursorSelectStart();
                initializeDrag();
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable.CursorSelectEnd();
            }
            endDrag();
        }
    }

    Vector2 lastScreenPosition;
    private Vector2 updateWorldPosition()
    {
        Vector3 screenPos = new Vector3(lastScreenPosition.x, lastScreenPosition.y, -Camera.main.transform.position.z);
        Vector2 worldPostion = Camera.main.ScreenToWorldPoint(screenPos);
        transform.position = worldPostion;
        return worldPostion;
    }

    private void onPoint(Vector2 position)
    {
        lastScreenPosition = position;
        Vector2 worldPosition = updateWorldPosition();
        currentInteractable = updateCurrentInteractable(currentInteractable, worldPosition, interactableLayers);
    }

    private Interactable updateCurrentInteractable(Interactable current, Vector2 worldPosition, LayerMask layerMask)
    {
        var hitInteracrable = RaycastForInteractable(layerMask, worldPosition);

        if (hitInteracrable != current)
        {
            if (current != null)
            {
                current.CursorExit();
            }

            if (hitInteracrable != null)
            {
                hitInteracrable.CursorEnter();
            }
        }

        return hitInteracrable;
    }

    private Interactable RaycastForInteractable(LayerMask layerMask, Vector2 positoin)
    {
        Interactable hitInteractable = null;

        if (use3dPhysics)
        {
            var rayHits = Physics.SphereCastAll(positoin,0.1f, Vector3.up, 0.1f, layerMask);
            if (rayHits.Length > 0)
                hitInteractable = rayHits[0].transform.GetComponentInParent<Interactable>();
        }
        else
        {
            var rayHits = Physics2D.RaycastAll(positoin, Vector2.zero, 1000f, layerMask);
            if (rayHits.Length > 0)
                hitInteractable = rayHits[0].rigidbody.GetComponentInParent<Interactable>();
            // add sorting and comparing if there are many
        }

        return hitInteractable;
    }
}
