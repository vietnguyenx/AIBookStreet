using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Data;
using AIBookStreet.Repositories.Repositories.Base;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AIBookStreet.Repositories.Repositories.Repositories.Repository
{
    public class ImageRepository(BSDbContext context) : BaseRepository<Image>(context), IImageRepository
    {
        public async Task<Image?> GetByID(Guid? id)
        {
            var query = GetQueryable(at => at.Id == id);
            var image = await query.SingleOrDefaultAsync();

            return image;
        }
        public async Task<List<Image>?> GetByTypeAndEntityID(string? type, Guid? entityID)
        {
            var query = GetQueryable();
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(i => i.Type == type);
            }
            if (entityID != null)
            {
                query = query.Where(i => i.EntityId == entityID);
            }
            var images = await query.ToListAsync();
            return images;
        }
    }
}
