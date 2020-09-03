﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using freeSqlWeb1.AutoMappers;
using freeSqlWeb1.Domain;
using freeSqlWeb1.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace freeSqlWeb1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IFreeSql _freesql;
        public UserController(IFreeSql freesql)
        {
            _freesql = freesql;
        }

        /// <summary>
        /// 查询所有user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> Get()
        {
            List<User> users = _freesql.Select<User>().OrderByDescending(r => r.Name).ToList();
            var userDtoList = AutoMapperHelper.MapToList<User, UserDto>(users);
            return userDtoList;
        }

        /// <summary>
        /// 查询所有user name
        /// </summary>
        /// <returns></returns>
        [HttpGet("allName")]
        public ActionResult<IEnumerable<string>> GetAllName()
        {
            List<string> blogs = _freesql.Select<User>().OrderByDescending(r => r.Name).ToList(x => x.Name);

            return blogs;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddUser([FromBody] UserDto userDto)
        {
            var user = AutoMapperHelper.MapTo<UserDto,User>(userDto);
            //user.CreateTime = DateTime.Now;
            var r = _freesql.Insert<User>(user).ExecuteIdentity();
            return Ok(r);
        }

    }

}