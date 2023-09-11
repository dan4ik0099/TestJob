using AutoMapper;
using TestJob.Domain.Entity;
using TestJob.Domain.Response;

namespace TestJob.Domain.MapProfile;

public class InfoUserProfile : Profile
{
    public InfoUserProfile()
    {
        CreateMap<User, InfoUserResponse>();

    }
}