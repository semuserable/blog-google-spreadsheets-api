using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace Core
{
    class Program
    {
        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private const string SpreadsheetId = "1bSU7TCa8xOiBlaWf2qncewyMUclO1HzEyiSzAs0Tqqw";
        private const string GoogleCredentialsFileName = "google-credentials.json";
        private const string ReadRange = "A:B";
        private const string WriteRange = "A5:B5";
        
        static async Task Main(string[] args)
        {
            var serviceValues = GetSheetsService().Spreadsheets.Values;
            await ReadAsync(serviceValues);
            await WriteAsync(serviceValues);
            await ReadAsync(serviceValues);
        }

        private static SheetsService GetSheetsService()
        {            
            using (var stream =
                new FileStream(GoogleCredentialsFileName, FileMode.Open, FileAccess.Read))
            {
                var serviceInitializer = new BaseClientService.Initializer
                {
                    HttpClientInitializer = GoogleCredential.FromStream(stream).CreateScoped(Scopes)
                };
                return new SheetsService(serviceInitializer);
            }                        
        }

        private static async Task ReadAsync(SpreadsheetsResource.ValuesResource valuesResource)
        {
            var response = await valuesResource.Get(SpreadsheetId, ReadRange).ExecuteAsync();
            var values = response.Values;

            if (values == null || !values.Any())
            {
                Console.WriteLine("No data found.");
                return;
            }            

            var header = string.Join(" ", values.First().Select(r => r.ToString()));
            Console.WriteLine($"Header: {header}");
            
            foreach (var row in values.Skip(1))
            {
                var res = string.Join(" ", row.Select(r => r.ToString()));
                Console.WriteLine(res);
            }
        }

        private static async Task WriteAsync(SpreadsheetsResource.ValuesResource valuesResource)
        {            
            var valueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "stan", 18 } } };
            var update = valuesResource.Update(valueRange, SpreadsheetId, WriteRange);
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            var response = await update.ExecuteAsync();
            Console.WriteLine($"Updated rows: {response.UpdatedRows}");   
        }
    }
}
