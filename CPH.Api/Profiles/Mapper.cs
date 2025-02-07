using AutoMapper;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.Auth;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.Lesson;
using CPH.Common.DTO.LessonClass;
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

            CreateMap<GetAllClassDTO, Class>()
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
                .ReverseMap();
            CreateMap<Account, AccountResponseDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ReverseMap();
            CreateMap<UpdateProjectDTO, Project>()
            .ForMember(dest => dest.NumberLesson, opt => opt.MapFrom(src => src.LessonList.Count)) // Map NumberLesson from LessonList count
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore()) // Ignore ProjectId in update, as it's usually handled separately
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Ignore CreatedDate, as it's usually set on creation
            .ForMember(dest => dest.Status, opt => opt.Ignore()); // Ignore Status, as it's usually handled separately


        }
    }
}
