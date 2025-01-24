using AutoMapper;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.Auth;
using CPH.Common.DTO.Message;
using CPH.Common.DTO.Notification;
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
            CreateMap<Message, MessageResponseDTO>()
                .ForMember(dest => dest.SendAccountId, opt => opt.MapFrom(src => src.AccountId))
                .ReverseMap();
            CreateMap<Message, MessageDTO>().ReverseMap();
            CreateMap<Notification, NotificationResponseDTO>().ReverseMap();
        }
    }
}
