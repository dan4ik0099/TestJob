using AutoMapper;
using TestJob.Domain.Entity;
using TestJob.Domain.Response;

namespace TestJob.Domain.MapProfile;

public class ItemProfile : Profile
{
    public ItemProfile()
    {
        CreateMap<Item, ItemCompactResponse>();
    }
}