using DalilAcadime.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Repository
{
    public interface IUsersRepository
    {
        Task<Users?> GetUserByEmail(string email);

    }
}
