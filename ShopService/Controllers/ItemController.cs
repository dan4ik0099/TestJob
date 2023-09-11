using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopService.MiniService;
using TestJob.Domain.Context;
using TestJob.Domain.Entity;
using TestJob.Domain.Request;
using TestJob.Domain.Response;

namespace ShopService.Controllers;

[ApiController]
[Route("shop")]
public class ItemController : ControllerBase
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly EmailSender _emailSender;
    private readonly IMapper _autoMapper;
    public ItemController(ApplicationDbContext applicationDbContext, EmailSender emailSender, IMapper autoMapper)
    {
        _applicationDbContext = applicationDbContext;
        _emailSender = emailSender;
        _autoMapper = autoMapper;
    }

    [HttpGet]
    [Route("items")]
    
    public async Task<IActionResult> GetAllItems()
    {
        var items = await _applicationDbContext.Items.ToListAsync();
        var resultItems = _autoMapper.Map<List<ItemCompactResponse>>(items);

        return Ok(resultItems);
    }

    [HttpGet]
    [Route("get-item-by-id")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> GetItemById([FromQuery] Guid id)
    {
        var item = await _applicationDbContext.Items.FirstOrDefaultAsync(x=>x.Id==id);
        if (item != null)
        {
            return Ok(item);
        }

        return NotFound();
    }

    [HttpPost]
    [Route("add-to-cart")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> AddToCart([FromBody] ItemIdCountRequest x)
    {
        if (x.Count > 0)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest();
            }
            var user = await _applicationDbContext.Users.FirstOrDefaultAsync(user=>user.Id == userId);
            if (user == null) {return BadRequest();}
            var item = await _applicationDbContext.Items.FirstOrDefaultAsync(item=>item.Id==x.Id);
            if (item == null) {return BadRequest();}
            var cartUnit = new CartUnit
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ItemId = item.Id,
                Cost = item.Cost,
                Count = x.Count,
                TotalCost = item.Cost* x.Count
            };
            _applicationDbContext.CartUnits.Add(cartUnit);
            await _applicationDbContext.SaveChangesAsync();
            return Ok(cartUnit);
        }

        return BadRequest();
    }

    [HttpDelete]
    [Route("delete-from-cart")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> DeleteFromCart([FromBody] Guid idCartUnit)
    {
        var cartUnit = await _applicationDbContext.CartUnits.FirstOrDefaultAsync(unit => unit.Id == idCartUnit);
        if (cartUnit==null) {return NotFound();}
        _applicationDbContext.CartUnits.Remove(cartUnit);
        await _applicationDbContext.SaveChangesAsync();
        return Ok();
    }
    [HttpPost]
    [Route("update-count-cart")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> UpdateCountCart([FromBody] ItemIdCountRequest x)
    {
        if (x.Count <= 0) {return BadRequest();}
        var cartUnit = await _applicationDbContext.CartUnits.FirstOrDefaultAsync(unit => unit.Id == x.Id);
        if (cartUnit == null) {return NotFound();}
        cartUnit.Count = x.Count;
        cartUnit.TotalCost = x.Count * cartUnit.Cost;
        _applicationDbContext.CartUnits.Update(cartUnit); 
        await _applicationDbContext.SaveChangesAsync();
        return Ok(cartUnit);

    }
    [HttpGet]
    [Route("create-order")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> CreateOrder()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest();
        }
        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var cartUnits = await _applicationDbContext.CartUnits.Where(x => x.UserId == userId)
            .Include(cartUnit => cartUnit.Item).ToListAsync();
        if (cartUnits.IsNullOrEmpty()) {return NotFound();}

        decimal totalCost = 0;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Date = DateTime.Now,
            TotalCost = 0
        };
        var orderUnits = new List<OrderUnit>();
        foreach (var x in cartUnits)
        {
            var orderUnit = new OrderUnit
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ItemId = x.Item.Id,
                Count = x.Count,
                Cost = x.Cost,
                TotalCost = x.TotalCost
            };
            _applicationDbContext.CartUnits.Remove(x);
            totalCost += x.TotalCost;
            orderUnits.Add(orderUnit);
            
        }

        order.TotalCost = totalCost;
        await _applicationDbContext.Orders.AddAsync(order);
        var infoOrder = $"{order.User.UserName} Ваш заказ {order.Id} на сумму {Math.Round(order.TotalCost, 2)} сформирован {order.Date}";
        await _emailSender.SendEmailAsync(user.Email, "gg", infoOrder);
        await _applicationDbContext.OrderUnits.AddRangeAsync(orderUnits);
        await _applicationDbContext.SaveChangesAsync();
        return Ok(order);

    }
    
}