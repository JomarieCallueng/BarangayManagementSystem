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
    }
}