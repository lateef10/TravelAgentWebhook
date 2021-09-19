using Airline.ApplicationDbContext;
using Airline.Models;
using Airline.Models.Dto;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Airline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookSubscriptionController : ControllerBase
    {
        private readonly AirlineDbContext _context;
        private readonly IMapper _mapper;

        public WebhookSubscriptionController(AirlineDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{secret}", Name = "GetSubscriptionBySecret")]
        public ActionResult<WebhookSubscriptionReadDto> GetSubscriptionBySecret(string secret)
        {
            var subscription = _context.webhookSubscriptions.FirstOrDefault(s => s.Secret == secret);

            if (subscription == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<WebhookSubscriptionReadDto>(subscription));
        }

        [HttpPost]
        public ActionResult<WebhookSubscriptionReadDto> CreateSubsription(WebhookSubscriptionCreateDto webhookSubscriptionCreateDto)
        {
            var subscription = _context.webhookSubscriptions.FirstOrDefault(s => s.WebhookURI == webhookSubscriptionCreateDto.WebhookURI);

            if (subscription == null)
            {
                subscription = _mapper.Map<WebhookSubscription>(webhookSubscriptionCreateDto);

                subscription.Secret = Guid.NewGuid().ToString();
                subscription.WebhookPublisher = "NGAir";
                try
                {
                    _context.webhookSubscriptions.Add(subscription);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

                var webhookSubscriptionReadDto = _mapper.Map<WebhookSubscriptionReadDto>(subscription);

                return CreatedAtRoute(nameof(GetSubscriptionBySecret), new { secret = webhookSubscriptionReadDto.Secret }, webhookSubscriptionReadDto);
            }
            else
            {
                return NoContent();
            }
        }
    }
}
