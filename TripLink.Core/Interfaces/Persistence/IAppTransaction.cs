namespace TripLink.Core.Interfaces.Persistence
{
    public interface IAppTransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken);

        Task RollbackAsync(CancellationToken cancellationToken);
    }
}