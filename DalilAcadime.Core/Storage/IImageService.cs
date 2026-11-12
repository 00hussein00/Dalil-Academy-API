using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Storage
{
    public interface IImageService
    {
        Task<string> saveCourseImage(Stream img, string filename,long lenght);
        void deleteCourseImage(string imgUrl);

    }
}
