using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace iSchool.Infrastructure
{
    public class OrgUnitOfWork : UnitOfWork, IOrgUnitOfWork
    {

        public OrgUnitOfWork(string connectionString) : base(connectionString)
        {

        }


        public OrgUnitOfWork(string connectionString, string readConnnectionString) : base(connectionString, readConnnectionString)
        {

        }
        public override void CommitChanges()
        {
            base.CommitChanges();
        }


    }


    #region WX
    public class WXOrgUnitOfWork : UnitOfWork, IWXUnitOfWork
    {


        public WXOrgUnitOfWork(string connectionString) : base(connectionString)
        {

        }



       
    }
    #endregion

    #region Openid_WX
    public class Openid_WXOrgUnitOfWork : UnitOfWork, IOpenid_WXUnitOfWork
    {



        public Openid_WXOrgUnitOfWork(string connectionString) : base(connectionString)
        {
        }

    }
    #endregion
}
