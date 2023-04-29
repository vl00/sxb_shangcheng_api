namespace iSchool.Organization.Appliaction.Queries
{
    public class SearchBaseQueryModel
    {
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}