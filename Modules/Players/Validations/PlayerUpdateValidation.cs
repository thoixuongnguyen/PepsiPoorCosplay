using FluentValidation;
using Nest;
using PepsiCompetitive.Modules.Players.Entities;
using PepsiCompetitive.Modules.Players.Requests;

namespace PepsiCompetitive.Modules.Players.Validations
{
    public class PlayerUpdateValidation : AbstractValidator<PlayerUpdateRequest>
    {
        private readonly IElasticClient _elasticClient;

        public PlayerUpdateValidation(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;

            RuleFor(x => x.Phone)
                .NotNull().NotEmpty()
                .Matches(@"(^[0-9]{10}$)|(^\+[0-9]{2}\s+[0-9]{2}[0-9]{8}$)|(^[0-9]{3}-[0-9]{4}-[0-9]{4}$)").WithMessage("{PropertyName}:Invalid")
                .Must(IsExistedPhone).WithMessage("{PropertyName}:Doesn't exist")
                .WithName("Phone");

            RuleFor(x => x.Avatar)
                .Must(IsValidImage).WithMessage("{PropertyName}:Invalid");
              
        }

        private bool IsExistedPhone(string phone)
        {
            ISearchResponse<Player> player = _elasticClient.Search<Player>(s => s
          .Query(q => q
            .Term(t => t.Phone, phone)
          ));
            return player is not null;
        }

        private bool IsValidImage(IFormFile file)
        {
            if (file == null) return false;
            return file.ContentType.Contains("image");
        }
    }
}
