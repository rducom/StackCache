namespace StackCache.Core.Stores
{
    public struct Crud<T>
    {
        public T Value { get; set; }
        public CrudAction Action { get; set; }
    }
}