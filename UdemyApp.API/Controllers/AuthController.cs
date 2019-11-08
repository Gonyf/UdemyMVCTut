using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UdemyApp.API.Data;
using UdemyApp.API.DTOs;
using UdemyApp.API.Models;

namespace UdemyApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo, IConfiguration configuration)
        {
            _repo = repo;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserForRegisterDTO userForRegisterDTO)
        {
            // validate request

            userForRegisterDTO.Username = userForRegisterDTO.Username.ToLower();

            if (await _repo.UserExists(userForRegisterDTO.Username))
            {
                return BadRequest("Username already exists");
            }

            var userToCreate = new User
            {
                Username = userForRegisterDTO.Username
            };

            var createdUser = await _repo.Register(userToCreate, userForRegisterDTO.Password);
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDTO userForLoginDTO)
        {
            userForLoginDTO.Username = userForLoginDTO.Username.ToLower();

            var userfromRepo = await _repo.Login(userForLoginDTO.Username, userForLoginDTO.Password);

            if (userfromRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userfromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userfromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor 
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}