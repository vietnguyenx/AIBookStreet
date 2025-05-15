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
            ImageMapping();
            InventoryMapping();
            PublisherMapping();
            StreetMapping();
            UserMapping();
            UserRoleMapping();
            UserStoreMapping();
            ZoneMapping();
            RoleMapping();
            SouvenirMapping();
            OrderMapping();
            OrderDetailMapping();
            StoreScheduleMapping();
            EventRegistrationMapping();
            TicketMapping();
            EventScheduleMapping();
        }
    }
}
