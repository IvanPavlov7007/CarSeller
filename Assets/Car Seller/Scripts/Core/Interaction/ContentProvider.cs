using UnityEngine;

public class ContentProvider : MonoBehaviour
{
    public object Model { get; private set; }

    public void Initialize(object model)
    {
        Model = model;
    }

    public T ProvideContent<T>(
        IInteractionContentProfile<T> profile,
        IInteractionContext context
        ) where T : IInteractionContent
    {
        return profile.GenerateContent(Model, context);
    }
}