namespace StackCache.Core.CacheKeys
{
    /// <summary>
    /// Key expiration mode, used internally
    /// </summary>
    public enum ExpirationMode
    {
        None,
        Sliding,
        AbsoluteUtc
    }
}