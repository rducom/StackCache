namespace StackCache.Core.Stores
{
    public class Crud<T>
    {
        public Crud(T value, CrudAction action)
        {
            this.Value = value;
            this.Action = action;
        }

        public T Value { get; private set; }
        public CrudAction Action { get; private set; }
    }
}