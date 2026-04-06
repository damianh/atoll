using System.Diagnostics.CodeAnalysis;

namespace Atoll.Lagoon.I18n;

/// <summary>
/// Provides pre-defined <see cref="UiTranslations"/> instances for common languages.
/// Use <see cref="ForLanguage"/> to look up translations by BCP-47 language tag.
/// </summary>
public static class BuiltInTranslations
{
    /// <summary>Gets the English (en) translations (identical to <see cref="UiTranslations.Default"/>).</summary>
    public static UiTranslations English { get; } = UiTranslations.Default;

    /// <summary>Gets the French (fr) translations.</summary>
    public static UiTranslations French { get; } = new()
    {
        SkipLinkLabel = "Aller au contenu",
        SearchLabel = "Rechercher",
        SearchPlaceholder = "Rechercher...",
        SearchDialogLabel = "Rechercher dans la documentation",
        SearchCloseLabel = "Fermer la recherche",
        SearchResultsLabel = "Résultats de recherche",
        SearchNoResults = "Aucun résultat trouvé.",
        ThemeToggleLabel = "Changer de thème",
        ThemeSwitchToLight = "Passer au thème clair",
        ThemeSwitchToDark = "Passer au thème sombre",
        SidebarNavLabel = "Principal",
        SiteNavigationLabel = "Navigation du site",
        MobileNavOpenLabel = "Ouvrir la navigation",
        BreadcrumbsLabel = "Fil d'Ariane",
        PaginationLabel = "Pagination",
        PaginationPrevious = "Précédent",
        PaginationNext = "Suivant",
        TocLabel = "Sur cette page",
        BuiltWithLabel = "Créé avec",
        LanguageSelectLabel = "Sélectionner la langue",
        UntranslatedContentNotice = "Cette page n'a pas encore été traduite.",
        VersionSelectLabel = "Sélectionner la version",
        OutdatedVersionNotice = "Vous consultez la documentation pour une version antérieure.",
        OutdatedVersionLinkText = "Voir la dernière version",
    };

    /// <summary>Gets the Spanish (es) translations — Latin American Spanish.</summary>
    public static UiTranslations Spanish { get; } = new()
    {
        SkipLinkLabel = "Ir al contenido",
        SearchLabel = "Buscar",
        SearchPlaceholder = "Buscar...",
        SearchDialogLabel = "Buscar en la documentación",
        SearchCloseLabel = "Cerrar búsqueda",
        SearchResultsLabel = "Resultados de búsqueda",
        SearchNoResults = "No se encontraron resultados.",
        ThemeToggleLabel = "Cambiar tema",
        ThemeSwitchToLight = "Cambiar a tema claro",
        ThemeSwitchToDark = "Cambiar a tema oscuro",
        SidebarNavLabel = "Principal",
        SiteNavigationLabel = "Navegación del sitio",
        MobileNavOpenLabel = "Abrir navegación",
        BreadcrumbsLabel = "Migas de pan",
        PaginationLabel = "Paginación",
        PaginationPrevious = "Anterior",
        PaginationNext = "Siguiente",
        TocLabel = "En esta página",
        BuiltWithLabel = "Creado con",
        LanguageSelectLabel = "Seleccionar idioma",
        UntranslatedContentNotice = "Esta página aún no ha sido traducida.",
        VersionSelectLabel = "Seleccionar versión",
        OutdatedVersionNotice = "Estás viendo la documentación de una versión anterior.",
        OutdatedVersionLinkText = "Ver la última versión",
    };

    /// <summary>Gets the German (de) translations.</summary>
    public static UiTranslations German { get; } = new()
    {
        SkipLinkLabel = "Zum Inhalt springen",
        SearchLabel = "Suchen",
        SearchPlaceholder = "Suchen...",
        SearchDialogLabel = "Dokumentation durchsuchen",
        SearchCloseLabel = "Suche schließen",
        SearchResultsLabel = "Suchergebnisse",
        SearchNoResults = "Keine Ergebnisse gefunden.",
        ThemeToggleLabel = "Design wechseln",
        ThemeSwitchToLight = "Zum hellen Design wechseln",
        ThemeSwitchToDark = "Zum dunklen Design wechseln",
        SidebarNavLabel = "Hauptnavigation",
        SiteNavigationLabel = "Seitennavigation",
        MobileNavOpenLabel = "Navigation öffnen",
        BreadcrumbsLabel = "Brotkrümelnavigation",
        PaginationLabel = "Seitennavigation",
        PaginationPrevious = "Zurück",
        PaginationNext = "Weiter",
        TocLabel = "Auf dieser Seite",
        BuiltWithLabel = "Erstellt mit",
        LanguageSelectLabel = "Sprache auswählen",
        UntranslatedContentNotice = "Diese Seite wurde noch nicht übersetzt.",
        VersionSelectLabel = "Version auswählen",
        OutdatedVersionNotice = "Sie sehen die Dokumentation für eine ältere Version.",
        OutdatedVersionLinkText = "Neueste Version anzeigen",
    };

