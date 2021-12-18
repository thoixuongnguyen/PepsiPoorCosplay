using AccountManagementV2.App.Ultilities;
using Microsoft.AspNetCore.Mvc;
using PepsiCompetitive.App.Controllers;
using PepsiCompetitive.Modules.Videos.Entities;
using PepsiCompetitive.Modules.Videos.Requests;
using PepsiCompetitive.Modules.Videos.Services;

namespace PepsiCompetitive.Modules.Videos.Controllers
{
    public class VideoController : BaseController
    {
        private readonly IVideoServices videoServices;

        public VideoController(IVideoServices videoServices)
        {
            this.videoServices = videoServices;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            List<Video> videos = await videoServices.Get();
            return ResponseOk(videos);
        }

        [HttpPost("gets")]
        public async Task<IActionResult> GetById([FromForm] List<string> ids)
        {
            List<Video> videos = await videoServices.GetByID(ids);
            return ResponseOk(videos);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> StoreBeat([FromForm] VideoUploadRequest request)
        {
            Video video = await videoServices.Store(request);
            return ResponseCreated(video);
        }

        [HttpPatch("update")]
        public async Task<IActionResult> Update([FromForm] string id, [FromForm] VideoUpdateRequest request)
        {
            Video video = await videoServices.Update(id, request);
            return ResponseOk(video);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Login([FromForm] List<string> id)
        {
            List<Video> list = await videoServices.Delete(id);
            return ResponseOk(list);
        }

        [HttpGet("sort")]
        public async Task<IActionResult> ShowAllAccount([FromQuery] PaginationRequest paginationRequest)
        {
            PaginationResponse<Video> paginationResponse = await videoServices.ShowAllPaginationAsync(paginationRequest);
            return ResponseOk(paginationResponse);
        }
    }
}
