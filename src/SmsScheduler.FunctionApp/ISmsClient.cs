using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wavebreak.SmsScheduler.Integration.Pack;

namespace Wavebreak.SmsScheduler.Integration.Pack
{
    public interface ISmsClient
    {
        Task<SmsSendResult> SendSmsAsync(Contact contact);
    }
}
