using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Lesson;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.IdentityModel.Tokens;

namespace CPH.BLL.Services
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;


        public LessonService(IUnitOfWork unitOfWork, IMapper mapper, IProjectService projectService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _projectService = projectService;
        }

        public async Task<ResponseDTO> UpdateLessonOfProject(UpdateLessonOfProjectDTO lessonOfProjectDTO)
        {

            var check = await CheckUpdateProject(lessonOfProjectDTO);
            if (!check.IsSuccess)
            {
                return check;
            }
            var list = GetAllLessonOfProject(lessonOfProjectDTO.ProjectId);
            if (list.Any())
            {
                for (int i = 0; i < list.Count; i++)
                {
                    {
                        var spt = list[i].LessonNo - 1;
                        if (lessonOfProjectDTO.LessonOfProject.Count >= spt + 1)
                        {
                            if (!list[i].Equals(lessonOfProjectDTO.LessonOfProject[spt]))
                            {

                                list[i].LessonContent = lessonOfProjectDTO.LessonOfProject[spt];
                                _unitOfWork.Lesson.Update(list[i]);
                            }
                        }
                        else
                        {
                            var lscr = _unitOfWork.LessonClass.GetAllByCondition(ls => ls.LessonId.Equals(list[i].LessonId));
                            foreach (var lc in lscr)
                            {
                                _unitOfWork.LessonClass.Delete(lc);
                            }
                            _unitOfWork.Lesson.Delete(list[i]);
                        }
                    }

                }
                if (list.Count < lessonOfProjectDTO.LessonOfProject.Count)
                {
                    var listToadd = lessonOfProjectDTO.LessonOfProject.Skip(list.Count).ToList();
                    int temp = list.Count;
                    for (int i = 0; i < listToadd.Count; i++)
                    {
                        var nId = Guid.NewGuid();
                        Lesson newLesson = new Lesson()
                        {
                            LessonId = nId,
                            LessonNo = temp + 1,
                            LessonContent = listToadd[i],
                            ProjectId = lessonOfProjectDTO.ProjectId
                        };
                        await _unitOfWork.Lesson.AddAsync(newLesson);
                        temp++;
                        var cl = _unitOfWork.Class.GetAllByCondition(c => c.ProjectId.Equals(lessonOfProjectDTO.ProjectId));
                        foreach (var lc in cl)
                        {
                            var c = new LessonClass()
                            {
                                LessonClassId = Guid.NewGuid(),
                                ClassId = lc.ClassId,
                                LessonId = nId
                            };
                            await _unitOfWork.LessonClass.AddAsync(c);
                        }
                    }
                }
                
            }
            var updated = await _unitOfWork.SaveChangeAsync();
            if (updated)
            {
                return new ResponseDTO("Cập nhật bài học của dự án thành công", 200, true);
            }


            return new ResponseDTO("Cập nhật không thành công", 500, false);
        }

        private List<Lesson> GetAllLessonOfProject(Guid projectId)
        {
            return _unitOfWork.Lesson.GetAllByCondition(l => l.ProjectId == projectId).ToList();
        }

        private async Task<ResponseDTO> CheckUpdateProject(UpdateLessonOfProjectDTO lessonOfProjectDTO)
        {
            var project = await _projectService.CheckProjectExisted(lessonOfProjectDTO.ProjectId);
            if (!project.IsSuccess)
            {
                return new ResponseDTO("Dự án không tồn tại", 404, false);
            }
            for (int i = 0; i < lessonOfProjectDTO.LessonOfProject.Count; i++)
            {
                for (int j = i + 1; j < lessonOfProjectDTO.LessonOfProject.Count; j++)
                {
                    if (lessonOfProjectDTO.LessonOfProject.ElementAt(i).Equals(lessonOfProjectDTO.LessonOfProject.ElementAt(j)))
                    {
                        return new ResponseDTO("Có 2 bài học có trùng nội dung là: " + lessonOfProjectDTO.LessonOfProject.ElementAt(i).ToString(), 400, false);
                    }
                }
            }
            var project2 = (Project)project.Result;
            if (project2.StartDate < DateTime.Now)
            {
                return new ResponseDTO("Không thể thay đổi bài học của dự án đã bắt đầu", 400, false);
            }
            return new ResponseDTO("Bài học hợp lệ", 200, true);
        }
    }
}
