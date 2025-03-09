using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Services.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IImageService
    {
        //Task<List<string>?> UploadImagesAsync(List<IFormFile> files);
        //string GetImageUrl(string imageName);
        //Task DeleteImageAsync(string imageName);
        Task<List<Image>?> AddImages(List<FileModel> models);
        Task<(long, Image?)> UpdateAnImage(Guid? id, FileModel model);
        Task<(long, Image?)> DeleteAnImage(Guid id);
        Task<Image?> GetAnImageById(Guid id);
        Task<List<Image>?> GetImagesByTypeAndEntityID(string? type, Guid? entityID);
    }
}
