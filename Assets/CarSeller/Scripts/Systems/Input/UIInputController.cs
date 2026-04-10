using Pixelplacement;
using UnityEngine.InputSystem;

public class UIInputController : Singleton<UIInputController>
{
    public event System.Action onPaused;
    public event System.Action onResumed;

    public void OnPause(InputValue value)
    {
        onPaused?.Invoke();
    }

    public void OnResume(InputValue value)
    {

        onResumed?.Invoke();
    }
}
