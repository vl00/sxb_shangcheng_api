using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure.Repositories
{
    public class SchoolRepository : ISchoolRepository
    {
        IRepository<School> _schoolRepository;

        public SchoolRepository(IRepository<School> schoolRepository, UnitOfWork unitOfWork)
        {
            _schoolRepository = schoolRepository;
        }

       

    }
}
