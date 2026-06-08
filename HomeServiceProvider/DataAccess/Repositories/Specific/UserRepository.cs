using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.DataAccess.Entities;
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
    }
}
