using Auth.Domain.Users;
using AutoMapper;

namespace Auth.API.Features.Users.CreateAccount;

public class CreateUserCommandMapping : Profile
{
    public CreateUserCommandMapping()
    {
        //insight - parent child relation & inheritance
        CreateMap<UserRoleDto, UserRole>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(s => s.RoleType));
        CreateMap<CreateUserCommand, User>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.Email));
    }
}
