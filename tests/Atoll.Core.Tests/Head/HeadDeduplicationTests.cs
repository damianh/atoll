using Atoll.Core.Head;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Head;

public sealed class HeadDeduplicationTests
{
    // ── Title deduplication ──

    [Fact]
    public void TitleElementsShouldAlwaysProduceSameKey()
    {
        var a = new HeadElement("title") { Content = "Page A" };
        var b = new HeadElement("title") { Content = "Page B" };

        HeadDeduplicator.GenerateKey(a).ShouldBe("title");
        HeadDeduplicator.GenerateKey(b).ShouldBe("title");
        HeadDeduplicator.GenerateKey(a).ShouldBe(HeadDeduplicator.GenerateKey(b));
    }

    // ── Meta deduplication ──

    [Fact]
    public void MetaWithSameNameShouldProduceSameKey()
    {
        var a = new HeadElement("meta")
            .SetAttribute("name", "description")
            .SetAttribute("content", "Page A description");
        var b = new HeadElement("meta")
            .SetAttribute("name", "description")
            .SetAttribute("content", "Page B description");

        HeadDeduplicator.GenerateKey(a).ShouldBe(HeadDeduplicator.GenerateKey(b));
    }

    [Fact]
    public void MetaWithDifferentNameShouldProduceDifferentKeys()
    {
        var description = new HeadElement("meta")
            .SetAttribute("name", "description")
            .SetAttribute("content", "A page");
        var keywords = new HeadElement("meta")
            .SetAttribute("name", "keywords")
            .SetAttribute("content", "some keywords");

        HeadDeduplicator.GenerateKey(description)
            .ShouldNotBe(HeadDeduplicator.GenerateKey(keywords));
    }

    [Fact]
    public void MetaWithPropertyShouldKeyByProperty()
    {
        var a = new HeadElement("meta")
            .SetAttribute("property", "og:title")
            .SetAttribute("content", "Title A");
        var b = new HeadElement("meta")
            .SetAttribute("property", "og:title")
            .SetAttribute("content", "Title B");

        HeadDeduplicator.GenerateKey(a).ShouldBe("meta:property:og:title");
        HeadDeduplicator.GenerateKey(a).ShouldBe(HeadDeduplicator.GenerateKey(b));
    }

    [Fact]
    public void MetaWithHttpEquivShouldKeyByHttpEquiv()
    {
        var a = new HeadElement("meta")
            .SetAttribute("http-equiv", "content-type")
            .SetAttribute("content", "text/html; charset=utf-8");

        HeadDeduplicator.GenerateKey(a).ShouldBe("meta:http-equiv:content-type");
    }

    [Fact]
    public void MetaWithCharsetShouldProduceFixedKey()
    {
        var a = new HeadElement("meta")
            .SetAttribute("charset", "utf-8");

        HeadDeduplicator.GenerateKey(a).ShouldBe("meta:charset");
    }

    // ── Link deduplication ──

    [Fact]
    public void LinkWithSameRelAndHrefShouldProduceSameKey()
    {
        var a = new HeadElement("link")
            .SetAttribute("rel", "stylesheet")
            .SetAttribute("href", "/css/main.css");
        var b = new HeadElement("link")
            .SetAttribute("href", "/css/main.css")
            .SetAttribute("rel", "stylesheet");

        HeadDeduplicator.GenerateKey(a).ShouldBe(HeadDeduplicator.GenerateKey(b));
    }

    [Fact]
    public void LinkWithDifferentHrefShouldProduceDifferentKeys()
    {
        var a = new HeadElement("link")
            .SetAttribute("rel", "stylesheet")
            .SetAttribute("href", "/css/a.css");
        var b = new HeadElement("link")
            .SetAttribute("rel", "stylesheet")
            .SetAttribute("href", "/css/b.css");

        HeadDeduplicator.GenerateKey(a)
            .ShouldNotBe(HeadDeduplicator.GenerateKey(b));
    }

    [Fact]
    public void LinkStylesheetKeyShouldMatchExpectedFormat()
    {
        var element = new HeadElement("link")
            .SetAttribute("rel", "stylesheet")
            .SetAttribute("href", "/css/main.css");

        HeadDeduplicator.GenerateKey(element).ShouldBe("link:stylesheet:/css/main.css");
    }

    // ── Attribute order independence ──

