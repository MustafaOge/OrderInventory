using System;
using AutoMapper;
using OrderInventory.Application.DTOs;
using OrderInventory.Domain.Entities;

namespace OrderInventory.Application.AutoMapper;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // Product
        CreateMap<Product, ProductDto>().ReverseMap();

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        // Order
        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(
                destination => destination.ProductDto,
                options => options.MapFrom(source => source.Product));
        CreateMap<OrderItemDto, OrderItem>()
            .ForMember(destination => destination.Order, options => options.Ignore())
            .ForMember(destination => destination.Product, options => options.Ignore());

    }

}
