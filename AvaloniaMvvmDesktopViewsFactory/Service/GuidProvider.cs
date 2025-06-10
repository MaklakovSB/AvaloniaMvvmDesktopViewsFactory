using AvaloniaMvvmDesktopViewsFactory.Interfaces;

namespace AvaloniaMvvmDesktopViewsFactory.Service
{
    public class GuidProvider : IGuidProvider
    {
        private readonly HashSet<Guid> _issuedGuids = new();
        private readonly object _lockObject = new();

        public GuidProvider() { }

        public Guid GetUID()
        {
            Guid newGuid;

            lock (_lockObject)
            {
                do
                {
                    newGuid = Guid.NewGuid();
                }
                while (_issuedGuids.Contains(newGuid));

                _issuedGuids.Add(newGuid);
            }

            return newGuid;
        }

        public void ReleaseGuid(Guid guidToRelease)
        {
            lock (_lockObject)
            {
                _issuedGuids.Remove(guidToRelease);
            }
        }
    }
}
