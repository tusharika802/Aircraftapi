namespace Aircraftapi.DTOs
{
    public class ContractCreateDto
    {
        public string Title { get; set; }
        public bool IsActive { get; set; }

        // Comma-separated partner IDs, e.g., "1,2,3"
        public string PartnerIds { get; set; }
    }
}
