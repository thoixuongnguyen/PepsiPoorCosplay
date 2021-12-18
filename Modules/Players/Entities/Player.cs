using Nest;
using PepsiCompetitive.App.Entities;

namespace PepsiCompetitive.Modules.Players.Entities
{
    public class Player : BaseEntity
    {
        [Keyword]
        public string? Phone { get; set; }
        [Keyword]
        public string? RapName { get; set; }
        public string? Avatar { get; set; }
        public int? TotalVideos { get; set; } = 0;
        public int? TotalViews { get; set; } = 0;
        public int? TotalLikes { get; set; } = 0;
    }
}
