using CPH.BLL.Interfaces;
using Firebase.Storage;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class ImageService : IImageService
    {
        private readonly IConfiguration _config;
        private readonly FirebaseApp _firebaseApp;
        private readonly string _firebaseBucket;

        public ImageService(IConfiguration config)
        {
            _config = config;
            _firebaseBucket = _config.GetSection("FirebaseConfig")["storage_bucket"];

            if (FirebaseApp.DefaultInstance == null)
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("firebase.json"),
                    ProjectId = _config.GetSection("FirebaseConfig")["project_id"],
                });
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
        }

        public async Task<string> StoreImageAndGetLink(IFormFile image, string folderName)
        {
            if (image == null || image.Length == 0)
            {
                return string.Empty;
            }

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var firebaseStorage = new FirebaseStorage(
                    _firebaseBucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(string.Empty)
                    });

                var storageUrl = await firebaseStorage
                    .Child(folderName)
                    .Child(fileName)
                    .PutAsync(memoryStream);

                return storageUrl;
            }
        }
    }
}
