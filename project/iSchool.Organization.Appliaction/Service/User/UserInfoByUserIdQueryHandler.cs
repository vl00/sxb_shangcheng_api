using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.User
{
    public class UserInfoByUserIdQueryHandler : IRequestHandler<UserInfoByUserIdQuery, UserInfoByUserIdResponse>
    {
        public async Task<UserInfoByUserIdResponse> Handle(UserInfoByUserIdQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            #region 业务逻辑TODO

            #endregion

            var response = new UserInfoByUserIdResponse()
            {
                FollowCount = 100,
                LikeCount = 99,
                Phone = "18825114526",
                ProFile = "myPhoto.png",
                ReleaseCount = 98,
                ReplyCount = 97,
                UserId = new Guid()
            };
            return response;
        }
    }
}
