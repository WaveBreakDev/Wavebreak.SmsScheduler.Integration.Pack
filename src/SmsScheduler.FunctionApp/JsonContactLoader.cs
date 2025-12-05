using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Wavebreak.SmsScheduler.Integration.Pack
{
    public class JsonContactLoader : IContactLoader
    {
        public async Task<List<Contact>> LoadContactsAsync(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            var list = JsonSerializer.Deserialize<List<Contact>>(json);

            return list ?? new List<Contact>();
        }
    }
}
