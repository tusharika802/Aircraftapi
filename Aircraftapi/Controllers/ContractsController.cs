using Aircraftapi.Data;
using Aircraftapi.DTOs;
using Aircraftapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AircraftDashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public ContractsController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllContracts()
        {
            var allPartners = await _context.Partners.ToListAsync();
            var contracts = await _context.Contracts.ToListAsync();

            // For each contract, build a object result with partner names
            var result = contracts.Select(contract =>
            {
                // Parse the comma-separated partner IDs into a list of integers
                var ids = contract.PartnerIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id, out var parsedId) ? parsedId : -1)
                    .Where(id => id != -1)
                    .ToList() ?? new List<int>();

                // Finding  partner names for these IDs
                var partnerNames = allPartners
                    .Where(p => ids.Contains(p.Id))
                    .Select(p => p.Name)
                    .ToList();

                return new
                {
                    contract.Id,
                    contract.Title,
                    contract.IsActive,
                    PartnerIds = string.Join(", ", ids),
                    PartnerNames = string.Join(", ", partnerNames)
                };
            });

            // Return the list of contracts with partner names
            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetActiveContractsCount()
        {
            var count = await _context.Contracts.CountAsync(c => c.IsActive);
            return Ok(count);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddContract([FromBody] ContractCreateDto dto)
        {
            //check if dto is not nulla and title not empty
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Invalid contract data");

            var partnerIdList = dto.PartnerIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var parsedId) ? parsedId : -1)
                .Where(id => id != -1)
                .ToList();

            if (partnerIdList.Count == 0)
                return BadRequest("No valid partner IDs provided.");

            // Fetching  the partner records from the database
            var validPartners = await _context.Partners
                .Where(p => partnerIdList.Contains(p.Id))
                .ToListAsync();

            // Check that all ids matches real partners
            if (validPartners.Count != partnerIdList.Count)
                return BadRequest("One or more selected partners do not exist.");


            string partnerIdStr = string.Join(",", validPartners.Select(p => p.Id));
            var partnerNames = validPartners.Select(p => p.Name).ToList();

            // Create a new contract entity
            var contract = new Contract
            {
                Title = dto.Title,
                IsActive = dto.IsActive,
                PartnerIds = partnerIdStr
            };

            // Add the contract to the database
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            // Send email to each partner about the new contract
            foreach (var partner in validPartners)
            {
                if (!string.IsNullOrEmpty(partner.Email))
                {
                    // List other partners (excluding the current one)
                    var otherPartners = partnerNames
                        .Where(name => name != partner.Name)
                        .ToList();

                    var otherPartnersText = otherPartners.Any()
                        ? string.Join(", ", otherPartners)
                        : "No other partners.";

                    var subject = "New Contract Assignment";

                    var body = new StringBuilder();
                    body.AppendLine($"<p>Dear {partner.Name},</p>");
                    body.AppendLine($"<p>You have been assigned to the contract: <strong>{contract.Title}</strong>.</p>");
                    body.AppendLine($"<p><strong>Other partners:</strong> {otherPartnersText}</p>");
                    body.AppendLine("<p>Thank you.</p>");

                    await _emailSender.SendEmailAsync(partner.Email, subject, body.ToString());
                }
            }

            return Ok(new
            {
                contract.Id,
                contract.Title,
                contract.IsActive,
                PartnerIds = partnerIdStr,
                PartnerNames = string.Join(", ", partnerNames)
            });
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> UpdateContract(int id, [FromBody] Contract updatedContract)
        {
            // Fetch the existing contract from the database
            var existing = await _context.Contracts.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Parse the comma-separated partner IDs from updatedContract
            var partnerIdList = updatedContract.PartnerIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(pid => int.TryParse(pid, out var parsed) ? parsed : -1)
                .Where(pid => pid != -1)
                .ToList();

            if (partnerIdList.Count == 0)
                return BadRequest("No valid partner IDs provided.");

            // Get valid partners from the database
            var validPartners = await _context.Partners
                .Where(p => partnerIdList.Contains(p.Id))
                .ToListAsync();

            if (validPartners.Count != partnerIdList.Count)
                return BadRequest("Some provided partner IDs are invalid.");

            // Get partner names for reference
            var partnerNames = validPartners.Select(p => p.Name).ToList();

            // Update contract fields
            existing.Title = updatedContract.Title;
            existing.IsActive = updatedContract.IsActive;
            existing.PartnerIds = string.Join(",", validPartners.Select(p => p.Id));

            // Save updated contract to database
            await _context.SaveChangesAsync();

            // Send email notification to all involved partners
            foreach (var partner in validPartners)
            {
                if (!string.IsNullOrEmpty(partner.Email))
                {
                    // Exclude current partner from "other partners" list
                    var otherPartners = partnerNames.Where(name => name != partner.Name).ToList();

                    var otherPartnersText = otherPartners.Any()
                        ? string.Join(", ", otherPartners)
                        : "No other partners.";

                    var subject = "Contract Updated";

                    var body = new StringBuilder();
                    body.AppendLine($"<p>Dear {partner.Name},</p>");
                    body.AppendLine($"<p>The contract <strong>{existing.Title}</strong> has been updated.</p>");
                    body.AppendLine($"<p><strong>Other partners:</strong> {otherPartnersText}</p>");
                    body.AppendLine("<p>Thank you.</p>");

                    await _emailSender.SendEmailAsync(partner.Email, subject, body.ToString());
                }
            }

            // Return updated contract info
            return Ok(new
            {
                existing.Id,
                existing.Title,
                existing.IsActive,
                PartnerIds = existing.PartnerIds,
                PartnerNames = string.Join(", ", partnerNames)
            });
        }


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}