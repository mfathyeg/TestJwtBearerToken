using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TestJwtBearerToken.Data;
using TestJwtBearerToken.Models;
using TestJwtBearerToken.Utilities;

namespace TestJwtBearerToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<MyUser> userManager;
        private readonly SignInManager<MyUser> signInManager;
        private readonly MyIdentityContext db;
        private readonly IConfiguration configuration;

        public AccountController(UserManager<MyUser> userManager
            , SignInManager<MyUser> signInManager
            , MyIdentityContext db
            , IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.db = db;
            this.configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<LoginDto> Login(LoginModel model)
        {
            var o = await userManager.FindByNameAsync(model.UserName);
            if (o == null) return new LoginDto();

            var check = await signInManager.CheckPasswordSignInAsync(o, model.Password, false);
            if (check.IsLockedOut) return new LoginDto(LoginStatus.Locked);
            if (!check.Succeeded) return new LoginDto();

            var refToken = new SystemRefreshToken
            {
                UserId = o.Id,
                RefreshToken = TokenManager.GenerateRefreshToken(),
                TokenTime = DateTime.Now
            };
            await db.SystemRefreshTokens.AddAsync(refToken);
            await db.SaveChangesAsync();

            var accessToken = TokenManager.GenerateAccessToken(o,
                configuration["JwtTokenSetting:Key"],
                Convert.ToInt32(configuration["JwtTokenSetting:LifeTime"]),
                configuration["JwtTokenSetting:Audience"],
                configuration["JwtTokenSetting:Issuer"]);
            return new LoginDto
            {
                Status = LoginStatus.Success,
                AccessToken = accessToken,
                RefreshToken = refToken.RefreshToken
            };
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<LoginDto> RefreshToken(LoginDto model)
        {
            var refreshLifetime = Convert.ToInt32(configuration["JwtTokenSetting:RefreshLifetime"]);
            var expiredToken = TokenManager.GetExpiredToken(model.AccessToken, configuration["JwtTokenSetting:Key"]);

            if (expiredToken == null) return new LoginDto(LoginStatus.NotAuthorized);

            var userName = expiredToken.Identity.Name;
            var user = await userManager.FindByNameAsync(userName);

            var token = await db.SystemRefreshTokens.Where(a => a.RefreshToken == model.RefreshToken && a.UserId == user.Id).SingleOrDefaultAsync();

            if (token == null || token.TokenTime.AddMinutes(refreshLifetime) < DateTime.Now)
                return new LoginDto(LoginStatus.NotAuthorized);

            var refToken = new SystemRefreshToken
            {
                UserId = user.Id,
                RefreshToken = TokenManager.GenerateRefreshToken(),
                TokenTime = DateTime.Now
            };
            db.SystemRefreshTokens.Remove(token);
            await db.SystemRefreshTokens.AddAsync(refToken);
            await db.SaveChangesAsync();

            var accessToken = TokenManager.GenerateAccessToken(user,
                configuration["JwtTokenSetting:Key"],
                Convert.ToInt32(configuration["JwtTokenSetting:LifeTime"]),
                configuration["JwtTokenSetting:Audience"],
                configuration["JwtTokenSetting:Issuer"]);
            return new LoginDto
            {
                Status = LoginStatus.Success,
                AccessToken = accessToken,
                RefreshToken = refToken.RefreshToken
            };
        }

        [AllowAnonymous]
        [HttpPost("Create")]
        public async Task<bool> CreateUser(CreateUserModel model)
        {
            var o = await userManager.CreateAsync(new MyUser
            {
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email
            }, model.Password);
            return o.Succeeded;
        }

        [HttpGet("GetUser")]
        public string GetUser()
        {
            return $"User:{User.Identity.Name}, Time:{DateTime.Now.ToLongTimeString()}";
        }
    }
}

