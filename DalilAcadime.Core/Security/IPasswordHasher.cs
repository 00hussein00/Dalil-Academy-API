using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Security
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
