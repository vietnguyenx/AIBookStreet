using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class SouvenirModel
    {
        public string SouvenirName { get; set; }
        public string? Description { get; set; }
        public IFormFile? BaseImgFile { get; set; }
        public List<IFormFile>? OtherImgFiles { get; set; }
        public decimal? Price { get; set; }
    }
}
