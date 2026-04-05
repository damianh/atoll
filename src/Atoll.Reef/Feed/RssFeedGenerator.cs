using Atoll.Reef.Components;
using Atoll.Reef.Configuration;
using System.Text;
using System.Xml;

namespace Atoll.Reef.Feed;

/// <summary>
/// Generates RSS 2.0 XML from a list of articles for use as a syndication feed.
/// </summary>
public static class RssFeedGenerator
{
    /// <summary>
    /// Generates a complete RSS 2.0 XML document string from the supplied articles.
    /// </summary>
    /// <param name="config">The site configuration providing title, description, and site URL.</param>
    /// <param name="articles">The articles to include as feed items.</param>
    /// <param name="basePath">
    /// The base URL path prefix for building item links (e.g. <c>"/blog"</c>).
    /// </param>
    /// <returns>A UTF-8 RSS 2.0 XML string ready to serve as <c>application/rss+xml</c>.</returns>
    public static string Generate(
        ReefConfig config,
        IReadOnlyList<ArticleListItem> articles,
        string basePath)
    {
        var siteUrl = config.SiteUrl.TrimEnd('/');
        var baseTrimmed = basePath.TrimEnd('/');

        var sb = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = false,
        };

        using (var writer = XmlWriter.Create(sb, settings))
        {
            writer.WriteStartDocument();

            writer.WriteStartElement("rss");
            writer.WriteAttributeString("version", "2.0");
            writer.WriteAttributeString("xmlns", "atom", null, "http://www.w3.org/2005/Atom");

            writer.WriteStartElement("channel");

            writer.WriteElementString("title", config.Title);
            writer.WriteElementString("link", siteUrl + baseTrimmed);
            writer.WriteElementString("description", config.Description);
            writer.WriteElementString("language", "en-us");
            writer.WriteElementString("generator", "Atoll.Reef");

            if (config.RssEnabled)
            {
                var feedUrl = $"{siteUrl}{baseTrimmed}/feed.xml";
                writer.WriteStartElement("atom", "link", "http://www.w3.org/2005/Atom");
                writer.WriteAttributeString("href", feedUrl);
                writer.WriteAttributeString("rel", "self");
                writer.WriteAttributeString("type", "application/rss+xml");
                writer.WriteEndElement();
            }

            foreach (var article in articles)
            {
                var articleUrl = $"{siteUrl}{baseTrimmed}/{article.Slug.TrimStart('/')}";

                writer.WriteStartElement("item");
                writer.WriteElementString("title", article.Title);
                writer.WriteElementString("link", articleUrl);
                writer.WriteElementString("guid", articleUrl);
                writer.WriteElementString("description", article.Description);
                writer.WriteElementString("pubDate", article.PubDate.ToUniversalTime().ToString("R"));

                if (!string.IsNullOrEmpty(article.Author))
                {
                    writer.WriteElementString("author", article.Author);
                }

                foreach (var tag in article.Tags)
                {
                    writer.WriteElementString("category", tag);
                }

                writer.WriteEndElement(); // item
            }

            writer.WriteEndElement(); // channel
            writer.WriteEndElement(); // rss
            writer.WriteEndDocument();
        }

        return sb.ToString();
    }
}
