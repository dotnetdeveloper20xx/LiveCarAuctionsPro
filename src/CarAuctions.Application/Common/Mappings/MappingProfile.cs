using AutoMapper;
using CarAuctions.Application.Features.Auctions.Queries.GetAuctions;
using CarAuctions.Application.Features.Bids.Queries.GetBidHistory;
using CarAuctions.Application.Features.Users.Queries.GetUserById;
using CarAuctions.Application.Features.Vehicles.Queries.GetVehicles;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;

namespace CarAuctions.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Auction, AuctionDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.Value))
            .ForMember(d => d.VehicleId, opt => opt.MapFrom(s => s.VehicleId.Value))
            .ForMember(d => d.SellerId, opt => opt.MapFrom(s => s.SellerId.Value))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.StartingPrice, opt => opt.MapFrom(s => s.StartingPrice.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.StartingPrice.Currency))
            .ForMember(d => d.ReservePrice, opt => opt.MapFrom(s => s.ReservePrice != null ? s.ReservePrice.Amount : (decimal?)null))
            .ForMember(d => d.BuyNowPrice, opt => opt.MapFrom(s => s.BuyNowPrice != null ? s.BuyNowPrice.Amount : (decimal?)null))
            .ForMember(d => d.CurrentHighBid, opt => opt.MapFrom(s => s.CurrentHighBid.Amount))
            .ForMember(d => d.BidCount, opt => opt.MapFrom(s => s.BidIds.Count));

        CreateMap<Vehicle, VehicleDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.Value))
            .ForMember(d => d.VIN, opt => opt.MapFrom(s => s.VIN.Value))
            .ForMember(d => d.Mileage, opt => opt.MapFrom(s => s.Mileage.Value))
            .ForMember(d => d.OwnerId, opt => opt.MapFrom(s => s.OwnerId.Value))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.TitleStatus, opt => opt.MapFrom(s => s.TitleStatus.ToString()));

        CreateMap<Vehicle, VehicleSummaryDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.Value))
            .ForMember(d => d.VIN, opt => opt.MapFrom(s => s.VIN.Value))
            .ForMember(d => d.Mileage, opt => opt.MapFrom(s => s.Mileage.Value));

        CreateMap<ConditionReport, ConditionReportDto>()
            .ForMember(d => d.OverallGrade, opt => opt.MapFrom(s => s.OverallGrade.ToString()))
            .ForMember(d => d.ExteriorGrade, opt => opt.MapFrom(s => s.ExteriorGrade.ToString()))
            .ForMember(d => d.InteriorGrade, opt => opt.MapFrom(s => s.InteriorGrade.ToString()))
            .ForMember(d => d.MechanicalGrade, opt => opt.MapFrom(s => s.MechanicalGrade.ToString()))
            .ForMember(d => d.InspectedAt, opt => opt.MapFrom(s => s.InspectedAt));

        CreateMap<Bid, BidDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.Value))
            .ForMember(d => d.AuctionId, opt => opt.MapFrom(s => s.AuctionId.Value))
            .ForMember(d => d.BidderId, opt => opt.MapFrom(s => s.BidderId.Value))
            .ForMember(d => d.Amount, opt => opt.MapFrom(s => s.Amount.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.Amount.Currency))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.MaxProxyAmount, opt => opt.MapFrom(s => s.MaxProxyAmount != null ? s.MaxProxyAmount.Amount : (decimal?)null));

        CreateMap<User, UserDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.Value))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.Phone))
            .ForMember(d => d.DealerCompanyName, opt => opt.MapFrom(s => s.CompanyName))
            .ForMember(d => d.CreditLimit, opt => opt.MapFrom(s => s.CreditLimit != null ? s.CreditLimit.TotalLimit.Amount : (decimal?)null))
            .ForMember(d => d.CreditLimitCurrency, opt => opt.MapFrom(s => s.CreditLimit != null ? s.CreditLimit.TotalLimit.Currency : null));
    }
}
