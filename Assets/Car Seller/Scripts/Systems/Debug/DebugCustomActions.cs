using Pixelplacement;

public class DebugCustomActions : Singleton<DebugCustomActions>
{
    private void OnEnable()
    {
        PlayerInputController.Instance.crouched += onActionOne;
        PlayerInputController.Instance.jumped += onActionTwo;
    }

    private void OnDisable()
    {
        PlayerInputController.Instance.crouched -= onActionOne;
        PlayerInputController.Instance.jumped -= onActionTwo;
    }

    void onActionOne()
    {
        FindAnyObjectByType<Interactable>().CursorClick();
    }

    void onActionTwo()
    {

    }
}