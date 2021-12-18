using PepsiCompetitive.App.Entities;

namespace PepsiCompetitive.Modules.Beats.Entities
{
    public class Beat:BaseEntity
    {
        public string? Name { get; set; }
        public string? Artist { get; set; }
        public int? CoverTimes { get; set; } = 0;
        public string? Avatar { get; set; }
        public string? Lyrics { get; set; }
        public string? Audio { get; set; }
    }
}
