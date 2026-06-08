using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Repositories.Generics;

namespace HomeServiceProvider.DataAccess.Repositories.Specific
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetByVerificationTokenAsync(string token);
        Task<User?> GetByPasswordResetTokenAsync(string token);
    }
}
