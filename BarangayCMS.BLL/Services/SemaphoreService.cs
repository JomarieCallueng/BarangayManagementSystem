using System.Text.Json;
using BarangayCMS.BLL.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BarangayCMS.BLL.Services
{
    public class SemaphoreService : ISemaphoreService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SemaphoreService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            // Kunin ang credentials mula sa appsettings.json
            var apiKey = _configuration["Semaphore:ApiKey"];
            var senderName = _configuration["Semaphore:SenderName"];

            // Endpoint ng Semaphore API
            var requestUrl = "https://api.semaphore.co/api/v4/messages";

            // I-prepare ang Form parameters na kailangan ng Semaphore
            var values = new Dictionary<string, string>
            {
                { "apikey", apiKey ?? "" },
                { "number", phoneNumber },
                { "message", message }
            };

            // Isama lamang ang sendername kung may approved custom sender name sa Semaphore
            if (!string.IsNullOrEmpty(senderName))
            {
                values.Add("sendername", senderName);
            }

            var content = new FormUrlEncodedContent(values);

            try
            {
                var response = await _httpClient.PostAsync(requestUrl, content);

                // Kapag HTTP 200 OK, ibig sabihin ay tinanggap ng Semaphore ang SMS request
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMS ERROR]: {ex.Message}");
                return false;
            }
        }
    }
}