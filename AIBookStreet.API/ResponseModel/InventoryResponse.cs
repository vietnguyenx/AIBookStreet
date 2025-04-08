using AIBookStreet.Services.Model;

namespace AIBookStreet.API.ResponseModel
{
    public class InventoryResponse
    {
        public Guid? EntityId { get; set; }
        public Guid? StoreId { get; set; }
        public BookModel? Book { get; set; }
        public StoreModel? Store { get; set; }
        public SouvenirModel? Souvenir { get; set; }
    }
}
