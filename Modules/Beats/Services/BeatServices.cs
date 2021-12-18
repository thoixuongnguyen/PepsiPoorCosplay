using AccountManagementV2.App.Ultilities;
using Amazon.S3;
using AutoMapper;
using Nest;
using PepsiCompetitive.App.Ultilities;
using PepsiCompetitive.Modules.Beats.Entities;
using PepsiCompetitive.Modules.Beats.Requests;
using System.Text.RegularExpressions;

namespace PepsiCompetitive.Modules.Beats.Services
{
    public interface IBeatServices
    {
        Task<Beat> Store(BeatStoreRequest request);
        Task<Beat> Update(string id, BeatUpdateRequest request);
        Task<List<Beat>> Delete(List<string> id);
        Task<List<Beat>> Get();
        Task<List<Beat>> GetById(List<string> ids);
        Task<PaginationResponse<Beat>> ShowAllPaginationAsync(PaginationRequest request);
    }

    public class BeatServices : IBeatServices
    {
        private readonly IElasticClient _elasticClient;
        private readonly IConfiguration _Configuration;
        private readonly IMapper _mapper;
        private readonly IAmazonS3Utility _amazonS3Utility;

        public BeatServices(IElasticClient elasticClient, IMapper mapper, IConfiguration Configuration, IAmazonS3Utility amazonS3Utility)
        {
            _elasticClient = elasticClient;
            _mapper = mapper;
            _Configuration = Configuration;
            _amazonS3Utility = amazonS3Utility;
        }

        public async Task<List<Beat>> Delete(List<string> id)
        {
            ISearchResponse<Beat> beats = await _elasticClient.SearchAsync<Beat>(idx => idx
               .Query(q => q
                   .Terms(t => t
                       .Field(f => f.Id)
                       .Terms(id)
                   )
               )
           );
            List<Beat> beatList = beats.Documents.ToList();
            await _elasticClient.DeleteManyAsync(beatList);
            return beatList;
        }

        public async Task<List<Beat>> Get()
        {
            ISearchResponse<Beat> results = await _elasticClient.SearchAsync<Beat>(s => s
          .Query(q => q
              .MatchAll()
              )
           );
            return results.Documents.ToList();
        }

        

        public async Task<Beat> Store(BeatStoreRequest request)
        {
            Beat beat = _mapper.Map<Beat>(request);
            beat = await StoreBeat(beat!, request);
            await _elasticClient.IndexDocumentAsync(beat);
            return beat;
        }

        public async Task<Beat> Update(string id, BeatUpdateRequest request)
        {
            ISearchResponse<Beat> beats = _elasticClient.Search<Beat>(s => s
        .Query(q => q
          .Term(t => t.Id, id)
        ));
            Beat? beat = beats.Documents.FirstOrDefault();
            _mapper.Map(request, beat);
            beat = await UpdateBeat(beat!, request);
            await _elasticClient.UpdateAsync<Beat>(beat, u => u.Doc(beat));
            return beat;
        }

        private async Task<Beat> UpdateBeat(Beat beat,BeatUpdateRequest request)
        {
            string? avatarFileName = request.Avatar != null ? new Random().Next() + "_" + Regex.Replace(request.Avatar!.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "") : null;
            string? audioFileName = request.Audio != null ? new Random().Next() + "_" + Regex.Replace(request.Audio!.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "") : null;
            string? lyricFileName = request.Lyrics != null ? new Random().Next() + "_" + Regex.Replace(request.Lyrics!.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "") : null;
            (string? avatarPath, string? avatarError) = await _amazonS3Utility.SaveFileAmazonS3Async(request.Avatar, _Configuration["AmazonS3:BucketName"], "Beat/Avatars/" + avatarFileName, S3CannedACL.PublicRead);
            (string? audioPath, string? audioError) = await _amazonS3Utility.SaveFileAmazonS3Async(request.Audio, _Configuration["AmazonS3:BucketName"], "Beat/Audios/" + audioFileName, S3CannedACL.PublicRead);
            (string? lyricsPath, string? lyricError) = await _amazonS3Utility.SaveFileAmazonS3Async(request.Lyrics, _Configuration["AmazonS3:BucketName"], "Beat/Lyrics/" + lyricFileName, S3CannedACL.PublicRead);
            beat.Avatar = avatarPath;
            beat.Audio = audioPath;
            beat.Lyrics = lyricsPath;
            return beat;
        }
        private async Task<Beat> StoreBeat(Beat beat, BeatStoreRequest request)
        {
            string? avatarFileName = request.Avatar != null ? new Random().Next() + "_" + Regex.Replace(request.Avatar!.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "") : null;
            string? audioFileName = request.Audio != null ? new Random().Next() + "_" + Regex.Replace(request.Audio!.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "") : null;
            string? lyricFileName = request.Lyrics != null ? new Random().Next() + "_" + Regex.Replace(request.Lyrics!.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "") : null;
            (string? avatarPath, string? avatarError) = await _amazonS3Utility.SaveFileAmazonS3Async(request.Avatar, _Configuration["AmazonS3:BucketName"], "Beat/Avatars/" + avatarFileName, S3CannedACL.PublicRead);
            (string? audioPath, string? audioError) = await _amazonS3Utility.SaveFileAmazonS3Async(request.Audio, _Configuration["AmazonS3:BucketName"], "Beat/Audios/" + audioFileName, S3CannedACL.PublicRead);
            (string? lyricsPath, string? lyricError) = await _amazonS3Utility.SaveFileAmazonS3Async(request.Lyrics, _Configuration["AmazonS3:BucketName"], "Beat/Lyrics/" + lyricFileName, S3CannedACL.PublicRead);
            beat.Avatar = avatarPath;
            beat.Audio = audioPath;
            beat.Lyrics = lyricsPath;
            return beat;
        }

        public async Task<List<Beat>> GetById(List<string> beatIds)
        {
            ISearchResponse<Beat> list = await _elasticClient.SearchAsync<Beat>(s => s
                .Query(q => q
                    .Terms(t => t
                        .Field(f => f.Id)
                            .Terms(beatIds)
                    )
                )
            );
            List<Beat> beats = list.Documents.ToList();
            return beats;
        }

        public async Task<PaginationResponse<Beat>> ShowAllPaginationAsync(PaginationRequest request)
        {
            request.PageSize = request.PageSize > 10000 ? 10000 : request.PageSize;
            List<Func<QueryContainerDescriptor<Beat>, QueryContainer>> filters = new();
            if (!string.IsNullOrEmpty(request.SearchContent))
            {
                filters.Add(ft => ft
                    .Bool(b => b
                        .Should(
                            s => s.Wildcard(w => w.Name!.ToLower(), "*" + request.SearchContent.ToLower() + "*"),
                            s => s.Wildcard(w => w.Artist!.ToLower(), "*" + request.SearchContent.ToLower() + "*")
                            )
                        )
                    );
            }
            ISearchResponse<Beat> results = await _elasticClient.SearchAsync<Beat>(s => s
               .From((request.PageNumber - 1) * request.PageSize)
               .Size(request.PageSize)
               .Query(q => q
                    .Bool(b => b
                        .Filter(filters)
                    )
                )
            );
            PaginationUtility<Beat> data = new(results.Documents.ToList(), results.Total, request.PageNumber, request.PageSize);
            PaginationResponse<Beat> paginationResponse = PaginationResponse<Beat>.PaginationInfo(data, data.PageInfo);
            return paginationResponse;
        }
    }
}
