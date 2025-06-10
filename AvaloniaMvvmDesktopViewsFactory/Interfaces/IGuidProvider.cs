namespace AvaloniaMvvmDesktopViewsFactory.Interfaces
{
    public interface IGuidProvider
    {
        Guid GetUID();
        public void ReleaseGuid(Guid guidToRelease);
    }
}
