using System.ComponentModel.DataAnnotations;

namespace Cantina.Models.Requests
{
    /// <summary>
    /// Запрос на активацию аккаунта
    /// </summary>
    public class ActivationRequest
    {
        [EmailAddress]
        public string Email { get; set; }

        public string ActivationCode { get; set; }
    }
}