    [Fact]
    public void GenericElementsShouldProduceSameKeyRegardlessOfAttributeOrder()
    {
        var a = new HeadElement("base")
            .SetAttribute("href", "/")
            .SetAttribute("target", "_blank");
        var b = new HeadElement("base")
            .SetAttribute("target", "_blank")
            .SetAttribute("href", "/");

        HeadDeduplicator.GenerateKey(a).ShouldBe(HeadDeduplicator.GenerateKey(b));
    }

    [Fact]
    public void GenericElementWithContentShouldIncludeContentInKey()
    {
        var a = new HeadElement("style") { Content = ".card { color: red; }" };
        var b = new HeadElement("style") { Content = ".card { color: blue; }" };

        HeadDeduplicator.GenerateKey(a)
            .ShouldNotBe(HeadDeduplicator.GenerateKey(b));
    }

    [Fact]
    public void GenericElementWithBooleanAttributeShouldIncludeItInKey()
    {
        var a = new HeadElement("script")
            .SetAttribute("src", "/js/app.js")
            .SetAttribute("defer", null);
        var b = new HeadElement("script")
            .SetAttribute("src", "/js/app.js");

        HeadDeduplicator.GenerateKey(a)
            .ShouldNotBe(HeadDeduplicator.GenerateKey(b));
    }

    // ── Manager integration ──

    [Fact]
    public void ManagerShouldDeduplicateSameStylesheet()
    {
        var manager = new HeadManager();

        var a = new HeadElement("link")
            .SetAttribute("rel", "stylesheet")
            .SetAttribute("href", "/css/main.css");
        var b = new HeadElement("link")
            .SetAttribute("href", "/css/main.css")
            .SetAttribute("rel", "stylesheet");

        manager.Add(a).ShouldBeTrue();
        manager.Add(b).ShouldBeFalse();
        manager.Count.ShouldBe(1);
    }

    [Fact]
    public void ManagerShouldDeduplicateSameMetaDescription()
    {
        var manager = new HeadManager();

        var a = new HeadElement("meta")
            .SetAttribute("name", "description")
            .SetAttribute("content", "First description");
        var b = new HeadElement("meta")
            .SetAttribute("name", "description")
            .SetAttribute("content", "Second description");

        manager.Add(a).ShouldBeTrue();
        manager.Add(b).ShouldBeFalse();
        manager.Count.ShouldBe(1);
    }

    [Fact]
    public void ManagerShouldDeduplicateTitleElements()
    {
        var manager = new HeadManager();

        var a = new HeadElement("title") { Content = "First Title" };
        var b = new HeadElement("title") { Content = "Second Title" };

        manager.Add(a).ShouldBeTrue();
        manager.Add(b).ShouldBeFalse();
        manager.Count.ShouldBe(1);
    }

    [Fact]
    public void ManagerShouldAllowDifferentElementTypes()
    {
        var manager = new HeadManager();

        manager.Add(new HeadElement("title") { Content = "My Page" });
        manager.Add(
            new HeadElement("meta")
                .SetAttribute("name", "description")
                .SetAttribute("content", "A great page"));
        manager.Add(
            new HeadElement("link")
                .SetAttribute("rel", "stylesheet")
                .SetAttribute("href", "/css/main.css"));

        manager.Count.ShouldBe(3);
    }

    [Fact]
    public async Task ManagerShouldRenderAllHeadContent()
    {
        var manager = new HeadManager();
        manager.Add(
            new HeadElement("link")
                .SetAttribute("rel", "stylesheet")
                .SetAttribute("href", "/css/main.css"));
        manager.Add(
            new HeadElement("meta")
                .SetAttribute("name", "description")
                .SetAttribute("content", "A test page"));

        var dest = new Atoll.Core.Rendering.StringRenderDestination();
        await manager.RenderAllHeadContentAsync(dest);

        var output = dest.GetOutput();
        output.ShouldContain("<link rel=\"stylesheet\" href=\"/css/main.css\">");
        output.ShouldContain("<meta name=\"description\" content=\"A test page\">");
    }

    [Fact]
    public async Task ManagerShouldRenderVoidElementsAsSelfClosing()
    {
        var manager = new HeadManager();
        manager.Add(
            new HeadElement("meta")
                .SetAttribute("charset", "utf-8"));

        var dest = new Atoll.Core.Rendering.StringRenderDestination();
        await manager.RenderAllHeadContentAsync(dest);

        var output = dest.GetOutput();
        output.ShouldContain("<meta charset=\"utf-8\">");
        output.ShouldNotContain("</meta>");
    }

