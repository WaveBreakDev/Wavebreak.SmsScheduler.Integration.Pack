using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wavebreak.SmsScheduler.Integration.Pack
{
    public interface IContactLoader
    {
        Task<List<Contact>> LoadContactsAsync(string filePath);
    }
}
