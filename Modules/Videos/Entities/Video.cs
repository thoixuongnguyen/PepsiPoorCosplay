using PepsiCompetitive.App.Entities;
using PepsiCompetitive.Modules.Players.Entities;

namespace PepsiCompetitive.Modules.Videos.Entities
{
    public class Video : BaseEntity
    {
        public string? Title { get; set; }
        public string? Lyric { get; set; }
        public string? Author { get; set; }
        public int? View { get; set; } = 0;
        public int? Like { get; set; } = 0;
        public string? VideoClip { get; set; }
        public string? Avatar { get; set; }
        public string? PlayerId { get; set; }  
    }
}