    /// <summary>Gets the Japanese (ja) translations.</summary>
    public static UiTranslations Japanese { get; } = new()
    {
        SkipLinkLabel = "コンテンツへスキップ",
        SearchLabel = "検索",
        SearchPlaceholder = "検索...",
        SearchDialogLabel = "ドキュメントを検索",
        SearchCloseLabel = "検索を閉じる",
        SearchResultsLabel = "検索結果",
        SearchNoResults = "結果が見つかりませんでした。",
        ThemeToggleLabel = "テーマ切り替え",
        ThemeSwitchToLight = "ライトテーマに切り替え",
        ThemeSwitchToDark = "ダークテーマに切り替え",
        SidebarNavLabel = "メインナビゲーション",
        SiteNavigationLabel = "サイトナビゲーション",
        MobileNavOpenLabel = "ナビゲーションを開く",
        BreadcrumbsLabel = "パンくずリスト",
        PaginationLabel = "ページネーション",
        PaginationPrevious = "前へ",
        PaginationNext = "次へ",
        TocLabel = "このページの内容",
        BuiltWithLabel = "構築ツール",
        LanguageSelectLabel = "言語を選択",
        UntranslatedContentNotice = "このページはまだ翻訳されていません。",
        VersionSelectLabel = "バージョンを選択",
        OutdatedVersionNotice = "古いバージョンのドキュメントを表示しています。",
        OutdatedVersionLinkText = "最新バージョンを表示",
    };

    /// <summary>Gets the Arabic (ar) translations. Note: Arabic is a right-to-left (RTL) language.</summary>
    public static UiTranslations Arabic { get; } = new()
    {
        SkipLinkLabel = "انتقل إلى المحتوى",
        SearchLabel = "بحث",
        SearchPlaceholder = "بحث...",
        SearchDialogLabel = "البحث في الوثائق",
        SearchCloseLabel = "إغلاق البحث",
        SearchResultsLabel = "نتائج البحث",
        SearchNoResults = "لم يتم العثور على نتائج.",
        ThemeToggleLabel = "تبديل السمة",
        ThemeSwitchToLight = "التبديل إلى السمة الفاتحة",
        ThemeSwitchToDark = "التبديل إلى السمة الداكنة",
        SidebarNavLabel = "الرئيسي",
        SiteNavigationLabel = "التنقل في الموقع",
        MobileNavOpenLabel = "فتح التنقل",
        BreadcrumbsLabel = "مسار التنقل",
        PaginationLabel = "ترقيم الصفحات",
        PaginationPrevious = "السابق",
        PaginationNext = "التالي",
        TocLabel = "في هذه الصفحة",
        BuiltWithLabel = "أنشئ بواسطة",
        LanguageSelectLabel = "اختر اللغة",
        UntranslatedContentNotice = "لم تتم ترجمة هذه الصفحة بعد.",
        VersionSelectLabel = "اختر الإصدار",
        OutdatedVersionNotice = "أنت تشاهد وثائق إصدار قديم.",
        OutdatedVersionLinkText = "عرض أحدث إصدار",
    };

    /// <summary>Gets the Chinese Simplified (zh-CN) translations.</summary>
    public static UiTranslations ChineseSimplified { get; } = new()
    {
        SkipLinkLabel = "跳转到内容",
        SearchLabel = "搜索",
        SearchPlaceholder = "搜索...",
        SearchDialogLabel = "搜索文档",
        SearchCloseLabel = "关闭搜索",
        SearchResultsLabel = "搜索结果",
        SearchNoResults = "未找到结果。",
        ThemeToggleLabel = "切换主题",
        ThemeSwitchToLight = "切换到浅色主题",
        ThemeSwitchToDark = "切换到深色主题",
        SidebarNavLabel = "主导航",
        SiteNavigationLabel = "站点导航",
        MobileNavOpenLabel = "打开导航",
        BreadcrumbsLabel = "面包屑导航",
        PaginationLabel = "分页",
        PaginationPrevious = "上一页",
        PaginationNext = "下一页",
        TocLabel = "本页内容",
        BuiltWithLabel = "构建工具",
        LanguageSelectLabel = "选择语言",
        UntranslatedContentNotice = "此页面尚未翻译。",
        VersionSelectLabel = "选择版本",
        OutdatedVersionNotice = "您正在查看旧版本的文档。",
        OutdatedVersionLinkText = "查看最新版本",
    };

