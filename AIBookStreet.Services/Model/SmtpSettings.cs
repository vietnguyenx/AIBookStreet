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
        public int Timeout { get; set; } = 30000; // 30 seconds default
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 2000; // 2 seconds delay between retries
    }
}
