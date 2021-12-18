using FluentValidation;
using Nest;
using PepsiCompetitive.Modules.Beats.Requests;
using PepsiCompetitive.Modules.Videos.Requests;

namespace PepsiCompetitive.Modules.Beats.Validations
{
    public class VideoUploadValidation : AbstractValidator<VideoUploadRequest>
    {
        private readonly IElasticClient? _elasticClient;
        public VideoUploadValidation(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;

            RuleFor(x => x.Title)
             .NotEmpty().NotNull().WithMessage("{PropertyName}:Required")
             
             ;

            RuleFor(x => x.Lyric)
              .NotEmpty().NotNull().WithMessage("{PropertyName}:Required")
              .Must(IsValidLyric).WithMessage("{PropertyName}:Invalid")
              .WithName("Lyrics")
              ;

            RuleFor(x => x.Avatar)
              .NotEmpty().NotNull().WithMessage("{PropertyName}:Required")
              .Must(IsValidImage).WithMessage("{PropertyName}:Invalid")
              .WithName("Avatar")
              ;


            RuleFor(x => x.VideoClip)
              .NotEmpty().NotNull().WithMessage("{PropertyName}:Required")
              .Must(IsValidAudio).WithMessage("{PropertyName}:Invalid")
              .WithName("Video")
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
        private bool IsValidAudio(IFormFile file)
        {
            if (file == null) return false;
            return file.ContentType.Contains("video");
        }
    }
}
