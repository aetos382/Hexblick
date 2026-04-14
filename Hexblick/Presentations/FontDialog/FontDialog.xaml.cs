using System;
using System.Collections.Generic;

using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml.Controls;

using Windows.System.UserProfile;

using ObservableCollections;

using R3;

using ZLinq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Hexblick.Presentations;

public sealed partial class FontDialog :
    UserControl,
    IDisposable
{
    public sealed record FontInfo(
        string FamilyName);

    private readonly ObservableList<FontInfo>? _fontFamilies;

    private NotifyCollectionChangedSynchronizedViewList<FontInfo> Fonts { get; }

    private readonly CompositeDisposable _disposables = [];

    public FontDialog()
    {
        this.InitializeComponent();

        var fontSet = CanvasFontSet.GetSystemFontSet().AddTo(this._disposables);

        var fontFamiliesHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var languages = GlobalizationPreferences.Languages;

        foreach (var font in fontSet.Fonts)
        {
            var familyName = GetFontFamily(font.FamilyNames, languages);
            if (familyName is not null)
            {
                fontFamiliesHash.Add(familyName);
            }
        }

        var fontFamilies = new ObservableList<FontInfo>();

        foreach (var familyName in fontFamiliesHash.AsValueEnumerable().Order(StringComparer.OrdinalIgnoreCase))
        {
            fontFamilies.Add(new(familyName));
        }

        this._fontFamilies = fontFamilies;
        this.Fonts = this._fontFamilies.ToNotifyCollectionChangedSlim().AddTo(this._disposables);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposables.Dispose();
    }

    private static string? GetFontFamily(
        IReadOnlyDictionary<string, string> familyNames,
        IReadOnlyList<string> userLanguages)
    {
        var names = new Dictionary<string, string>(familyNames, StringComparer.OrdinalIgnoreCase);

        foreach (var language in userLanguages)
        {
            string? familyName;

            if (names.TryGetValue(language, out familyName))
            {
                return familyName;
            }

            var prefix = language.Split('-')[0];
            if (names.TryGetValue(prefix, out familyName))
            {
                return familyName;
            }

            var prefixMatched = names
                .AsValueEnumerable()
                .OrderBy(static x => x.Key, StringComparer.OrdinalIgnoreCase)
                .FirstOrNull(x => string.Equals(x.Key.Split('-')[0], prefix));

            if (prefixMatched is not null)
            {
                return prefixMatched.Value.Value;
            }
        }

        return null;
    }
}
