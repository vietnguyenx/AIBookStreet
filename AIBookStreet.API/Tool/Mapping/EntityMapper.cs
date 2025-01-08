using AutoMapper;


namespace AIBookStreet.API.Tool.Mapping
{
    public partial class EntityMapper
    {
        public EntityMapper()
        {
            AuthorMapping();
            BookMapping();
            BookAuthorMapping();
            BookCategoryMapping();
            BookStoreMapping();
            CategoryMapping();
            EventMapping();
            InventoryMapping();
            PublisherMapping();
            StreetMapping();
            UserMapping();
            UserRoleMapping();
            ZoneMapping();  
        }
    }
}
