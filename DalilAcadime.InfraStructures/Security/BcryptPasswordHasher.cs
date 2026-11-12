using DalilAcadime.Core.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.InfraStructures.Security
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
