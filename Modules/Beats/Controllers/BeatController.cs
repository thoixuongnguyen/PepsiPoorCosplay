using AccountManagementV2.App.Ultilities;
using Microsoft.AspNetCore.Mvc;
using PepsiCompetitive.App.Controllers;
using PepsiCompetitive.Modules.Beats.Entities;
using PepsiCompetitive.Modules.Beats.Requests;
using PepsiCompetitive.Modules.Beats.Services;

namespace PepsiCompetitive.Modules.Beats.Controllers
{
    public class BeatController : BaseController
    {
            private readonly IBeatServices beatServices;

            public BeatController(IBeatServices BeatServices)
            {
                beatServices = BeatServices;
            }

            [HttpGet("get")]
            public async Task<IActionResult> Get()
            {
                List<Beat> beats = await beatServices.Get();
                return ResponseOk(beats);
            }

            [HttpPost("gets")]
            public async Task<IActionResult> GetById([FromForm] List<string> ids)
            {
                List<Beat> beats = await beatServices.GetById(ids);
                return ResponseOk(beats);
            }

        [HttpPost("upload")]
            public async Task<IActionResult> StoreBeat([FromForm] BeatStoreRequest request)
            {
                Beat beat = await beatServices.Store(request);
                return ResponseCreated(beat);
            }

            [HttpPatch("update")]
            public async Task<IActionResult> Update([FromForm]string id,[FromForm] BeatUpdateRequest request)
            {
                Beat beat = await beatServices.Update(id, request);
                return ResponseOk(beat);
            }

            [HttpDelete("delete")]
            public async Task<IActionResult> Login([FromForm] List<string> id)
            {
                List<Beat> list = await beatServices.Delete(id);
                return ResponseOk(list);
            }

            [HttpGet("sort")]
            public async Task<IActionResult> ShowAllAccount([FromQuery] PaginationRequest paginationRequest)
            {
                PaginationResponse<Beat> paginationResponse = await beatServices.ShowAllPaginationAsync(paginationRequest);
                return ResponseOk(paginationResponse);
            }

    }
    }
