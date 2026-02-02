using UnityEngine;

public class ContentProvider : MonoBehaviour
{
    public object Model { get; private set; }

    public void Initialize(object model)
    {
        Model = model;
    }

    public T ProvideContent<T,U>(
        IInteractionContentProfile<T,U> profile,
        U context
        ) where T : IInteractionContent where U : IInteractionContext
    {
        return profile.GenerateContent(Model, context);
    }
}