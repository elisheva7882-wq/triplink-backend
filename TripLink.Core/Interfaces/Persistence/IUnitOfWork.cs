namespace TripLink.Core.Interfaces.Persistence
{
    public interface IUnitOfWork
    {
        Task<IAppTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken);

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}