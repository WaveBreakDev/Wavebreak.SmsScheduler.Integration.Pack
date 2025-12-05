using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wavebreak.SmsScheduler.Integration.Pack
{
    public class SmsSendResult
    {
        public bool Success { get; set; }
        public string? ProviderMessageId { get; set; }
        public string? Error {  get; set; }

    }
}
