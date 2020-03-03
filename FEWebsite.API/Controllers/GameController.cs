﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using FEWebsite.API.Data.BaseServices;
using AutoMapper;
using FEWebsite.API.DTOs.GameDTOs;
using System.Collections.Generic;

namespace FEWebsite.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private IGameService GameService { get; }

        private IMapper Mapper { get; }

        public GameController(IGameService gameService, IMapper mapper)
        {
            this.GameService = gameService;
            this.Mapper = mapper;
        }

        // GET api/games
        [AllowAnonymous]
        [HttpGet]
        public async Task<OkObjectResult> GetGames()
        {
            var games = await this.GameService
                .GetGames()
                .ConfigureAwait(false);

            var gamesDto = this.Mapper.Map<IEnumerable<GameForDetailedDto>>(games);

            return this.Ok(gamesDto);
        }

        // GET api/games/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<OkObjectResult> GetGame(int id)
        {
            var game = await this.GameService
                .GetGame(id)
                .ConfigureAwait(false);

            var gameDto = this.Mapper.Map<GameForDetailedDto>(game);

            return this.Ok(gameDto);
        }

        // POST api/games
        [HttpPost]
        public void Post([FromBody] string value)
        {

        }

        // PUT api/games/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {

        }

        // DELETE api/games/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }
    }
}