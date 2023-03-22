using AutoMapper;
using OrderAppWebApı.Models.Dtos;
using OrderAppWebApı.Models.Entities;

namespace OrderAppWebApı.Context
{
    public class MapperProfile :Profile
    {
        public MapperProfile() 
        {
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();
            CreateMap<Order, CreateOrderRequest>();
            CreateMap<CreateOrderRequest,Order>();
            CreateMap<ProductDetailDto,OrderDetail>();
            CreateMap<OrderDetail, ProductDetailDto>();
        }
    }
}
