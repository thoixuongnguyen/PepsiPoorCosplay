using Nest;

namespace PepsiCompetitive.Modules.Players.Requests
{
    public class PlayerUpdateRequest
    {
        [Keyword]
        public string? Phone { get; set; }
        [Keyword]
        public string? RapName { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
