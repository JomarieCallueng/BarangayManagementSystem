using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface ISemaphoreService
    {
        /// <summary>
        /// Magpadala ng SMS notification gamit ang Semaphore API.
        /// </summary>
        /// <param name="phoneNumber">Numero ng residente (ex: "09123456789" o "639123456789")</param>
        /// <param name="message">Mensahe na ipapadala</param>
        /// <returns>True kung matagumpay na naisend, False kung may error</returns>
        Task<bool> SendSmsAsync(string phoneNumber, string message);

        /// <summary>
        /// Magpadala ng iisang Emergency SMS blast sa maraming residente.
        /// Vina-validate ang bawat numero, hinahati ang invalid at valid,
        /// at hinuhuli ang mga API error para hindi mag-crash ang app.
        /// </summary>
        /// <param name="phoneNumbers">Listahan ng mga numero ng residente</param>
        /// <param name="message">Emergency message na ipapadala</param>
        /// <returns>Buod ng resulta (success/failed/invalid counts)</returns>
        Task<SmsSendResultDTO> SendBulkSmsAsync(IEnumerable<string> phoneNumbers, string message);

        /// <summary>
        /// Sinusuri kung valid ang format ng isang Philippine mobile number.
        /// Tinatanggap ang 09XXXXXXXXX o 639XXXXXXXXX (at +639...).
        /// </summary>
        bool IsValidPhoneNumber(string? phoneNumber);
    }
}