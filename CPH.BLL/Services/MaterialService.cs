﻿using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Material;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Project;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Firebase.Storage;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly FirebaseApp _firebaseApp;
        private readonly string _firebaseBucket;
        private readonly IProjectService _projectService;

        public MaterialService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration config,
            IProjectService projectService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _config = config;
            _projectService = projectService;
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

        public async Task<ResponseDTO> CreateMaterial(MaterialCreateRequestDTO model)
        {
            var check = await _projectService.CheckProjectExisted(model.ProjectId);
            if (!check.IsSuccess)
            {
                return check;
            }

            if (model.File == null || model.File.Length == 0)
            {
                return new ResponseDTO("File không hợp lệ!", 400, false);
            }

            var url = await StoreFileAndGetLink(model.File, "cph_learning_material");
            if (url.IsNullOrEmpty())
            {
                return new ResponseDTO("Lưu tài liệu không thành công", 400, false);
            }

            var newMaterial = new Material
            {
                MaterialId = Guid.NewGuid(),
                MaterialUrl = url,
                ProjectId = model.ProjectId,
                Title = model.Title,
                UploadedAt = DateTime.Now,

            };

            await _unitOfWork.Material.AddAsync(newMaterial);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Lưu tài liệu thành công", 201, true);
            }
            return new ResponseDTO("Lưu tài liệu không thành công", 400, false);
        }

        public async Task<string> StoreFileAndGetLink(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var firebaseStorage = new FirebaseStorage(
                    _firebaseBucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(string.Empty),
                        ThrowOnCancel = true
                    });

                var storageUrl = await firebaseStorage
                    .Child(folderName)
                    .Child(fileName)
                    .PutAsync(memoryStream);

                return storageUrl;
            }
        }

        public async Task<ResponseDTO> GetAllMaterialProject(Guid projectId, string? searchValue, int? pageNumber, int? rowsPerPage)
        {
            var list = _unitOfWork.Material.GetAllByCondition(c => c.ProjectId == projectId);

            if (!list.Any())
            {
                return new ResponseDTO("Dự án chưa có tài nguyên", 400, false);
            }

            if (!searchValue.IsNullOrEmpty())
            {
                list = list.Where(c =>
                   c.Title.ToLower().Contains(searchValue.ToLower())
                );
            }

            if (!list.Any())
            {
                return new ResponseDTO("Không tìm thấy tài nguyên hợp lệ", 400, false);
            }

            if (pageNumber == null && rowsPerPage != null)
            {
                return new ResponseDTO("Vui lòng chọn số trang", 400, false);
            }
            if (pageNumber != null && rowsPerPage == null)
            {
                return new ResponseDTO("Vui lòng chọn số dòng mỗi trang", 400, false);
            }
            if (pageNumber <= 0 || rowsPerPage <= 0)
            {
                return new ResponseDTO("Giá trị phân trang không hợp lệ", 400, false);
            }

            var listDTO = _mapper.Map<List<GetAllMaterialDTO>>(list);

            if (pageNumber != null && rowsPerPage != null)
            {
                var pagedList = PagedList<GetAllMaterialDTO>.ToPagedList(listDTO.AsQueryable(), pageNumber, rowsPerPage);
                var result = new ListMaterialDTO
                {
                    GetAllMaterialDTOs = pagedList,
                    CurrentPage = pageNumber,
                    RowsPerPages = rowsPerPage,
                    TotalCount = listDTO.Count,
                    TotalPages = (int)Math.Ceiling(listDTO.Count / (double)rowsPerPage)
                };
                return new ResponseDTO("Tìm kiếm tài nguyên thành công", 200, true, result);
            }
            return new ResponseDTO("Lấy tài nguyên của dự án thành công", 200, true, listDTO);
        }
    }
}
