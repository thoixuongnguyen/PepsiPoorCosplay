namespace PepsiCompetitive.Modules.Beats.Requests
{
    public class BeatStoreRequest
    {
        public string? Name { get; set; }
        public string? Artist { get; set; }
        public int? CoverTimes { get; set; } 
        public IFormFile? Avatar { get; set; }
        public IFormFile? Lyrics { get; set; }
        public IFormFile? Audio { get; set; }
    }
}
