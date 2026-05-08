using System;
using System.Collections.Generic;

using Microsoft.Graphics.Canvas.Text;
using Microsoft.Windows.Globalization;

using ObservableCollections;

using R3;

using ZLinq;

namespace Hexblick.Presentations;

internal sealed record FontInfo(string FamilyName);

internal sealed partial class FontDialog :
    IDisposable
{
    private readonly ObservableList<FontInfo>? _fontFamilies;

    private NotifyCollectionChangedSynchronizedViewList<FontInfo> Fonts { get; }

    private readonly CompositeDisposable _disposables = [];

    public FontDialog()
    {
        this.InitializeComponent();

        var fontSet = CanvasFontSet.GetSystemFontSet().AddTo(this._disposables);

        var fontFamiliesHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var languages = ApplicationLanguages.Languages;

        foreach (var font in fontSet.Fonts)
        {
            if (!font.IsMonospaced || font.IsSymbolFont)
            {
                continue;
            }

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
                .FirstOrNull(x => string.Equals(x.Key.Split('-')[0], prefix, StringComparison.OrdinalIgnoreCase));

            if (prefixMatched is not null)
            {
                return prefixMatched.Value.Value;
            }
        }

        return null;
    }
}
