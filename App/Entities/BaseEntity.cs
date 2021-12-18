using Nest;

namespace PepsiCompetitive.App.Entities
{
    [ElasticsearchType(IdProperty = "Id")]
    public class BaseEntity
    {
        [Keyword]
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? DateCreate { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset DateUpdate { get; set; } = DateTimeOffset.Now;
    }
}
