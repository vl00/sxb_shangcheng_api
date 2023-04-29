using iSchool.Domain;
using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace iSchool.Infrastructure
{
    public class UserUnitOfWork : UnitOfWork, IUserUnitOfWork
    {
        public UserUnitOfWork(string connectionString) : base(connectionString)
        {

        }
    }
}
