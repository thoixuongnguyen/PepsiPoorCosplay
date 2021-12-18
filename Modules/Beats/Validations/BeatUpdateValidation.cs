using FluentValidation;
using Nest;
using PepsiCompetitive.Modules.Beats.Requests;

namespace PepsiCompetitive.Modules.Beats.Validations
{
    public class BeatUpdateValidation : AbstractValidator<BeatUpdateRequest>
    {
        private readonly IElasticClient _elasticClient;
        public BeatUpdateValidation(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;

            RuleFor(x => x.Lyrics)     
              .Must(IsValidLyric).WithMessage("{PropertyName}:Invalid")
              .WithName("Lyrics")
              ;

            RuleFor(x => x.Avatar)
              .Must(IsValidImage).WithMessage("{PropertyName}:Invalid")
              .WithName("Avatar")
              ;


            RuleFor(x => x.Audio)
              .Must(IsValidAudio).WithMessage("{PropertyName}:Invalid")
              .WithName("Audio")
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
            return file.ContentType.Contains("audio");
        }
    }
}
