using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Services.Interface
{
    public interface IQRGeneratorService
    {
        Task<int> SendEmail(string email);
        int GenerateQRCode(string name, int age);
        int GenerateBarCode(string infor);
    }
}
