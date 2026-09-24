using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace BarangayCMS.Web.Validation
{
    /// <summary>
    /// Validates a person-name field: letters (any language), spaces, hyphens,
    /// apostrophes and periods only, with at least one letter. Rejects digits and
    /// symbols like @ # $ % etc. Empty is allowed here — pair with [Required] when
    /// the field is mandatory. Works both server-side (IsValid) and client-side
    /// (emits unobtrusive data-* attributes). Message is localized via SharedResource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PersonNameAttribute : ValidationAttribute, IClientModelValidator
    {
        // Localization key (looked up in SharedResource.en/.fil.resx).
        private const string MessageKey = "Val.Name.Invalid";

        // Allowed set: unicode letters, space, hyphen, apostrophe, period.
        private static readonly Regex Allowed = new(@"^[\p{L} .'\-]+$", RegexOptions.Compiled);
        private static readonly Regex HasLetter = new(@"\p{L}", RegexOptions.Compiled);

        public static bool IsValidName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true; // Required handles emptiness
            var v = value.Trim();
            return Allowed.IsMatch(v) && HasLetter.IsMatch(v);
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var s = value as string;
            if (IsValidName(s)) return ValidationResult.Success;

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
            MergeAttribute(context.Attributes, "data-val-personname", message);
        }

        private static string Localize(IStringLocalizer<SharedResource>? localizer)
        {
            var localized = localizer?[MessageKey];
            return localized is { ResourceNotFound: false } ? localized.Value : "Please enter a valid name.";
        }

        private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (!attributes.ContainsKey(key)) attributes.Add(key, value);
        }
    }
}
