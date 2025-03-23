using AutoMapper;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.Associate;
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
using CPH.Common.DTO.ProjectLogging;
using CPH.Common.DTO.Registration;
using CPH.Common.DTO.Trainee;
using CPH.DAL.Entities;

namespace CPH.Api.Profiles
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<SignUpRequestDTO, Account>().ReverseMap();
            CreateMap<SignUpRequestDTO2, Account>().ReverseMap();
            CreateMap<Account, LocalAccountDTO>()
               .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
               .ReverseMap();
            CreateMap<ImportAccountDTO, Account>().ReverseMap();
            CreateMap<ImportTraineeDTO, Trainee>().ReverseMap();
            CreateMap<Message, MessageResponseDTO>()
                .ForMember(dest => dest.SendAccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.SendAccountName, opt => opt.MapFrom(src => src.Account!.AccountName))
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
                .ForMember(dest => dest.LecturerEmail, opt => opt.MapFrom(src => src.Lecturer.Email))
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
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project.Title))
                .ForMember(dest => dest.ProjectManagerId, opt => opt.MapFrom(src => src.Project.ProjectManagerId))
                .ForMember(dest => dest.ProjectStatus, opt => opt.MapFrom(src => src.Project.Status))
                .ReverseMap();
            CreateMap<Registration, SendRegistrationDTO>()
               .ReverseMap();

            
            CreateMap<Member, MemberProjectDTO>()
                .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src.Account))
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class.ClassCode))
                .ReverseMap();

            CreateMap<Class, LecturerProjectDTO>()
                .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src.Lecturer))
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.ClassCode))
                .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.ClassId));

            CreateMap<Registration, SentRegistDTO>().
                ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class.ClassCode)).
                ForMember(dest => dest.ClassId, opt =>opt.MapFrom(src=>src.Class.ClassId)). 
                ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Class.ProjectId)).ReverseMap();
            CreateMap<Registration, RegistrationsOfProjectDTO>()
           .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class.ClassCode)) // Access ClassCode from the related Class
           .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Class.Project.ProjectId)) // Access ProjectId from the related Class and Project
           .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Class.Project.Title)) // Access Title from the related Class and Project
           .ForMember(dest => dest.AccountCode, opt => opt.MapFrom(src => src.Account.AccountCode)) // Access AccountCode from the related Account
           .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Account.FullName)) // Access FullName from the related Account
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email)) // Access Email from the related Account
           .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Account.Gender)) // Access Gender from the related Account
           .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Account.DateOfBirth)) // Access DateOfBirth from the related Account
           .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Account.RoleId)) // Access RoleId from the related Account
           .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Account.Role.RoleName));

            CreateMap<Trainee, GetAllTraineeOfClassDTO>()
                .ReverseMap();

            CreateMap<ProjectLogging, GetAllProjectLoggingDTO>()
                .ReverseMap();
            CreateMap<Account, MemberResponseDTO>().ReverseMap();

            CreateMap<Class, GetAllClassOfLecturer>()
               .ForMember(dest => dest.LecturerName, opt => opt.MapFrom(src => src.Lecturer.FullName))
               .ForMember(dest => dest.LecturerPhone, opt => opt.MapFrom(src => src.Lecturer.Phone))
               .ForMember(dest => dest.LecturerEmail, opt => opt.MapFrom(src => src.Lecturer.Email))
               .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project.Title))
               .ForMember(dest => dest.ProjectStartDate , opt => opt.MapFrom(src => src.Project.StartDate))
               .ForMember(dest => dest.ProjectEndDate, opt => opt.MapFrom(src => src.Project.EndDate))
               .ForMember(dest => dest.ProjectAddress, opt => opt.MapFrom(src => src.Project.Address))
               .ForMember(dest => dest.ProjectNumberLesson, opt => opt.MapFrom(src => src.Project.NumberLesson))
               .ForMember(dest => dest.ProjectApplicationStartDate, opt => opt.MapFrom(src => src.Project.ApplicationStartDate))
               .ForMember(dest => dest.ProjectApplicationEndDate, opt => opt.MapFrom(src => src.Project.ApplicationEndDate))
               .ForMember(dest => dest.ProjectApplicationEndDate, opt => opt.MapFrom(src => src.Project.ApplicationEndDate))
               .ForMember(dest => dest.ProjectCreatedDate, opt => opt.MapFrom(src => src.Project.CreatedDate))
               .ForMember(dest => dest.ProjectStatus, opt => opt.MapFrom(src => src.Project.Status))
               .ForMember(dest => dest.ProjectManagerId, opt => opt.MapFrom(src => src.Project.ProjectManagerId))
               .ReverseMap();

            CreateMap<Class, GetAllClassOfTrainee>()
               .ForMember(dest => dest.LecturerName, opt => opt.MapFrom(src => src.Lecturer.FullName))
               .ForMember(dest => dest.LecturerPhone, opt => opt.MapFrom(src => src.Lecturer.Phone))
               .ForMember(dest => dest.LecturerEmail, opt => opt.MapFrom(src => src.Lecturer.Email))
               .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project.Title))
               .ForMember(dest => dest.ProjectStartDate, opt => opt.MapFrom(src => src.Project.StartDate))
               .ForMember(dest => dest.ProjectEndDate, opt => opt.MapFrom(src => src.Project.EndDate))
               .ForMember(dest => dest.ProjectAddress, opt => opt.MapFrom(src => src.Project.Address))
               .ForMember(dest => dest.ProjectNumberLesson, opt => opt.MapFrom(src => src.Project.NumberLesson))
               .ForMember(dest => dest.ProjectApplicationStartDate, opt => opt.MapFrom(src => src.Project.ApplicationStartDate))
               .ForMember(dest => dest.ProjectApplicationEndDate, opt => opt.MapFrom(src => src.Project.ApplicationEndDate))
               .ForMember(dest => dest.ProjectApplicationEndDate, opt => opt.MapFrom(src => src.Project.ApplicationEndDate))
               .ForMember(dest => dest.ProjectCreatedDate, opt => opt.MapFrom(src => src.Project.CreatedDate))
               .ForMember(dest => dest.ProjectStatus, opt => opt.MapFrom(src => src.Project.Status))
               .ForMember(dest => dest.ProjectManagerId, opt => opt.MapFrom(src => src.Project.ProjectManagerId))
               .ReverseMap();

            CreateMap<Class, GetAllClassOfStudent>()
               .ForMember(dest => dest.LecturerName, opt => opt.MapFrom(src => src.Lecturer.FullName))
               .ForMember(dest => dest.LecturerPhone, opt => opt.MapFrom(src => src.Lecturer.Phone))
               .ForMember(dest => dest.LecturerEmail, opt => opt.MapFrom(src => src.Lecturer.Email))
               .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project.Title))
               .ForMember(dest => dest.ProjectStartDate, opt => opt.MapFrom(src => src.Project.StartDate))
               .ForMember(dest => dest.ProjectEndDate, opt => opt.MapFrom(src => src.Project.EndDate))
               .ForMember(dest => dest.ProjectAddress, opt => opt.MapFrom(src => src.Project.Address))
               .ForMember(dest => dest.ProjectNumberLesson, opt => opt.MapFrom(src => src.Project.NumberLesson))
               .ForMember(dest => dest.ProjectApplicationStartDate, opt => opt.MapFrom(src => src.Project.ApplicationStartDate))
               .ForMember(dest => dest.ProjectApplicationEndDate, opt => opt.MapFrom(src => src.Project.ApplicationEndDate))
               .ForMember(dest => dest.ProjectApplicationEndDate, opt => opt.MapFrom(src => src.Project.ApplicationEndDate))
               .ForMember(dest => dest.ProjectCreatedDate, opt => opt.MapFrom(src => src.Project.CreatedDate))
               .ForMember(dest => dest.ProjectStatus, opt => opt.MapFrom(src => src.Project.Status))
               .ForMember(dest => dest.ProjectManagerId, opt => opt.MapFrom(src => src.Project.ProjectManagerId))
               .ReverseMap();
            CreateMap<SignUpRequestOfTraineeDTO, Account>().ReverseMap();

            CreateMap<Account, GetTraineeOfClassDTO>()
                .ReverseMap();
            CreateMap<Class, ClassAvailableDTO>().ReverseMap();
            CreateMap<LessonClass, LessonClassOfClassAvailableDTO>().ReverseMap();

            CreateMap<LessonClass, GetAllLessonClassForScheduleDTO>()
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class.ClassCode))
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Lesson.LessonContent))
                .ForMember(dest => dest.LessonNo, opt => opt.MapFrom(src => src.Lesson.LessonNo))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Class.Project.Title))
                .ReverseMap();

            CreateMap<Account, AssociateResponseDTO>()
                .ForMember(dest => dest.AssociateName, opt => opt.MapFrom(src => src.Associate!.AssociateName))
                .ReverseMap();
        }
    }
}
