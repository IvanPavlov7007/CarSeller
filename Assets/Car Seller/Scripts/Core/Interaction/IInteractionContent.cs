public interface IInteractionContentProfile<T> where T : IInteractionContent
{
    T GenerateContent(
        object model,
        IInteractionContext context
    );
}

public interface IInteractionContent { }

public interface IInteractionContext {
}