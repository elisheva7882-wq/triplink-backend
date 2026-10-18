
using Microsoft.EntityFrameworkCore.Storage;
using TripLink.Core.Interfaces.Persistence;

namespace TripLink.Data.Persistence
{
    public class DbTransactionAdapter : IAppTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public DbTransactionAdapter(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public async Task CommitAsync(
            CancellationToken cancellationToken)
        {
            await _transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackAsync(
            CancellationToken cancellationToken)
        {
            await _transaction.RollbackAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _transaction.DisposeAsync();
        }
    }
}
