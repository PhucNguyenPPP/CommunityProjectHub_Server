using AutoMapper;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.Auth;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.Lecturer;
using CPH.Common.DTO.Lesson;
using CPH.Common.DTO.LessonClass;
using CPH.Common.DTO.Material;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Message;
using CPH.Common.DTO.Notification;
using CPH.Common.DTO.Project;
using CPH.DAL.Entities;

namespace CPH.Api.Profiles
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<SignUpRequestDTO, Account>().ReverseMap();
            CreateMap<Account, LocalAccountDTO>()
               .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
               .ReverseMap();
            CreateMap<ImportAccountDTO, Account>().ReverseMap();
            CreateMap<ImportTraineeDTO, Trainee>().ReverseMap();
            CreateMap<Message, MessageResponseDTO>()
                .ForMember(dest => dest.SendAccountId, opt => opt.MapFrom(src => src.AccountId))
                .ReverseMap();
            CreateMap<Message, MessageDTO>().ReverseMap();
            CreateMap<Notification, NotificationResponseDTO>().ReverseMap();
            CreateMap<Project, GetAllProjectDTO>()
               .ForMember(dest => dest.ProjectManagerName, opt => opt.MapFrom(src => src.ProjectManager.FullName))
               .ReverseMap();

            CreateMap<Class, GetAllClassDTO>()
                .ReverseMap();

            CreateMap<GetAllLessonDTO, Lesson>()
                .ForMember(dest => dest.LessonClasses, opt => opt.MapFrom(src => src.LessonClasses))
                .ReverseMap();

            CreateMap<GetAllLessonClassDTO, LessonClass>().ReverseMap();
            CreateMap<NewProjectDTO, Project>().ReverseMap();
            CreateMap<Project, ProjectDetailDTO>()
                .ForMember(dest => dest.ProjectManagerName, opt => opt.MapFrom(src => src.ProjectManager.FullName))
                .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons))
                .ForMember(dest => dest.Classes, opt => opt.MapFrom(src => src.Classes))
                .ForMember(dest => dest.TotalNumberTrainee, opt => opt.MapFrom(src => src.Classes.Sum(c => c.Trainees.Count())))
                .ForMember(dest => dest.TotalNumberLecturer, opt => opt.MapFrom(src => src.Classes.Count(c=> c.Lecturer != null)))
                .ReverseMap();
            CreateMap<Account, AccountResponseDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ReverseMap();
            CreateMap<ImportTraineeDTO, Account>().ReverseMap();
              //  .ReverseMap();
            CreateMap<UpdateProjectDTO, Project>().ReverseMap(); // Ignore Status, as it's usually handled separately.
            CreateMap<LessonClass, GetAllLessonClassByClassDTO>()
                 .ForMember(dest => dest.LessonNo, opt => opt.MapFrom(src => src.Lesson.LessonNo))
                 .ForMember(dest => dest.LessonContent, opt => opt.MapFrom(src => src.Lesson.LessonContent))
                 .ReverseMap();

            CreateMap<Class, GetAllClassOfProjectDTO>()
                .ForMember(dest => dest.LecturerName, opt => opt.MapFrom(src => src.Lecturer.FullName))
                .ForMember(dest => dest.LecturerPhone, opt => opt.MapFrom(src => src.Lecturer.Phone))
                .ForMember(dest => dest.TotalTrainee, opt => opt.MapFrom(src => src.Trainees.Count()))
                .ReverseMap();

            CreateMap<Account, LecturerResponseDTO>().ReverseMap();
            CreateMap<Material, GetAllMaterialDTO>().ReverseMap();

            CreateMap<Account, GetMemberOfClassDTO>()
                .ReverseMap();

            CreateMap<Class, ClassDetailDTO>()
                .ForMember(dest => dest.LecturerName, opt => opt.MapFrom(src => src.Lecturer.FullName))
                .ForMember(dest => dest.LecturerPhone, opt => opt.MapFrom(src => src.Lecturer.Phone))
                .ForMember(dest => dest.TotalTrainee, opt => opt.MapFrom(src => src.Trainees.Count()))
                .ReverseMap();


        }
    }
}
