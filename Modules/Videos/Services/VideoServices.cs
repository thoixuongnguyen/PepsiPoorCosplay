using AccountManagementV2.App.Ultilities;
using Amazon.S3;
using AutoMapper;
using Nest;
using PepsiCompetitive.App.Ultilities;
using PepsiCompetitive.Modules.Videos.Entities;
using PepsiCompetitive.Modules.Videos.Requests;
using System.Text.RegularExpressions;

namespace PepsiCompetitive.Modules.Videos.Services
{
    public interface IVideoServices
    {
        Task<Video> Store(VideoUploadRequest request);
        Task<Video> Update(string videoId, VideoUpdateRequest request);
        Task<List<Video>> Delete(List<string> videoIds);
        Task<List<Video>> Get();
        Task<List<Video>> GetByID(List<string> videoIds);
        Task<PaginationResponse<Video>> ShowAllPaginationAsync(PaginationRequest request);
    }
    public class VideoServices : IVideoServices
    {
        private readonly IElasticClient _elasticClient;
        private readonly IConfiguration _Configuration;
        private readonly IMapper _mapper;
        private readonly IAmazonS3Utility _amazonS3Utility;

        public VideoServices(IElasticClient elasticClient, IMapper mapper, IConfiguration Configuration, IAmazonS3Utility amazonS3Utility)
        {
            _elasticClient = elasticClient;
            _mapper = mapper;
            _Configuration = Configuration;
            _amazonS3Utility = amazonS3Utility;
        }
        public async Task<List<Video>> Delete(List<string> videoIds)
        {
            ISearchResponse<Video> videos = await _elasticClient.SearchAsync<Video>(idx => idx
               .Query(q => q
                   .Terms(t => t
                       .Field(f => f.Id)
                       .Terms(videoIds)
                   )
               )
           );
            List<Video> videoList = videos.Documents.ToList();
            await _elasticClient.DeleteManyAsync(videoList);
            return videoList;
        }

        public async Task<List<Video>> Get()
        {
            ISearchResponse<Video> results = await _elasticClient.SearchAsync<Video>(s => s
       .Query(q => q
           .MatchAll()
           )
        );
            return results.Documents.ToList();
        }

        public async Task<List<Video>> GetByID(List<string> videoIds)
        {
            ISearchResponse<Video> list = await _elasticClient.SearchAsync<Video>(s => s
               .Query(q => q
                   .Terms(t => t
                       .Field(f => f.Id)
                           .Terms(videoIds)
                   )
               )
           );
            List<Video> videos = list.Documents.ToList();
            return videos;
        }

        public async Task<PaginationResponse<Video>> ShowAllPaginationAsync(PaginationRequest request)
        {
            request.PageSize = request.PageSize > 10000 ? 10000 : request.PageSize;
            List<Func<QueryContainerDescriptor<Video>, QueryContainer>> filters = new();
            if (!string.IsNullOrEmpty(request.SearchContent))
            {
                filters.Add(ft => ft
                    .Bool(b => b
                        .Should(
                            s => s.Wildcard(w => w.Title!.ToLower(), "*" + request.SearchContent.ToLower() + "*"),
                            s => s.Wildcard(w => w.Lyric!.ToLower(), "*" + request.SearchContent.ToLower() + "*"),
                            s => s.Wildcard(w => w.Author!.ToLower(), "*" + request.SearchContent.ToLower() + "*")
                            )
                        )
                    );
            }
            ISearchResponse<Video> results = await _elasticClient.SearchAsync<Video>(s => s
               .From((request.PageNumber - 1) * request.PageSize)
               .Size(request.PageSize)
               .Query(q => q
                    .Bool(b => b
                        .Filter(filters)
                    )
                )
            );
            PaginationUtility<Video> data = new(results.Documents.ToList(), results.Total, request.PageNumber, request.PageSize);
            PaginationResponse<Video> paginationResponse = PaginationResponse<Video>.PaginationInfo(data, data.PageInfo);
            return paginationResponse;
        }

        public async Task<Video> Store(VideoUploadRequest request)
        {
            Video video = _mapper.Map<Video>(request);
            video = await StoreVideo(video!, request);
            await _elasticClient.IndexDocumentAsync(video);
            return video;
        }

        public async Task<Video> Update(string videoId, VideoUpdateRequest request)
        {
            ISearchResponse<Video> videos = _elasticClient.Search<Video>(s => s
        .Query(q => q
          .Term(t => t.Id, videoId)
        ));
            Video? video = videos.Documents.FirstOrDefault();
            _mapper.Map(request, video);
            video = await UpdateVideo(video!, request);
            await _elasticClient.UpdateAsync<Video>(video, u => u.Doc(video));
            return video;
        }

        private async Task<Video> UpdateVideo(Video video, VideoUpdateRequest request)
        {
            string? videoFileName = request.VideoClip != null ? new Random().Next() + "_" + Regex.Replace(request.VideoClip!.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "") : null;
            string? lyricFileName = request.Lyric != null ? new Random().Next() + "_" + Regex.Replace(request.Lyric!.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "") : null;
            (string? videoPath, string? audioError) = await _amazonS3Utility.SaveFileAmazonS3Async(request.VideoClip!, _Configuration["AmazonS3:BucketName"], "Videos/Clips/" + videoFileName, S3CannedACL.PublicRead);
            (string? lyricsPath, string? lyricError) = await _amazonS3Utility.SaveFileAmazonS3Async(request.Lyric!, _Configuration["AmazonS3:BucketName"], "Videos/Lyrics/" + lyricFileName, S3CannedACL.PublicRead);
            video.VideoClip = videoPath;
            video.Lyric = lyricsPath;
            return video;
        }
        private async Task<Video> StoreVideo(Video video, VideoUploadRequest request)
        {
            string? videoFileName = request.VideoClip != null ? new Random().Next() + "_" + Regex.Replace(request.VideoClip!.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "") : null;
            string? lyricFileName = request.Lyric != null ? new Random().Next() + "_" + Regex.Replace(request.Lyric!.FileName.Trim(), @"[^a-zA-Z0-9-_.]", "") : null;
            (string? videoPath, string? audioError) = await _amazonS3Utility.SaveFileAmazonS3Async(request.VideoClip!, _Configuration["AmazonS3:BucketName"], "Videos/Clips/" + videoFileName, S3CannedACL.PublicRead);
            (string? lyricsPath, string? lyricError) = await _amazonS3Utility.SaveFileAmazonS3Async(request.Lyric!, _Configuration["AmazonS3:BucketName"], "Videos/Lyrics/" + lyricFileName, S3CannedACL.PublicRead);
            video.VideoClip = videoPath;
            video.Lyric = lyricsPath;
            return video;
        }
    }
}
    