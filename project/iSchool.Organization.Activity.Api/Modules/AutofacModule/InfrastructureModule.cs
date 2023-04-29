using Autofac;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Repositories;
using iSchool.Infrastructure.Repositories.Organization;
using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Organization.Activity.Api.Modules.AutofacModule
{
    public class InfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public InfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(OrgBaseRepository<>))
             .As(typeof(IBaseRepository<>))
             .Named("OrgBaseRepository", typeof(IBaseRepository<>))
             .InstancePerLifetimeScope();

            //main repository
            builder.RegisterGeneric(typeof(OrgBaseRepository<>))
             .As(typeof(IRepository<>))
             .InstancePerLifetimeScope();

            builder.RegisterType<OrgUnitOfWork>()
               .As<IOrgUnitOfWork>()
               .WithParameter("connectionString", _databaseConnectionString)
               .InstancePerLifetimeScope();
        }
    }


    #region WX
    public class WXInfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public WXInfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {              
            builder.RegisterGeneric(typeof(WXBaseRepository<>))
                .As(typeof(IBaseRepository<>))
                .Named("WXBaseRepository", typeof(IBaseRepository<>))
                .InstancePerLifetimeScope();

            builder.RegisterType<WXOrgUnitOfWork>()
               .As<IWXUnitOfWork>()
               .WithParameter("connectionString", _databaseConnectionString)
               .InstancePerLifetimeScope();
        }
    }
    #endregion

    #region Openid_WX
    public class Openid_WXInfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public Openid_WXInfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {            
            builder.RegisterGeneric(typeof(Openid_WXBaseRepository<>))
                .As(typeof(IBaseRepository<>))
                .Named("Openid_WXBaseRepository", typeof(IBaseRepository<>))
                .InstancePerLifetimeScope();

            builder.RegisterType<Openid_WXOrgUnitOfWork>()
               .As<IOpenid_WXUnitOfWork>()
               .WithParameter("connectionString", _databaseConnectionString)
               .InstancePerLifetimeScope();
        }
    }
    #endregion

    #region 虎叔叔用户中心
    public class UserInfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public UserInfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterGeneric(typeof(UserBaseRepository<>)).As(typeof(IBaseRepository<>))
            //    .Named("UserBaseRepository", typeof(IBaseRepository<>))
            //    .InstancePerLifetimeScope();

            builder.RegisterType<UserUnitOfWork>().As(typeof(IUserUnitOfWork))
                .WithParameter("connectionString", _databaseConnectionString)
                .InstancePerLifetimeScope();
        }
    }
    #endregion
}
