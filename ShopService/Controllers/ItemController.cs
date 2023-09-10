using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopService.MiniService;
using TestJob.Domain.Context;
using TestJob.Domain.Entity;
using TestJob.Domain.Response;

namespace ShopService.Controllers;

[ApiController]
[Route("shop")]
public class ItemController : ControllerBase
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly EmailSender _emailSender;
    private readonly IMapper _autoMapper;
    private readonly string fid = "08dbb1d6-7877-478c-86a7-c260897970e4";
    public ItemController(ApplicationDbContext applicationDbContext, EmailSender emailSender, IMapper autoMapper)
    {
        _applicationDbContext = applicationDbContext;
        _emailSender = emailSender;
        _autoMapper = autoMapper;
    }

    [HttpGet("items")]
    public async Task<IActionResult> GetAllItems()
    {
        var items = await _applicationDbContext.Items.ToListAsync();
        var resultItems = _autoMapper.Map<List<ItemCompactResponse>>(items);

        return Ok(resultItems);
    }

    [HttpGet("items/{id:guid}")]
    public async Task<IActionResult> GetItemById(Guid id)
    {
        var item = await _applicationDbContext.Items.FindAsync(id);
        if (item != null)
        {
            return Ok(item);
        }

        return NotFound();
    }

    [HttpGet("add-to-cart")]
    public async Task<IActionResult> AddToCart(Guid idItem, int count)
    {
        if (count > 0)
        {
            var userId = Guid.Parse(fid); //User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _applicationDbContext.Users.FindAsync(userId);
            if (user == null) {return BadRequest();}
            var item = await _applicationDbContext.Items.FindAsync(idItem);
            if (item == null) {return BadRequest();}
            var cartUnit = new CartUnit
            {
                Id = default,
                UserId = default,
                User = user,
                ItemId = default,
                Item = item,
                Cost = item.Cost,
                Count = count,
                TotalCost = item.Cost* count
            };
            _applicationDbContext.CartUnits.Add(cartUnit);
            await _applicationDbContext.SaveChangesAsync();
            return Ok(cartUnit);
        }

        return BadRequest();
    }

    [HttpDelete("delete-from-cart")]
    public async Task<IActionResult> DeleteFromCart(Guid idCartUnit)
    {
        var cartUnit = await _applicationDbContext.CartUnits.FindAsync(idCartUnit);
        if (cartUnit==null) {return NotFound();}
        _applicationDbContext.CartUnits.Remove(cartUnit);
        await _applicationDbContext.SaveChangesAsync();
        return Ok();
    }
    [HttpGet("update-count-cart")]
    public async Task<IActionResult> UpdateCountCart(Guid idCartUnit, int count)
    {
        if (count <= 0) {return BadRequest();}
        var cartUnit = await _applicationDbContext.CartUnits.FindAsync(idCartUnit);
        if (cartUnit == null) {return NotFound();}
        cartUnit.Count = count;
        cartUnit.TotalCost = count * cartUnit.Cost;
        _applicationDbContext.CartUnits.Update(cartUnit); 
        await _applicationDbContext.SaveChangesAsync();
        return Ok(cartUnit);

    }
    [HttpGet("create-order")]
    public async Task<IActionResult> CreateOrder()
    {
        var userId = Guid.Parse(fid);
        var user = await _applicationDbContext.Users.FindAsync(userId);
        var cartUnits = await _applicationDbContext.CartUnits.Where(x => x.UserId == userId)
            .Include(cartUnit => cartUnit.Item).ToListAsync();
        if (cartUnits.IsNullOrEmpty()) {return NotFound();}

        decimal totalCost = 0;
        var order = new Order
        {
            Id = default,
            UserId = default,
            User = user,
            OrderUnits = new List<OrderUnit>(),
            Date = default,
            TotalCost = 0
        };
        var orderUnits = new List<OrderUnit>();
        foreach (var x in cartUnits)
        {
            var orderUnit = new OrderUnit
            {
                Id = default,
                OrderId = default,
                Order = order,
                ItemId = default,
                Item = x.Item,
                Count = x.Count,
                Cost = x.Cost,
                TotalCost = x.TotalCost
            };
            totalCost += x.TotalCost;
            orderUnits.Add(orderUnit);
            
        }

        order.TotalCost = totalCost;
        order.OrderUnits.AddRange(orderUnits);
        await _applicationDbContext.Orders.AddAsync(order);
        await _applicationDbContext.OrderUnits.AddRangeAsync(orderUnits);
        await _applicationDbContext.SaveChangesAsync();
        return Ok(order);

    }
    
}