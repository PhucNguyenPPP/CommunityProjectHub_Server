using AutoMapper;
using CPH.Common.DTO.Auth;
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
            CreateMap<Project, GetAllProjectDTO>()
                .ForMember(dest => dest.ProjectManagerName, opt => opt.MapFrom(src => src.ProjectManager.FullName))
                .ReverseMap();
        }
    }
}
