
using Microsoft.EntityFrameworkCore;
using TripLink.Core.Entities;
using TripLink.Core.Interfaces.Repositories;
using TripLink.Data.Context;

namespace TripLink.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TripLinkDbContext _context;

        public UserRepository(TripLinkDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.Id == id,
                    cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.Email == email,
                    cancellationToken);
        }

        public async Task<List<User>> GetAllAsync(
    CancellationToken cancellationToken)
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            User user,
            CancellationToken cancellationToken)
        {
            await _context.Users.AddAsync(
                user,
                cancellationToken);
        }
    }
}