    [Fact]
    public async Task ManagerShouldRenderNonVoidElementsWithClosingTag()
    {
        var manager = new HeadManager();
        manager.Add(new HeadElement("title") { Content = "My Page" });

        var dest = new Atoll.Core.Rendering.StringRenderDestination();
        await manager.RenderAllHeadContentAsync(dest);

        dest.GetOutput().ShouldContain("<title>My Page</title>");
    }

    [Fact]
    public async Task ManagerShouldRenderBooleanAttributes()
    {
        var manager = new HeadManager();
        manager.Add(
            new HeadElement("script")
                .SetAttribute("src", "/js/app.js")
                .SetAttribute("defer", null));

        var dest = new Atoll.Core.Rendering.StringRenderDestination();
        await manager.RenderAllHeadContentAsync(dest);

        dest.GetOutput().ShouldContain("<script src=\"/js/app.js\" defer>");
    }

    [Fact]
    public async Task ManagerShouldEncodeAttributeValues()
    {
        var manager = new HeadManager();
        manager.Add(
            new HeadElement("meta")
                .SetAttribute("name", "description")
                .SetAttribute("content", "A \"great\" page & more"));

        var dest = new Atoll.Core.Rendering.StringRenderDestination();
        await manager.RenderAllHeadContentAsync(dest);

        dest.GetOutput().ShouldContain(
            "content=\"A &quot;great&quot; page &amp; more\"");
    }

    [Fact]
    public void ClearShouldRemoveAllElements()
    {
        var manager = new HeadManager();
        manager.Add(new HeadElement("title") { Content = "My Page" });
        manager.Add(
            new HeadElement("link")
                .SetAttribute("rel", "stylesheet")
                .SetAttribute("href", "/css/main.css"));

        manager.Clear();

        manager.Count.ShouldBe(0);
    }

    [Fact]
    public void ClearShouldAllowReAddingPreviousKeys()
    {
        var manager = new HeadManager();
        manager.Add(new HeadElement("title") { Content = "First Title" });

        manager.Clear();
        manager.Add(new HeadElement("title") { Content = "Second Title" }).ShouldBeTrue();

        manager.Count.ShouldBe(1);
    }

    [Fact]
    public void GetElementsShouldReturnInInsertionOrder()
    {
        var manager = new HeadManager();
        var title = new HeadElement("title") { Content = "My Page" };
        var meta = new HeadElement("meta")
            .SetAttribute("name", "description")
            .SetAttribute("content", "A page");
        var link = new HeadElement("link")
            .SetAttribute("rel", "stylesheet")
            .SetAttribute("href", "/css/main.css");

        manager.Add(title);
        manager.Add(meta);
        manager.Add(link);

        var elements = manager.GetElements();
        elements.Count.ShouldBe(3);
        elements[0].ShouldBeSameAs(title);
        elements[1].ShouldBeSameAs(meta);
        elements[2].ShouldBeSameAs(link);
    }

    // ── Null argument validation ──

    [Fact]
    public void DeduplicatorShouldThrowForNullElement()
    {
        Should.Throw<ArgumentNullException>(
            () => HeadDeduplicator.GenerateKey(null!));
    }

    [Fact]
    public void ManagerAddShouldThrowForNullElement()
    {
        var manager = new HeadManager();

        Should.Throw<ArgumentNullException>(() => manager.Add(null!));
    }

    [Fact]
    public async Task ManagerRenderAllShouldThrowForNullDestination()
    {
        var manager = new HeadManager();

        await Should.ThrowAsync<ArgumentNullException>(
            () => manager.RenderAllHeadContentAsync(null!).AsTask());
    }

    [Fact]
    public void HeadElementConstructorShouldThrowForNullTag()
    {
        Should.Throw<ArgumentNullException>(() => new HeadElement(null!));
    }

    [Fact]
    public void HeadElementConstructorWithAttrsShouldThrowForNullTag()
    {
        Should.Throw<ArgumentNullException>(
            () => new HeadElement(null!, new Dictionary<string, string?>()));
    }

    [Fact]
    public void HeadElementConstructorShouldThrowForNullAttributes()
    {
        Should.Throw<ArgumentNullException>(
            () => new HeadElement("meta", null!));
    }

    [Fact]
    public void SetAttributeShouldThrowForNullName()
    {
        var element = new HeadElement("meta");

        Should.Throw<ArgumentNullException>(
            () => element.SetAttribute(null!, "value"));
    }
}
