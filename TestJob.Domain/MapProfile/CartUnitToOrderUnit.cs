using AutoMapper;
using TestJob.Domain.Entity;

namespace TestJob.Domain.MapProfile;

public class CartUnitToOrderUnit : Profile
{
    public CartUnitToOrderUnit()
    {
        CreateMap<CartUnit, OrderUnit>()
            .ForMember(x => x.Order, act => act.Ignore())
            .ForMember(x => x.Id, act => act.Ignore())
            .ForMember(x => x.OrderId, act => act.Ignore());

    }
}