    /// <summary>Gets the Brazilian Portuguese (pt-BR) translations.</summary>
    public static UiTranslations Portuguese { get; } = new()
    {
        SkipLinkLabel = "Pular para o conteúdo",
        SearchLabel = "Pesquisar",
        SearchPlaceholder = "Pesquisar...",
        SearchDialogLabel = "Pesquisar na documentação",
        SearchCloseLabel = "Fechar pesquisa",
        SearchResultsLabel = "Resultados da pesquisa",
        SearchNoResults = "Nenhum resultado encontrado.",
        ThemeToggleLabel = "Alternar tema",
        ThemeSwitchToLight = "Mudar para tema claro",
        ThemeSwitchToDark = "Mudar para tema escuro",
        SidebarNavLabel = "Principal",
        SiteNavigationLabel = "Navegação do site",
        MobileNavOpenLabel = "Abrir navegação",
        BreadcrumbsLabel = "Trilha de navegação",
        PaginationLabel = "Paginação",
        PaginationPrevious = "Anterior",
        PaginationNext = "Próximo",
        TocLabel = "Nesta página",
        BuiltWithLabel = "Criado com",
        LanguageSelectLabel = "Selecionar idioma",
        UntranslatedContentNotice = "Esta página ainda não foi traduzida.",
        VersionSelectLabel = "Selecionar versão",
        OutdatedVersionNotice = "Você está visualizando a documentação de uma versão mais antiga.",
        OutdatedVersionLinkText = "Ver versão mais recente",
    };

    /// <summary>
    /// Gets the built-in translations for the given BCP-47 language tag,
    /// or <c>null</c> if no built-in translation exists.
    /// Case-insensitive lookup. Matches on primary subtag too (e.g., "fr-CA" matches French).
    /// </summary>
    /// <param name="lang">A BCP-47 language tag (e.g., "fr", "fr-CA", "zh-CN").</param>
    [return: MaybeNull]
    public static UiTranslations? ForLanguage(string lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return null;
        }

        // Try exact match first (case-insensitive).
        if (TryGetExact(lang, out var result))
        {
            return result;
        }

        // Fall back to primary subtag match (e.g., "fr-CA" → "fr").
        var hyphenIndex = lang.IndexOf('-');
        if (hyphenIndex > 0)
        {
            var primarySubtag = lang[..hyphenIndex];
            if (TryGetExact(primarySubtag, out result))
            {
                return result;
            }
        }

        return null;
    }

    private static bool TryGetExact(string lang, [MaybeNullWhen(false)] out UiTranslations result)
    {
        if (lang.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            result = English;
            return true;
        }

        if (lang.Equals("fr", StringComparison.OrdinalIgnoreCase))
        {
            result = French;
            return true;
        }

        if (lang.Equals("es", StringComparison.OrdinalIgnoreCase))
        {
            result = Spanish;
            return true;
        }

        if (lang.Equals("de", StringComparison.OrdinalIgnoreCase))
        {
            result = German;
            return true;
        }

        if (lang.Equals("ja", StringComparison.OrdinalIgnoreCase))
        {
            result = Japanese;
            return true;
        }

        if (lang.Equals("ar", StringComparison.OrdinalIgnoreCase))
        {
            result = Arabic;
            return true;
        }

        if (lang.Equals("zh-CN", StringComparison.OrdinalIgnoreCase)
            || lang.Equals("zh", StringComparison.OrdinalIgnoreCase))
        {
            result = ChineseSimplified;
            return true;
        }

        if (lang.Equals("pt-BR", StringComparison.OrdinalIgnoreCase)
            || lang.Equals("pt", StringComparison.OrdinalIgnoreCase))
        {
            result = Portuguese;
            return true;
        }

        result = null;
        return false;
    }
}
