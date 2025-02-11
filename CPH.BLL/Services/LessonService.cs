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
            var project = await _projectService.CheckProjectExisted(lessonOfProjectDTO.ProjectId);
            if (!project.IsSuccess)
            {
                return new ResponseDTO("Dự án không tồn tại", 404, false);
            }
            var check = await CheckUpdateProject(lessonOfProjectDTO.LessonOfProject);
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
                        var lessonNo = list[i].LessonNo;
                        if (lessonOfProjectDTO.LessonOfProject[lessonNo - 1] != null)
                        {
                            if (!list[i].Equals(lessonOfProjectDTO.LessonOfProject[lessonNo - 1]))
                            {

                                list[i].LessonContent = lessonOfProjectDTO.LessonOfProject[lessonNo - 1];
                                _unitOfWork.Lesson.Update(list[i]);
                            }
                        }
                        else
                        {
                            var lscr = _unitOfWork.LessonClass.GetAllByCondition(ls => ls.LessonId.Equals(list[i].LessonId));
                            foreach(var lc in lscr)
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
                    for(int i=0;i<listToadd.Count;i++)
                    {
                        var nId = Guid.NewGuid();
                        Lesson newLesson = new Lesson()
                        {
                            LessonId = nId,
                            LessonNo = temp+1,
                            LessonContent = listToadd[i]
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
            return new ResponseDTO("Bài học hợp lệ", 200, true);
        }

        private List<Lesson> GetAllLessonOfProject(Guid projectId)
        {
            return _unitOfWork.Lesson.GetAllByCondition(l => l.ProjectId == projectId).ToList();
        }

        private async Task<ResponseDTO> CheckUpdateProject(List<string> lessonOfProject)
        {
            for (int i = 0; i < lessonOfProject.Count; i++)
            {
                for (int j = i + 1; j < lessonOfProject.Count; j++)
                {
                    if (lessonOfProject.ElementAt(i).Equals(lessonOfProject.ElementAt(j)))
                    {
                        return new ResponseDTO("Có 2 bài học có trùng nội dung là: " + lessonOfProject.ElementAt(i).ToString(), 400, false);
                    }
                }
            }
            return new ResponseDTO("Bài học hợp lệ", 200, true);
        }
    }
}
