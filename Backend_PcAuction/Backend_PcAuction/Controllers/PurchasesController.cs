using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/auctions/{auctionId}/purchases")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IAuctionsRepository _auctionsRepository;

        public PurchasesController(IPurchaseRepository purchaseRepository, IAuctionsRepository auctionsRepository) 
        {
            _purchaseRepository = purchaseRepository;
            _auctionsRepository = auctionsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<SeriesDto>> Get(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var purchase = await _purchaseRepository.GetLastAsync(auctionId);

            if (purchase == null)
            {
                return NotFound();
            }

            return Ok(new PurchaseDto(purchase.Id, purchase.Status, purchase.AuctionWinDate, purchase.PaymentDate, purchase.Buyer.Id, purchase.Auction.Id));
        }
    }
}
