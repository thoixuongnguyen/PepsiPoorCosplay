using AccountManagementV2.App.Ultilities;
using Amazon.S3;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nest;
using PepsiCompetitive.App.Helpers;
using PepsiCompetitive.App.Ultilities;
using PepsiCompetitive.Modules.Players.Entities;
using PepsiCompetitive.Modules.Players.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace PepsiCompetitive.Modules.Players.Services
{
    public interface IPlayerServices
    {
        Task<string> Login(PlayerLoginRequest playerLoginRequest);
        Task<Player> Store(PlayerSignUpRequest playerStoreRequest);
        Task<Player> Update(string id, PlayerUpdateRequest playerUpdateRequest);
        Task<List<Player>> Delete(List<string> id);
        Task<List<Player>> Get();
        Task<List<Player>> GetByID(List<string> ids);
        Task<PaginationResponse<Player>> ShowAllPaginationAsync(PaginationRequest paginationRequest);
    }

    public class PlayerServices : IPlayerServices
    {
        private readonly IElasticClient _elasticClient;
        private readonly IConfiguration _Configuration;
        private readonly IMapper _mapper;
        private readonly IAmazonS3Utility _amazonS3Utility;
        public PlayerServices (IElasticClient elasticClient, IMapper mapper, IConfiguration Configuration, IAmazonS3Utility amazonS3Utility)
        {
            _elasticClient = elasticClient;
            _mapper = mapper;
            _Configuration = Configuration;
            _amazonS3Utility = amazonS3Utility;
        }
        public async Task<List<Player>> Delete(List<string> id)
        {
            ISearchResponse<Player> players = await _elasticClient.SearchAsync<Player>(idx => idx
                .Query(q => q
                    .Terms(t => t
                        .Field(f => f.Id)
                        .Terms(id)
                    )
                )
            );
            List<Player> player = players.Documents.ToList();
            await _elasticClient.DeleteManyAsync(player);
            return player;
        }

        public async Task<List<Player>> Get()
        {
            ISearchResponse<Player> results =  await _elasticClient.SearchAsync<Player>(s => s
            .Query(q => q
                .MatchAll()
                )
            );
            return results.Documents.ToList();
        }

        public async Task<List<Player>> GetByID(List<string> idPlayers)
        {
            ISearchResponse<Player> list = await _elasticClient.SearchAsync<Player>(s => s
                .Query(q => q
                    .Terms(t => t
                        .Field(f => f.Id)
                            .Terms(idPlayers)   
                    )
                )
            );
            List<Player> players = list.Documents.ToList();
            return players;
        }

        public async Task<string> Login(PlayerLoginRequest playerLoginRequest)
        {
            ISearchResponse<Player> players = _elasticClient.Search<Player>(s => s
          .Query(q => q
            .Term(t => t.Phone, playerLoginRequest.Phone)
          ));
            Player? player = players.Documents.FirstOrDefault();
            string token =  GenerateJwtToken(player);
            player.DateUpdate = DateTime.Now;
            await _elasticClient.UpdateAsync<Player>(player, u => u.Doc(player)
            );
            return token;
        }

        public async Task<Player> Store(PlayerSignUpRequest request)
        {
            Player player = _mapper.Map<Player>(request);
            await _elasticClient.IndexDocumentAsync(player);
            return player;
        }

        public async Task<Player> Update( string id ,PlayerUpdateRequest playerUpdateRequest)
        {
            ISearchResponse<Player> players =  _elasticClient.Search<Player>(s => s
          .Query(q => q
            .Term(t => t.Id, id)
          ));
            Player? player = players.Documents.FirstOrDefault();
            _mapper.Map(playerUpdateRequest, player);
            if (playerUpdateRequest.Avatar != null)
            {
                string fileFullName = new Random().Next() + "_" + Regex.Replace(playerUpdateRequest.Avatar.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "");
                (string? avatarPath, string? error)  = await  _amazonS3Utility.SaveFileAmazonS3Async(playerUpdateRequest.Avatar,_Configuration["AmazonS3:BucketName"], "PlayerAvatar/"+ fileFullName, S3CannedACL.PublicRead);
                player.Avatar = avatarPath;
            }

            
            await _elasticClient.UpdateAsync<Player>(player, u => u.Doc(player));
            return player;
        }
        public async Task<PaginationResponse<Player>> ShowAllPaginationAsync(PaginationRequest paginationRequest)
        {
            paginationRequest.PageSize = paginationRequest.PageSize > 10000 ? 10000 : paginationRequest.PageSize;
            List<Func<QueryContainerDescriptor<Player>, QueryContainer>> filters = new();
            if (!string.IsNullOrEmpty(paginationRequest.SearchContent))
            {
                filters.Add(ft => ft
                    .Bool(b => b
                        .Should(
                            s => s.Wildcard(w => w.RapName!.ToLower(), "*" + paginationRequest.SearchContent.ToLower() + "*"),
                            s => s.Wildcard(w => w.Phone!.ToLower(), "*" + paginationRequest.SearchContent.ToLower() + "*")
                            )
                        )
                    );
            }
            ISearchResponse<Player> results = await _elasticClient.SearchAsync<Player>(s => s
               .From((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
               .Size(paginationRequest.PageSize)
               .Query(q => q
                    .Bool(b => b
                        .Filter(filters)
                    )
                )
            );
            PaginationUtility<Player> data = new(results.Documents.ToList(), results.Total, paginationRequest.PageNumber, paginationRequest.PageSize);
            PaginationResponse<Player> paginationResponse = PaginationResponse<Player>.PaginationInfo(data, data.PageInfo);
            return paginationResponse;
        }

        private string GenerateJwtToken(Player? player)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, player.Id!),
                new Claim("Id", player.Id!),
                new Claim("Phone", player.Phone!),
                new Claim("RapName", player.RapName!),
            };

            JwtSecurityToken jwtSecurityToken = new(
                    claims: claims,
                    expires: DateTime.Now.AddDays(double.Parse(_Configuration["JwtSettings:ExpiredTime"].ToString())),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Configuration["JwtSettings:SecretKey"])), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        }
    }
}
