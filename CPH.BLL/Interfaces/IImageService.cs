using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface IImageService
    {
        Task<string> StoreImageAndGetLink(IFormFile image, string folderName);

        Task DeleteFileFromFirebase(string fileUrl);
    }
}
