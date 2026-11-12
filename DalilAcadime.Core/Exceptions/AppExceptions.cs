using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Exceptions
{
    public class ConflictException(string msg) : Exception(msg);
    public class NotFoundException(string msg) : Exception(msg);  
    public class UnauthorizedException(string msg) : Exception(msg);
    public class ForbiddenException(string msg) : Exception(msg);
    public class InvalidFileFormatException(string msg) : Exception(msg);
}
