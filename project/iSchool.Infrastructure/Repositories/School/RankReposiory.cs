using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using Dapper;
using System.Text;

namespace iSchool.Infrastructure.Repositories
{
    public class RankReposiory : IRankReposiory
    {
        private IRepository<RankingList> _rankingListRepository;
        private UnitOfWork UnitOfWork { get; set; }



        public RankReposiory(IRepository<RankingList> rankingListRepository, IUnitOfWork unitOfWork)
        {
            this._rankingListRepository = rankingListRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="rankId"></param>
        /// <returns></returns>
        public int SortRank(Guid sid, Guid rankId, double Placing, bool isJux)
        {
            var list = _rankingListRepository
                .GetAll(p => p.IsValid && p.RankNameId == rankId)
                .OrderBy(p => p.Placing);
            IEnumerable<RankingList> other = new List<RankingList>();

            var rank = list.FirstOrDefault(p => p.SchoolId == sid);

            try
            {
                //事务
                UnitOfWork.BeginTransaction();

                //如果不存在此学校
                if (rank == null)
                {
                    other = list.Where(p => p.Placing < Placing);
                    //如果不并列排名
                    if (!isJux)
                        other = other.Union(list.Where(p => p.Placing == Placing));
                    other.ToList().ForEach(p => { p.Placing += 1; p.ModifyDateTime = DateTime.Now; });
                    //添加当前学校
                    _rankingListRepository.Insert(new RankingList
                    {
                        Placing = Convert.ToInt16(Placing),
                        SchoolId = sid,
                        RankNameId = rankId,
                    });
                }
                //存在此学校
                else
                {
                    if (rank.Placing == Placing)
                        return 1;
                    rank.Placing = Convert.ToInt16(Placing);
                    rank.ModifyDateTime = DateTime.Now;
                    _rankingListRepository.Update(rank);


                    #region 重排其他学校
                    //上移
                    if (rank.Placing > Placing)
                    {
                        //旧数据是存在并列排名中
                        if (list.Count(p => p.Placing == rank.Placing) > 1)
                        {

                            //不并存
                            if (!isJux)
                            {
                                other = list
                                    .Where(p => p.Placing <= Placing && p.SchoolId != sid)
                                    .ToList();

                            }
                        }
                        //旧数据是不存在并列排名中
                        else
                        {
                            //是否并存
                            if (isJux)
                            {
                                other = list
                                    .Where(p => p.Placing < rank.Placing
                                    && p.Placing > Placing && p.SchoolId != sid)
                                    .ToList();

                            }
                            else
                            {
                                other = list
                                   .Where(p => p.Placing >= Placing && p.SchoolId != sid)
                                   .ToList();

                            }
                        }
                        //排序
                        foreach (var p in other)
                        {
                            p.Placing += 1;
                            p.ModifyDateTime = DateTime.Now;
                        }
                    }


                    //下移
                    if (rank.Placing < Placing)
                    {
                        if (list.Count(p => p.Placing == rank.Placing) > 1)
                        {
                            if (!isJux)
                            {
                                other = list.Where(p => p.Placing >= Placing && p.SchoolId != sid)
                                    .ToList();
                            }
                        }
                        //旧数据是不存在并列排名
                        else
                        {
                            //是否并存
                            if (isJux)
                            {
                                other = list.Where(p => p.Placing > rank.Placing
                                  && p.Placing < Placing && p.SchoolId != sid)
                                  .ToList();
                            }
                            else
                            {
                                other = list
                                    .Where(p => p.Placing <= Placing && p.SchoolId != sid)
                                    .ToList();
                            }
                        }
                        //排序
                        foreach (var item in other)
                        {
                            item.Placing -= 1;
                            item.ModifyDateTime = DateTime.Now;
                        }
                    }
                }
                #endregion

                //批量更新
                _rankingListRepository.BatchUpdate(other.AsList());
                UnitOfWork.CommitChanges();
                return 1;
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                return 0;
            }
        }

        /// <summary>
        ///删除排行榜的学校
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="rankId"></param>
        /// <returns></returns>
        public int DelRank(Guid sid, Guid rankId)
        {
            var list = _rankingListRepository
                 .GetAll(p => p.IsValid && p.RankNameId == rankId)
                 .OrderBy(p => p.Placing);
            var rank = list.FirstOrDefault(p => p.SchoolId == sid);
            if (rank == null)
            {
                return 1;
            }
            else
            {
                try
                {
                    //删除排行版中的学校
                    //并进行排序
                    UnitOfWork.BeginTransaction();
                    rank.IsValid = false;
                    rank.ModifyDateTime = DateTime.Now;
                    //删除排行版
                    _rankingListRepository.Update(rank);
                    //重新排序
                    if (list.Count(p => p.Placing == rank.Placing) == 1)
                    {
                        var other = list.Where(p => p.Placing > rank.Placing).ToList();
                        other.ForEach(p =>
                        {
                            p.Placing -= 1;
                            p.ModifyDateTime = DateTime.Now;
                        });
                        _rankingListRepository.BatchUpdate(other);
                    }
                    UnitOfWork.CommitChanges();
                    return 1;
                }
                catch (Exception)
                {
                    UnitOfWork.Rollback();
                    return 0;
                }
            }

        }
    }
}
