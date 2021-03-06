﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JwtDemo.Authorization.Jwt;
using JwtDemo.Authorization.Secret;
using JwtDemo.Authorization.Secret.Dto;
using JwtDemo.Configuration;
using JwtDemo.Inputs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace JwtDemo.Controllers
{
    /// <summary>
    /// 权限相关
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        #region Initialize

        /// <summary>
        /// Jwt 服务
        /// </summary>
        private readonly IJwtAppService _jwtApp;

        /// <summary>
        /// 日志记录服务
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 用户服务
        /// </summary>
        private readonly ISecretService _secretApp;

        /// <summary>
        /// 配置信息
        /// </summary>
        public IConfiguration _configuration { get; }

        public JwtOptions _jwtOptions { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="jwtApp"></param>
        /// <param name="secretApp"></param>
        /// <param name="jwtOptions"></param>
        public AuthenticateController(ILogger<AuthenticateController> logger, IConfiguration configuration,
            IJwtAppService jwtApp, ISecretService secretApp, IOptions<JwtOptions> jwtOptions)
        {
            _configuration = configuration;
            _jwtApp = jwtApp;
            _secretApp = secretApp;
            _logger = logger;
            _jwtOptions = jwtOptions.Value;
        }

        #endregion



        #region APIs
        /// <summary>
        /// 获取 Jwt 授权数据
        /// </summary>
        /// <param name="dto">授权用户信息</param>
        [HttpPost("token")]
        [AllowAnonymous]
        public IActionResult LoginAsync([FromBody] SecretDto dto)
        {
            //获取用户信息
            var user = _secretApp.GetCurrentUserAsync(dto.Account, dto.Password);

            if (user == null)
                return Ok(new JwtResponseDto
                {
                    Access = "无权访问",
                    Type = "Bearer",
                    Profile = new Profile
                    {
                        Name = dto.Account,
                        Auths = 0,
                        Expires = 0
                    }
                });

            var jwt = _jwtApp.Create(user);

            return Ok(new JwtResponseDto
            {
                Access = jwt.Token,
                Type = "Bearer",
                Profile = new Profile
                {
                    Name = user.UserName,
                    Auths = jwt.Auths,
                    Expires = jwt.Expires
                }
            });
        }

        /// <summary>
        /// 刷新 Jwt 授权数据
        /// </summary>
        /// <param name="dto">刷新授权用户信息</param>
        /// <returns></returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAccessTokenAsync([FromBody] SecretDto dto)
        {
            //Todo：获取用户信息
            var user = _secretApp.GetCurrentUserAsync(dto.Account, dto.Password);

            if (user == null)
                return Ok(new JwtResponseDto
                {
                    Access = "无权访问",
                    Type = "Bearer",
                    Profile = new Profile
                    {
                        Name = dto.Account,
                        Auths = 0,
                        Expires = 0
                    }
                });

            var jwt = await _jwtApp.RefreshAsync(dto.Token, user);

            return Ok(new JwtResponseDto
            {
                Access = jwt.Token,
                Type = "Bearer",
                Profile = new Profile
                {
                    Name = user.UserName,
                    Auths = jwt.Success ? jwt.Auths : 0,
                    Expires = jwt.Success ? jwt.Expires : 0
                }
            });
        }


        /// <summary>
        /// 停用 Jwt 授权数据
        /// </summary>
        /// <returns></returns>
        [HttpPost("deactivate")]
        public async Task<IActionResult> CancelAccessToken()
        {
            await _jwtApp.DeactivateCurrentAsync();
            return Ok();
        }
        #endregion
    }
}