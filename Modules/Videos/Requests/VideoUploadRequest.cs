using PepsiCompetitive.Modules.Players.Entities;

namespace PepsiCompetitive.Modules.Videos.Requests
{
    public class VideoUploadRequest
    {
        public string? Title { get; set; }
        public IFormFile? VideoClip { get; set; }
        public IFormFile? Lyric { get; set; }
        public IFormFile? Avatar { get; set; }
        public string? PlayerId { get; set; }
    }
}
