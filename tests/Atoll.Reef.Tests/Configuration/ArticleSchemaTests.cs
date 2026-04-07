using Atoll.Reef.Configuration;

namespace Atoll.Reef.Tests.Configuration;

public sealed class ArticleSchemaTests
{
    [Fact]
    public void GetTagsShouldReturnEmptyArrayWhenTagsIsEmpty()
    {
        var schema = new ArticleSchema { Title = "T", Description = "D", PubDate = DateTime.Now };
        schema.GetTags().ShouldBeEmpty();
    }

    [Fact]
    public void GetTagsShouldSplitOnComma()
    {
        var schema = new ArticleSchema { Title = "T", Description = "D", PubDate = DateTime.Now, Tags = "atoll,tutorial" };
        schema.GetTags().ShouldBe(["atoll", "tutorial"]);
    }

    [Fact]
    public void GetTagsShouldTrimWhitespace()
    {
        var schema = new ArticleSchema { Title = "T", Description = "D", PubDate = DateTime.Now, Tags = " atoll , tutorial " };
        schema.GetTags().ShouldBe(["atoll", "tutorial"]);
    }

    [Fact]
    public void GetTagsShouldRemoveEmptyEntries()
    {
        var schema = new ArticleSchema { Title = "T", Description = "D", PubDate = DateTime.Now, Tags = "atoll,,tutorial" };
        schema.GetTags().ShouldBe(["atoll", "tutorial"]);
    }
}
