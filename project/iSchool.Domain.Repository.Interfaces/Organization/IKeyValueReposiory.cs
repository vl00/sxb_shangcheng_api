using iSchool.Organization.Domain.Modles;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Repository.Interfaces
{
    public interface IKeyValueReposiory : IDependency
    {
        List<OrgSelectItemsKeyValues> GetSubjects(int type);
    }
}
