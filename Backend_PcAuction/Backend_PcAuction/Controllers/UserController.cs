﻿using Backend_PcAuction.Auth.Models;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserController : ControllerBase
    {

        private readonly IAuctionsRepository _auctionsRepository;
        private readonly IBidsRepository _bidsRepository;
        private readonly IPurchaseRepository _purchaseRepository;

        public UserController(IAuctionsRepository auctionsRepository, IBidsRepository bidsRepository, IPurchaseRepository purchaseRepository)
        {
            _auctionsRepository = auctionsRepository;
            _bidsRepository = bidsRepository;
            _purchaseRepository = purchaseRepository;
        }

        [HttpGet]
        [Route("bids")]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<IEnumerable<BidWithAuctionIdDto>>> GetAllBids()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var bids = await _bidsRepository.GetAllByUserAsync(userId);

            return Ok(bids.Select(bid => new BidWithAuctionIdDto(bid.Id, bid.Amount, bid.CreationDate, bid.Auction.Id, bid.Auction.Name)));
        }

        [HttpGet]
        [Route("winningBids")]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<IEnumerable<BidWithAuctionIdDto>>> GetAllWinningBids()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);


            var bids = await _bidsRepository.GetAllByUserAsync(userId);
            var winningBids = new List<Bid>();
            
            foreach(var bid in bids)
            {
                var highestBid = await _bidsRepository.GetLastAsync(bid.Auction.Id);
                if(highestBid.Id == bid.Id && (bid.Auction.Status == AuctionStatuses.Active || bid.Auction.Status == AuctionStatuses.ActiveNA))
                {
                    winningBids.Add(bid);
                }
            }

            return Ok(winningBids.Select(bid => new BidWithAuctionIdDto(bid.Id, bid.Amount, bid.CreationDate, bid.Auction.Id, bid.Auction.Name)));
        }

        [HttpGet]
        [Route("newauctions")]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<IEnumerable<Auction>>> GetAllNewAuctions()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var auctions = await _auctionsRepository.GetAllNewByUserAsync(userId);

            var resultAuctions = auctions.Select(auction =>
                            new AuctionWithPartNameDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                                auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status, auction.UserId, auction.Part.Name, auction.Part.Category.Id));

            return Ok(resultAuctions);
        }

        [HttpGet]
        [Route("activeauctions")]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<IEnumerable<Auction>>> GetAllActiveAuctions()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var auctions = await _auctionsRepository.GetAllActiveByUserAsync(userId);

            var resultAuctions = auctions.Select(auction =>
                            new AuctionWithPartNameDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                                auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status, auction.UserId, auction.Part.Name, auction.Part.Category.Id));

            return Ok(resultAuctions);
        }

        [HttpGet]
        [Route("endedauctions")]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<IEnumerable<Auction>>> GetAllEndedAuctions()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var auctions = await _auctionsRepository.GetAllEndedByUserAsync(userId);

            var resultAuctions = auctions.Select(auction =>
                            new AuctionWithPartNameDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                                auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status, auction.UserId, auction.Part.Name, auction.Part.Category.Id));

            return Ok(resultAuctions);
        }

        [HttpGet]
        [Route("wonauctions")]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<IEnumerable<Auction>>> GetAllWonAuctions()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var purchases = await _purchaseRepository.GetAllUserPurchasesAsync(userId);

            var auctions = new List<Auction>();

            foreach(var purchase in purchases)
            {
                if(!auctions.Contains(purchase.Auction))
                {
                    auctions.Add(purchase.Auction);
                }
            }

            auctions = auctions.OrderByDescending(a => a.EndDate).ToList();

            var resultAuctions = auctions.Select(auction =>
                            new AuctionWithPartNameDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                                auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status, auction.UserId, auction.Part.Name, auction.Part.Category.Id));

            return Ok(resultAuctions);
        }
    }
}
