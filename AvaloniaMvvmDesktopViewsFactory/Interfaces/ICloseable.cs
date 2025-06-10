namespace AvaloniaMvvmDesktopViewsFactory.Interfaces
{
    public interface ICloseable<TResult>
    {
        public event EventHandler<TResult> Close;
        Task<TResult> Result { get; }
        void TrySetDefaultResult();
    }
}
