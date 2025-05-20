using AIBookStreet.Repositories.Data.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class UserModel : BaseModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public DateTime? DOB { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public Guid? RequestedRoleId { get; set; }
        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? AdditionalImageFiles { get; set; }

        public PublisherModel? Publisher { get; set; }

        public IList<UserRoleModel> UserRoles { get; set; }
        public IList<UserStoreModel> UserStores { get; set; }

        public IList<Image>? Images { get; set; }
    }
}
