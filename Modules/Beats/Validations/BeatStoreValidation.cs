using FluentValidation;
using Nest;
using PepsiCompetitive.Modules.Beats.Requests;

namespace PepsiCompetitive.Modules.Beats.Validations
{
    public class BeatStoreValidation : AbstractValidator<BeatStoreRequest>
    {
        private readonly IElasticClient? _elasticClient;
        public BeatStoreValidation(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;

            RuleFor(x => x.Name)
             .NotEmpty().NotNull().WithMessage("{PropertyName}:Required")
             
             ;


            RuleFor(x => x.Artist)
             .NotEmpty().NotNull().WithMessage("{PropertyName}:Required");
             

            RuleFor(x => x.Lyrics)
              .NotEmpty().NotNull().WithMessage("{PropertyName}:Required")
              .Must(IsValidLyric).WithMessage("{PropertyName}:Invalid")
              .WithName("Lyrics")
              ;

            RuleFor(x => x.Avatar)
              .NotEmpty().NotNull().WithMessage("{PropertyName}:Required")
              .Must(IsValidImage).WithMessage("{PropertyName}:Invalid")
              .WithName("Avatar")
              ;


            RuleFor(x => x.Audio)
              .NotEmpty().NotNull().WithMessage("{PropertyName}:Required")
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
