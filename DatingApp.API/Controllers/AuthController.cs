using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp.API.Controllers {
        [Route ("api/[controller]")]
        [ApiController]
        public class AuthController : ControllerBase {
            private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController (IAuthRepository repo,IConfiguration config) {
                _repo = repo;
            _config = config;
        }

            [HttpPost ("registor")]
            public async Task<IActionResult> Registor (UserForRegistorDto userForRegistorDto) {
                userForRegistorDto.Username = userForRegistorDto.Username.ToLower ();
                if (await _repo.UserExists (userForRegistorDto.Username)) {
                    return BadRequest ("user Already Exist");
                }
                var createToUser = new User {
                    UserName = userForRegistorDto.Username
                };
                var createdUser = await _repo.Registor (createToUser,userForRegistorDto.Password);
                return StatusCode(201);
            }
            [HttpPost("login")]
            
            public async Task<IActionResult> Login(UserForLoginDto userForLoginDto){

                 var userForRepo=await _repo.Login(userForLoginDto.Username.ToLower(),userForLoginDto.Password.ToLower());
                 if(userForRepo==null){
                     return Unauthorized();
                 }
                 var claims= new[]{
                     new Claim(ClaimTypes.NameIdentifier,userForRepo.Id.ToString()),
                     new Claim(ClaimTypes.Name,userForRepo.UserName),
                 };
                 var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value)); 
                 var creds=new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);
                 var tokenDescripter=new SecurityTokenDescriptor{
                     Subject=new ClaimsIdentity(claims),
                     Expires= DateTime.Now.AddDays(1),
                     SigningCredentials=creds
                 };
                 var tokenHandler=new JwtSecurityTokenHandler();
                 var token=tokenHandler.CreateToken(tokenDescripter);
                 return Ok(new{
                     token=tokenHandler.WriteToken(token),
                 });

            }
        }
}