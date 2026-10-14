// The action owner decides when input is accepted and when a transition is allowed.
// This buffer only retains the first request and its original input time.
public sealed class ActionInputBuffer<T>
{
    private T pendingRequest;
    private float pendingInputAt;
    public bool HasPending { get; private set; }

    public bool TryQueue(T request, float inputAt)
    {
        if (HasPending)
            return false;

        pendingRequest = request;
        pendingInputAt = inputAt;
        HasPending = true;
        return true;
    }

    public bool TryConsume(out T request, out float inputAt)
    {
        request = pendingRequest;
        inputAt = pendingInputAt;
        bool hadPending = HasPending;
        Clear();
        return hadPending;
    }

    public void Clear()
    {
        pendingRequest = default;
        pendingInputAt = 0f;
        HasPending = false;
    }
}
