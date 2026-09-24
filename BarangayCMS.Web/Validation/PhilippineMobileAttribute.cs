using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace BarangayCMS.Web.Validation
{
    /// <summary>
    /// Validates a Philippine mobile number: exactly 11 digits starting with "09"
    /// (09XXXXXXXXX), digits only — matches the app's existing rule (^09\d{9}$).
    /// Empty is allowed here — pair with [Required] when mandatory. Server + client.
    /// Message is localized via SharedResource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PhilippineMobileAttribute : ValidationAttribute, IClientModelValidator
    {
        private const string MessageKey = "Val.Mobile.Format";
        private static readonly Regex Pattern = new(@"^09\d{9}$", RegexOptions.Compiled);

        public static bool IsValidMobile(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true; // Required handles emptiness
            return Pattern.IsMatch(value.Trim());
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var s = value as string;
            if (IsValidMobile(s)) return ValidationResult.Success;

            var message = Localize(validationContext.GetService<IStringLocalizer<SharedResource>>());
            return new ValidationResult(message, validationContext.MemberName is null
                ? null
                : new[] { validationContext.MemberName });
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            var localizer = context.ActionContext.HttpContext.RequestServices
                .GetService<IStringLocalizer<SharedResource>>();
            var message = Localize(localizer);

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-phmobile", message);
        }

        private static string Localize(IStringLocalizer<SharedResource>? localizer)
        {
            var localized = localizer?[MessageKey];
            return localized is { ResourceNotFound: false } ? localized.Value : "Mobile number must contain exactly 11 digits.";
        }

        private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (!attributes.ContainsKey(key)) attributes.Add(key, value);
        }
    }
}
