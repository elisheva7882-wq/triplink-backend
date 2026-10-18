
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TripLink.Core.Exceptions;
using TripLink.Core.Interfaces.Persistence;
using TripLink.Data.Context;

namespace TripLink.Data.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TripLinkDbContext _context;

        public UnitOfWork(TripLinkDbContext context)
        {
            _context = context;
        }

        public async Task<IAppTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken)
        {
            var transaction =
                await _context.Database.BeginTransactionAsync(
                    cancellationToken);

            return new DbTransactionAdapter(transaction);
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                return await _context.SaveChangesAsync(
                    cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new DomainConflictException(
                    "The data was changed by another user. Please try again.");
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException pg &&
                      pg.SqlState == PostgresErrorCodes.UniqueViolation &&
                      pg.ConstraintName == "PK_RideRegistrations")
            {
                throw new DomainConflictException(
                    "You are already registered for this ride.");
            }
        }
    }
}
