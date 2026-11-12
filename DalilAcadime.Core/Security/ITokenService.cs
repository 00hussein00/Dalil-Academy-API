using DalilAcadime.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Security
{
    public interface ITokenService
    {
        (string token, DateTime expiration) GenerateToken(Users user);
        (string token, DateTime expiration) GenerateToken(Student student);
    }
}
