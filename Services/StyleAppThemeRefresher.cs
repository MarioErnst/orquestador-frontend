using System.Reflection;

namespace OrquestadorFrontend.Services;

// Generic workaround for the family of .NET MAUI bugs (dotnet/maui#6596,
// dotnet/maui#20243 and related) where an AppThemeBinding declared inside
// a Style Setter does not re-evaluate when Application.UserAppTheme
// changes at runtime. The Setter resolves the AppThemeBinding once during
// the first Style application; from then on, the BindableProperty holds a
// concrete colour and never refreshes, so cards, labels and other styled
// controls stay frozen in the previous palette after the user toggles
// Sistema / Claro / Oscuro.
//
// Strategy: walk a visual subtree, find every VisualElement that carries
// a Style, enumerate every Setter (including those inherited via
// BasedOn), and for any Setter whose Value is an AppThemeBinding read the
// Light or Dark slot directly and assign it to the BindableProperty via
// SetValue. The reassignment bypasses the broken binding and guarantees
// the resolved colour matches the requested theme.
//
// Why reflection: AppThemeBinding lives in Microsoft.Maui.Controls but is
// marked internal, so the type cannot be imported. The reflection handles
// are resolved once at class load and cached; if a future MAUI version
// removes or relocates the type the refresher silently no-ops and the
// other ThemeService passes still cover Shell chrome and page
// backgrounds.
//
// Scalability: the walker is intentionally schema-free. It does not
// enumerate Style keys, property names or element types. Adding a new
// theme-aware Style anywhere in the project requires zero changes here.
internal static class StyleAppThemeRefresher
{
    private static readonly Type? AppThemeBindingType =
        typeof(Application).Assembly.GetType("Microsoft.Maui.Controls.AppThemeBinding");

    private static readonly PropertyInfo? LightProperty =
        AppThemeBindingType?.GetProperty("Light", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly PropertyInfo? DarkProperty =
        AppThemeBindingType?.GetProperty("Dark", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly PropertyInfo? DefaultProperty =
        AppThemeBindingType?.GetProperty("Default", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly bool ReflectionReady =
        AppThemeBindingType is not null
        && LightProperty is not null
        && DarkProperty is not null;

    public static void Refresh(IVisualTreeElement root, AppTheme currentTheme)
    {
        if (!ReflectionReady)
        {
            return;
        }

        Walk(root, currentTheme);
    }

    private static void Walk(IVisualTreeElement element, AppTheme currentTheme)
    {
        if (element is VisualElement ve)
        {
            RefreshStyleSetters(ve, currentTheme);
        }

        foreach (var child in element.GetVisualChildren())
        {
            Walk(child, currentTheme);
        }
    }

    private static void RefreshStyleSetters(VisualElement element, AppTheme currentTheme)
    {
        var style = element.Style;
        if (style is null)
        {
            return;
        }

        // MAUI's Style precedence resolves more-derived Setters before
        // their BasedOn counterparts. The walker mirrors that order
        // (derived first) and tracks which properties have already been
        // processed so a base-style Setter never clobbers a value the
        // derived style already set.
        var alreadyApplied = new HashSet<BindableProperty>();

        foreach (var setter in EnumerateInheritedSetters(style))
        {
            if (setter.Property is not BindableProperty property)
            {
                continue;
            }

            if (!alreadyApplied.Add(property))
            {
                continue;
            }

            var value = setter.Value;
            if (value is null || value.GetType() != AppThemeBindingType)
            {
                continue;
            }

            var resolved = ResolveThemeValue(value, currentTheme);
            if (resolved is null)
            {
                continue;
            }

            // Kill the broken AppThemeBinding before assigning a local
            // value. If we only call SetValue, MAUI's broken binding
            // can still fire later in the same dispatch cycle and write
            // the stale colour back over the resolved one. Removing the
            // binding first ensures the local value wins definitively.
            // Future theme toggles and Shell navigations re-run this
            // walker, so the lack of a live binding is by design.
            element.RemoveBinding(property);
            element.SetValue(property, resolved);
        }
    }

    // Mirrors AppThemeBinding's own fallback ladder: pick the slot for
    // the current theme, fall back to Default, and only then fall back
    // to the opposite slot so the element keeps something usable rather
    // than reverting to the BindableProperty default.
    private static object? ResolveThemeValue(object themeBinding, AppTheme currentTheme)
    {
        var primary = currentTheme == AppTheme.Dark
            ? DarkProperty!.GetValue(themeBinding)
            : LightProperty!.GetValue(themeBinding);

        if (primary is not null)
        {
            return primary;
        }

        var defaultValue = DefaultProperty?.GetValue(themeBinding);
        if (defaultValue is not null)
        {
            return defaultValue;
        }

        return currentTheme == AppTheme.Dark
            ? LightProperty!.GetValue(themeBinding)
            : DarkProperty!.GetValue(themeBinding);
    }

    private static IEnumerable<Setter> EnumerateInheritedSetters(Style style)
    {
        var current = style;
        while (current is not null)
        {
            foreach (var setter in current.Setters)
            {
                yield return setter;
            }
            current = current.BasedOn;
        }
    }
}
