namespace BarangayCMS.Web
{
    /// <summary>
    /// Marker type for the app's centralized, shared localization resources.
    /// Its strings live in /Resources/SharedResource.en.resx (default/fallback)
    /// and /Resources/SharedResource.fil.resx (Filipino). Inject
    /// <c>IStringLocalizer&lt;SharedResource&gt;</c> (aliased as <c>L</c> in the
    /// view imports) and reference keys such as <c>L["Nav.Dashboard"]</c>.
    /// Keys are organized by module: Nav.*, Common.*, Help.*, Validation.*, Errors.*.
    /// </summary>
    public class SharedResource
    {
    }
}
