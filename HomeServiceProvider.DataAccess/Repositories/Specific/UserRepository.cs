using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.DataAccess.Repositories.Generics;
using Microsoft.EntityFrameworkCore;

namespace HomeServiceProvider.DataAccess.Repositories.Specific
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
            => await _dbSet.FirstOrDefaultAsync(u => u.Email == email.ToLower());

        public async Task<bool> EmailExistsAsync(string email)
            => await _dbSet.AnyAsync(u => u.Email == email.ToLower());

        public async Task<User?> GetByVerificationTokenAsync(string token)
            => await _dbSet.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

        public async Task<User?> GetByPasswordResetTokenAsync(string token)
            => await _dbSet.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == token &&
                u.PasswordResetTokenExpiry > DateTime.UtcNow);

        // ADD this method inside the existing UserRepository class
        public async Task<IEnumerable<User>> SearchUsersAsync(
            UserRole? role, bool? isActive, string? search)
        {
            var query = _dbSet
                .Include(u => u.CustomerProfile)
                .Include(u => u.ProviderProfile)
                .AsQueryable();

            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    u.PhoneNumber.Contains(term));
            }

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }
    }
}
