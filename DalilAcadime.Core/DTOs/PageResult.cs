using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.DTOs
{
    public record PageResult<T>(List<T> Data, int Page, int PageSize, int TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    }

}
