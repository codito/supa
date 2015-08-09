namespace Supa.Platform.Tests.TestDoubles
{
    using Supa.Platform;

    public class TestableTfsWorkItem<T> : TfsWorkItem
    {
        public TestableTfsWorkItem(object workItem) : base(workItem, typeof(T))
        {
        }
    }
}