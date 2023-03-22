using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderAppWebApı.Context;
using OrderAppWebApı.Models.Dtos;
using OrderAppWebApı.Models.Entities;
using OrderAppWebApı.Models.Results;
using OrderAppWebApı.RabbitMq;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System.Text;

namespace OrderAppWebApı.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly OrderContextDb _context;
        private readonly IMapper _mapper;

        public OrdersController(IMemoryCache memoryCache, OrderContextDb context, IMapper mapper)
        {
            _memoryCache = memoryCache;
            _context = context;
            _mapper = mapper;
        }

        #region
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest createOrderRequest)
        {
            Order order = _mapper.Map<Order>(createOrderRequest);
            List<OrderDetail> orderDetails = _mapper.Map<List<ProductDetailDto>, List<OrderDetail>>(createOrderRequest.ProductDetails) as List<OrderDetail>;

            order.TotalAmount = createOrderRequest.ProductDetails.Sum(p => p.Amount);
            order.OrderDetails = orderDetails;
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            //Mail İşlemi

            var datasBody = Encoding.UTF8.GetBytes(createOrderRequest.CustomerEmail);

            SetQueues.SendQueue(datasBody);

            return Ok(new ApıResponse<int>(StatusType.Success, order.Id));
        }

        #endregion

        #region Redis Loglama

        [HttpGet]
        public async Task<IActionResult> Get(string? category)
        {
            var redisClient = new RedisClient("localhost", 6379);
            IRedisTypedClient<List<Product>> redisProducts = redisClient.As<List<Product>>();

            var result = new List<Product>();
            if (category == null)
            {
                result = redisClient.Get<List<Product>>("products");
                if (result is null)
                {
                    result = await _context.Products.ToListAsync();
                    redisClient.Set("products", result, TimeSpan.FromMinutes(10));
                }
            }
            else
            {
                result = redisClient.Get<List<Product>>($"products-{category}");
                if (result is null)
                {
                    result = await _context.Products.Where(p => p.Category == category).ToListAsync();
                    redisClient.Set($"products-{category}", result, TimeSpan.FromMinutes(10));
                }
            }

            var productDtos = _mapper.Map<List<Product>, List<ProductDto>>(result);
            return Ok(new ApıResponse<List<ProductDto>>(StatusType.Success, productDtos));
        }




        #endregion






        #region Memory Cache Logger

        //[HttpGet]
        //public async Task<IActionResult> Get(string? category)
        //{
        //    var result = new List<Product>();
        //    if (category == null)
        //    {
        //        result = _memoryCache.Get("products") as List<Product>;
        //        if (result is null)
        //        {
        //            result = await _context.Products.ToListAsync();
        //            _memoryCache.Set("products", result, TimeSpan.FromMinutes(10));
        //        }
        //    }
        //    else
        //    {
        //        result = _memoryCache.Get($"products-{category}") as List<Product>;
        //        if (result is null)
        //        {
        //            result = await _context.Products.Where(p => p.Category == category).ToListAsync();
        //            _memoryCache.Set($"products-{category}", result, TimeSpan.FromMinutes(10));
        //        }
        //    }

        //    var productDtos = _mapper.Map<List<Product>, List<ProductDto>>(result);
        //    return Ok(new ApıResponse<List<ProductDto>>(StatusType.Success, productDtos));
        //}

        #endregion




        #region Add 1000 Product
        //1000 tane ürün üretir.
        //[HttpPost]
        //public async Task<IActionResult> Post()
        //{
        //    for(int i = 0; i<1000; i++)
        //    {
        //        Product product = new()
        //        {
        //            Category = $"Kategori {i}",
        //            CreateDate = DateTime.Now,
        //            Description = "Açıklama",
        //            Status = true,
        //            Unit = i,
        //            UnitPrice = i * 10,
        //        };

        //        await _context.Products.AddAsync(product);
        //        await _context.SaveChangesAsync();
        //    }
        //        return Ok("Ürün kaydı başarıyla oluşturuldu");
        //}
        #endregion


    }
}
