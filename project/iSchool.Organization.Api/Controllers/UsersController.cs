using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Organization.Api.Filters;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.User;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace iSchool.Organization.Api.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        ///// <summary>
        ///// 获取用户信息--我的【伪数据】
        ///// </summary>
        ///// <param name="userId">用户Id</param>
        ///// <returns></returns>
        //[HttpGet("{userId}")]
        //[ProducesResponseType(typeof(UserInfoByUserIdResponse), 200)]
        //[Obsolete]
        //public ResponseResult GetUserInfoById(Guid userId)
        //{
        //    var res = _mediator.Send(new UserInfoByUserIdQuery() { UserId = userId }).Result;            
        //    return ResponseResult.Success(res);
        //}



        ///// <summary>
        ///// 添加当前账号的收货地址
        ///// </summary>
        ///// <param name="me"></param>
        ///// <param name="dto"></param>
        ///// <returns></returns>
        //[Authorize, CheckBindMobile]
        //[HttpPost("recvaddress/add")]
        //[ProducesResponseType(typeof(RecvAddressDto), 200)]
        //[Obsolete]
        //public async Task<ResponseResult> AddRecvAddress([FromServices] IUserInfo me, RecvAddressDto dto)
        //{            
        //    var r = await _mediator.Send(new AddRecvAddressCommand { AddressDto = dto, UserId = me.UserId });
        //    return ResponseResult.Success(r);
        //}

        ///// <summary>
        ///// 删除当前账号的收货地址
        ///// </summary>
        ///// <param name="me"></param>
        ///// <param name="cmd"></param>
        ///// <returns></returns>
        //[Authorize, CheckBindMobile]
        //[HttpPost("recvaddress/del")]
        //[ProducesResponseType(typeof(bool), 200)]
        //[Obsolete]
        //public async Task<ResponseResult> DeleteRecvAddress([FromServices] IUserInfo me, DeleteRecvAddressCommand cmd)
        //{
        //    cmd.UserId = me.UserId;
        //    var r = await _mediator.Send(cmd);
        //    return ResponseResult.Success(r);
        //}

        ///// <summary>
        ///// 我的收货地址s列表
        ///// </summary>
        ///// <param name="me"></param>
        ///// <param name="pageIndex">第几页</param>
        ///// <param name="pageSize">页大小</param>
        ///// <returns></returns>
        //[Authorize]
        //[HttpGet("recvaddress/ls/my")]
        //[ProducesResponseType(typeof(RecvAddressPglistQueryResult), 200)]
        //[Obsolete]
        //public async Task<ResponseResult> ListMyRecvAddresses([FromServices] IUserInfo me, int pageIndex = 1, int pageSize = 10)
        //{
        //    var r = await _mediator.Send(new RecvAddressPglistQuery { UserId = me.UserId, PageIndex = pageIndex, PageSize = pageSize });
        //    return ResponseResult.Success(r);
        //}
    }
}
