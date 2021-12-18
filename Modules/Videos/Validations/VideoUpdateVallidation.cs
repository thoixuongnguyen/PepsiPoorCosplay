using FluentValidation;
using Nest;
using PepsiCompetitive.Modules.Beats.Requests;
using PepsiCompetitive.Modules.Videos.Requests;

namespace PepsiCompetitive.Modules.Videos.Validations
{
    public class VideoUpdateValidation : AbstractValidator<VideoUpdateRequest>
    {
        private readonly IElasticClient _elasticClient;
        public VideoUpdateValidation(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;

            RuleFor(x => x.Lyric)     
              .Must(IsValidLyric).WithMessage("{PropertyName}:Invalid")
              .WithName("Lyric")
              ;

            RuleFor(x => x.VideoClip)
              .Must(IsValidVideo).WithMessage("{PropertyName}:Invalid")
              .WithName("Video")
              ;


            RuleFor(x => x.Avatar)
              .Must(IsValidImage).WithMessage("{PropertyName}:Invalid")
              .WithName("Avatar")
              ;


        }

        private bool IsValidImage(IFormFile file)
        {
            if (file == null) return false;
            return file.ContentType.Contains("image");
        }
        private bool IsValidLyric(IFormFile file)
        {
            if (file == null) return false;
            return file.ContentType.Contains("text");
        }
        private bool IsValidVideo(IFormFile file)
        {
            if (file == null) return false;
            return file.ContentType.Contains("video");
        }
    }
}
