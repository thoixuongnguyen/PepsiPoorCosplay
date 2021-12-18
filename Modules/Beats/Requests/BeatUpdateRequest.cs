namespace PepsiCompetitive.Modules.Beats.Requests
{
    public class BeatUpdateRequest
    {
        public string? Name { get; set; }
        public string? Artist { get; set; }
        public int? CoverTimes { get; set; }
        public IFormFile? Avatar { get; set; }
        public IFormFile? Lyrics { get; set; }
        public IFormFile? Audio { get; set; }
    }
}
