using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Services.Model
{
    public class SmtpSettings
    {
        public string SmtpServer { get; set; } = null!;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string From { get; set; } = null!;
    }
}
