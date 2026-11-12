using DalilAcadime.Core.Exceptions;
using DalilAcadime.Core.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.InfraStructures.Storage
{
    public class ImgService : IImageService
    {
        private readonly string _imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img","courses");
        private readonly string[] allowed = [".jpg", ".jpeg", ".png", ".gif",".webp"];

        public async Task<string> saveCourseImage(Stream img, string filename, long lenght)
        {
            if(lenght == 0 || lenght >2 *1024 * 1024 )
                throw new InvalidFileFormatException("File size must be less than 2 MB.");

            var ex = Path.GetExtension(filename).ToLower();
            if (!allowed.Contains(ex))
                throw new InvalidFileFormatException("Invalid file format. Only .jpg, .jpeg, .png, .gif and .webp are allowed."); 

            Directory.CreateDirectory(_imagePath);
            var name = $"{Guid.NewGuid()}{ex}";
            await using var file = File.Create(Path.Combine(_imagePath, name));
            await img.CopyToAsync(file);
            
            return "/img/courses/" + name;
        }

        public void deleteCourseImage(string imgUrl)
        {
            if (string.IsNullOrEmpty(imgUrl))
                return ;
            var filePath = Path.Combine(_imagePath, Path.GetFileName(imgUrl));
           
            if (File.Exists(filePath))
                File.Delete(filePath);
            
        }
    }
}
