using AccountManagementV2.App.Ultilities;
using Microsoft.AspNetCore.Mvc;
using PepsiCompetitive.App.Controllers;
using PepsiCompetitive.Modules.Players.Entities;
using PepsiCompetitive.Modules.Players.Requests;
using PepsiCompetitive.Modules.Players.Services;
using System.Security.Claims;

namespace PepsiCompetitive.Modules.Players.Controllers
{
    public class PlayerController : BaseController
    {
        private readonly IPlayerServices playerServices;

        public PlayerController(IPlayerServices PlayerServices)
        {
            playerServices = PlayerServices;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] PlayerLoginRequest request)
        {
            string token = await playerServices.Login(request);
            return ResponseOk(token);
        }

       

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromForm] PlayerSignUpRequest request)
        {
            Player player = await playerServices.Store(request);
            return ResponseCreated(player);
        }

        [HttpGet("find")]
        public async Task<IActionResult> Get()
        {
            var players = await playerServices.Get();
            return ResponseOk(players);
        }

        [HttpPost("finds")]
        public async Task<IActionResult> GetById([FromForm] List<string> ids)
        {
            List<Player> players = await playerServices.GetByID(ids);
            return ResponseOk(players);
        }

        [HttpPatch("update")]
        public async Task<IActionResult> Update([FromForm] PlayerUpdateRequest request)
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Player player = await playerServices.Update(id, request);
            return ResponseOk(player);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Login([FromForm] List<string> id)
        {
            List<Player> listPlayer = await playerServices.Delete(id);
            return ResponseOk(listPlayer);
        }

        [HttpGet("players_sort")]
        public async Task<IActionResult> ShowAllAccount([FromQuery] PaginationRequest paginationRequest)
        {
            PaginationResponse<Player> paginationResponse = await playerServices.ShowAllPaginationAsync(paginationRequest);
            return ResponseOk(paginationResponse);
        }
    }
}
