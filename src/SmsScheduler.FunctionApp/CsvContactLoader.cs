using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wavebreak.SmsScheduler.Integration.Pack
{
    public class CsvContactLoader : IContactLoader
    {
        public Task<List<Contact>> LoadContactsAsync(string filePath)
        {
            var lines = File.ReadAllLines(filePath);

            // Skip header
            return Task.FromResult(
                lines
                    .Skip(1)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(line =>
                    {
                        var parts = line.Split(',');

                        return new Contact
                        {
                            Name = parts.ElementAtOrDefault(0)?.Trim() ?? "",
                            Phone = parts.ElementAtOrDefault(1)?.Trim() ?? "",
                            Message = parts.ElementAtOrDefault(2)?.Trim() ?? ""
                        };
                    })
                    .ToList()
            );
        }
    }
}
