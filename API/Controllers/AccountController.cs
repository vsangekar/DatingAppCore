using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenServices _tokenService;

        public AccountController(DataContext context, ITokenServices tokenService)
        {
            _context=context;
            _tokenService=tokenService;
        }

        [HttpPost("Register")]

        public async Task<ActionResult<UserDto>>Register(RegisterDTO RegisterDto)
        {

            if(await UserExists(RegisterDto.Username)) return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();
            var user= new AppUser
            {
                UserName=RegisterDto.Username.ToLower(),
                PasswordHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(RegisterDto.Password)),
                PasswordSalt=hmac.Key
            };

            _context.Users .Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username=user.UserName,
                Token =_tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>>Login(LoginDto loginDto)
        {
            var user=await _context.Users
                       .SingleOrDefaultAsync(x=>x.UserName==loginDto.Username);
            if(user==null) return Unauthorized("Invalid username");
            using var hmac=new HMACSHA512(user.PasswordSalt);
            var ComputeHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for(int i=0;i<ComputeHash.Length;i++)
            {
                if(ComputeHash[i] !=user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }
             return new UserDto
            {
                Username=user.UserName,
                Token =_tokenService.CreateToken(user)
            };
        }

        

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x=>x.UserName==username.ToLower());
        }
        
    }
}