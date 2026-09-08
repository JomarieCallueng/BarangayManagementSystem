using System.Text.RegularExpressions;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DTO;
using Microsoft.Extensions.Configuration;

namespace BarangayCMS.BLL.Services
{
    public class SemaphoreService : ISemaphoreService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // Endpoint ng Semaphore API
        private const string RequestUrl = "https://api.semaphore.co/api/v4/messages";

        public SemaphoreService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            if (!IsValidPhoneNumber(phoneNumber) || string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            // Kunin ang credentials mula sa configuration/environment (HINDI naka-hardcode).
            var apiKey = _configuration["Semaphore:ApiKey"];
            var senderName = _configuration["Semaphore:SenderName"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("[SMS ERROR]: Walang naka-configure na Semaphore API key.");
                return false;
            }

            // I-prepare ang Form parameters na kailangan ng Semaphore
            var values = new Dictionary<string, string>
            {
                { "apikey", apiKey },
                { "number", NormalizeNumber(phoneNumber) },
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
                var response = await _httpClient.PostAsync(RequestUrl, content);

                // Kapag HTTP 200 OK, ibig sabihin ay tinanggap ng Semaphore ang SMS request
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Hinuhuli ang error para hindi mag-crash ang application
                Console.WriteLine($"[SMS ERROR]: {ex.Message}");
                return false;
            }
        }

        public async Task<SmsSendResultDTO> SendBulkSmsAsync(IEnumerable<string> phoneNumbers, string message)
        {
            var result = new SmsSendResultDTO();

            if (string.IsNullOrWhiteSpace(message))
            {
                return result; // Walang mensahe — walang ipapadala (Status = Failed)
            }

            // I-linis: alisin ang null/blank, i-normalize, at alisin ang duplicates
            var cleaned = (phoneNumbers ?? Enumerable.Empty<string>())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct()
                .ToList();

            // Ihiwalay ang valid at invalid na numero
            var validNumbers = new List<string>();
            foreach (var number in cleaned)
            {
                if (IsValidPhoneNumber(number))
                {
                    validNumbers.Add(NormalizeNumber(number));
                }
                else
                {
                    result.InvalidNumbers.Add(number);
                }
            }

            result.TotalRecipients = validNumbers.Count;

            if (validNumbers.Count == 0)
            {
                return result; // Walang valid na numero (Status = Failed)
            }

            var apiKey = _configuration["Semaphore:ApiKey"];
            var senderName = _configuration["Semaphore:SenderName"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("[SMS ERROR]: Walang naka-configure na Semaphore API key.");
                result.FailedCount = validNumbers.Count;
                result.FailedNumbers.AddRange(validNumbers);
                return result;
            }

            // Ang Semaphore ay tumatanggap ng comma-separated numbers sa isang request,
            // ngunit para makakuha ng per-number status, isa-isa nating ipapadala nang safe.
            foreach (var number in validNumbers)
            {
                var values = new Dictionary<string, string>
                {
                    { "apikey", apiKey },
                    { "number", number },
                    { "message", message }
                };

                if (!string.IsNullOrEmpty(senderName))
                {
                    values.Add("sendername", senderName);
                }

                try
                {
                    var response = await _httpClient.PostAsync(RequestUrl, new FormUrlEncodedContent(values));
                    if (response.IsSuccessStatusCode)
                    {
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.FailedCount++;
                        result.FailedNumbers.Add(number);
                    }
                }
                catch (Exception ex)
                {
                    // Isahang failure — huwag itigil ang buong blast
                    Console.WriteLine($"[SMS ERROR] ({number}): {ex.Message}");
                    result.FailedCount++;
                    result.FailedNumbers.Add(number);
                }
            }

            return result;
        }

        public bool IsValidPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

            var trimmed = phoneNumber.Trim();

            // Tinatanggap: 09XXXXXXXXX (11 digits), 639XXXXXXXXX, o +639XXXXXXXXX
            return Regex.IsMatch(trimmed, @"^(09\d{9}|(\+?63)9\d{9})$");
        }

        /// <summary>
        /// Isinasa-standard ang numero papuntang 09XXXXXXXXX na format na tinatanggap ng Semaphore.
        /// </summary>
        private static string NormalizeNumber(string phoneNumber)
        {
            var digits = Regex.Replace(phoneNumber.Trim(), @"[^\d]", "");

            // 639XXXXXXXXX -> 09XXXXXXXXX
            if (digits.StartsWith("63") && digits.Length == 12)
            {
                return "0" + digits.Substring(2);
            }

            return digits;
        }
    }
}
