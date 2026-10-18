
using Microsoft.EntityFrameworkCore;
using TripLink.Core.DTOs.Rides;
using TripLink.Core.Entities;
using TripLink.Core.Interfaces.Repositories;
using TripLink.Data.Context;

namespace TripLink.Data.Repositories
{
    public class RideRepository : IRideRepository
    {
        private readonly TripLinkDbContext _context;

        public RideRepository(TripLinkDbContext context)
        {
            _context = context;
        }

        public async Task<Ride?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return await _context.Rides
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.Id == id,
                    cancellationToken);
        }

        public async Task<Ride?> GetByIdForUpdateAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return await _context.Rides
                .FirstOrDefaultAsync(
                    r => r.Id == id,
                    cancellationToken);
        }

        private IQueryable<Ride> ApplyFilters(
            RideSearchFilter filter)
        {
            IQueryable<Ride> query = _context.Rides
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.OriginCity))
            {
                query = query.Where(
                    r => r.OriginCity == filter.OriginCity);
            }

            if (!string.IsNullOrWhiteSpace(filter.DestinationCity))
            {
                query = query.Where(
                    r => r.DestinationCity == filter.DestinationCity);
            }

            if (filter.DepartureDate.HasValue)
            {
                DateTime start = DateTime.SpecifyKind(
                    filter.DepartureDate.Value.Date,
                    DateTimeKind.Utc);

                DateTime end = start.AddDays(1);

                query = query.Where(r =>
                    r.DepartureAtUtc >= start &&
                    r.DepartureAtUtc < end);
            }

            return query;
        }

        private static IQueryable<Ride> ApplySorting(
    IQueryable<Ride> query,
    RideSearchFilter filter)
        {
            string sortBy = filter.SortBy.ToLowerInvariant();

            return sortBy switch
            {
                "origin" => filter.SortDescending
                    ? query.OrderByDescending(r => r.OriginCity)
                           .ThenBy(r => r.Id)
                    : query.OrderBy(r => r.OriginCity)
                           .ThenBy(r => r.Id),

                "destination" => filter.SortDescending
                    ? query.OrderByDescending(r => r.DestinationCity)
                           .ThenBy(r => r.Id)
                    : query.OrderBy(r => r.DestinationCity)
                           .ThenBy(r => r.Id),

                "capacity" => filter.SortDescending
                    ? query.OrderByDescending(r => r.Capacity)
                           .ThenBy(r => r.Id)
                    : query.OrderBy(r => r.Capacity)
                           .ThenBy(r => r.Id),

                _ => filter.SortDescending
                    ? query.OrderByDescending(r => r.DepartureAtUtc)
                           .ThenBy(r => r.Id)
                    : query.OrderBy(r => r.DepartureAtUtc)
                           .ThenBy(r => r.Id)
            };
        }

        public async Task<List<Ride>> SearchAsync(
            RideSearchFilter filter,
            CancellationToken cancellationToken)
        {
            if (filter.Page < 1 ||
                filter.PageSize < 1 ||
                filter.PageSize > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(filter), "Invalid pagination values.");
            }

            var query = ApplyFilters(filter);

            query = ApplySorting(query, filter);

            return await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(
            RideSearchFilter filter,
            CancellationToken cancellationToken)
        {
            return await ApplyFilters(filter)
                .CountAsync(cancellationToken);
        }

        public async Task<List<Ride>> GetByOwnerIdAsync(
    int ownerId,
    CancellationToken cancellationToken)
        {
            return await _context.Rides
                .AsNoTracking()
                .Where(r => r.OwnerId == ownerId)
                .OrderByDescending(r => r.DepartureAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            Ride ride,
            CancellationToken cancellationToken)
        {
            await _context.Rides.AddAsync(
                ride,
                cancellationToken);
        }

        public void Remove(Ride ride)
        {
            _context.Rides.Remove(ride);
        }
    }
}
