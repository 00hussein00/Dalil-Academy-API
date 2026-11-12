using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Repository;
using DalilAcadime.InfraStructures.Data;
using Microsoft.EntityFrameworkCore;
namespace DalilAcadime.InfraStructures.Repository
{
    public class UsersRepository(AppDbContext db) : IUsersRepository
    {
        public async Task<Users?> GetUserByEmail(string email)
        {
            return  await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
