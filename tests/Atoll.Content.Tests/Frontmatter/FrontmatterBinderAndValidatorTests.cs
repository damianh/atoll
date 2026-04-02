using System.ComponentModel.DataAnnotations;
using Atoll.Content.Frontmatter;
using Shouldly;
using Xunit;

namespace Atoll.Content.Tests.Frontmatter;

public sealed class FrontmatterBinderTests
{
    private sealed class BlogPost
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime PubDate { get; set; }
        public List<string> Tags { get; set; } = [];
        public bool Draft { get; set; }
    }

    private sealed class SimpleData
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

    [Fact]
    public void ShouldBindStringProperties()
    {
        var yaml = "title: Hello World\ndescription: A test post";
        var result = FrontmatterBinder.Bind<BlogPost>(yaml);

        result.Title.ShouldBe("Hello World");
        result.Description.ShouldBe("A test post");
    }

    [Fact]
    public void ShouldBindDateTimeProperties()
    {
        var yaml = "pubDate: 2026-03-15";
        var result = FrontmatterBinder.Bind<BlogPost>(yaml);

        result.PubDate.Year.ShouldBe(2026);
        result.PubDate.Month.ShouldBe(3);
        result.PubDate.Day.ShouldBe(15);
    }

    [Fact]
    public void ShouldBindListProperties()
    {
        var yaml = "tags:\n  - csharp\n  - dotnet\n  - astro";
        var result = FrontmatterBinder.Bind<BlogPost>(yaml);

        result.Tags.ShouldBe(["csharp", "dotnet", "astro"]);
    }

    [Fact]
    public void ShouldBindBooleanProperties()
    {
        var yaml = "draft: true";
        var result = FrontmatterBinder.Bind<BlogPost>(yaml);

        result.Draft.ShouldBeTrue();
    }

    [Fact]
    public void ShouldBindIntegerProperties()
    {
        var yaml = "name: Test\ncount: 42";
        var result = FrontmatterBinder.Bind<SimpleData>(yaml);

        result.Count.ShouldBe(42);
    }

    [Fact]
    public void ShouldIgnoreUnmatchedYamlProperties()
    {
        var yaml = "title: Hello\nunknownField: some value";
        var result = FrontmatterBinder.Bind<BlogPost>(yaml);

        result.Title.ShouldBe("Hello");
    }

    [Fact]
    public void ShouldReturnDefaultsForMissingProperties()
    {
        var yaml = "title: Hello";
        var result = FrontmatterBinder.Bind<BlogPost>(yaml);

        result.Title.ShouldBe("Hello");
        result.Description.ShouldBe("");
        result.Draft.ShouldBeFalse();
    }

    [Fact]
    public void ShouldReturnNewInstanceForEmptyYaml()
    {
        var result = FrontmatterBinder.Bind<BlogPost>("");

        result.ShouldNotBeNull();
        result.Title.ShouldBe("");
    }

    [Fact]
    public void ShouldReturnNewInstanceForWhitespaceYaml()
    {
        var result = FrontmatterBinder.Bind<BlogPost>("   ");

        result.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldThrowOnNullYaml()
    {
        Should.Throw<ArgumentNullException>(() => FrontmatterBinder.Bind<BlogPost>(null!));
    }

    [Fact]
    public void ShouldThrowBindingExceptionOnInvalidYaml()
    {
        var yaml = "title: [invalid: yaml: structure";
        Should.Throw<FrontmatterBindingException>(() => FrontmatterBinder.Bind<BlogPost>(yaml));
    }

    [Fact]
    public void ShouldBindUsingUntypedOverload()
    {
        var yaml = "title: Hello";
        var result = FrontmatterBinder.Bind(yaml, typeof(BlogPost));

        result.ShouldBeOfType<BlogPost>();
        ((BlogPost)result).Title.ShouldBe("Hello");
    }

    [Fact]
    public void ShouldReturnNewInstanceFromUntypedOverloadForEmptyYaml()
    {
        var result = FrontmatterBinder.Bind("", typeof(BlogPost));

        result.ShouldBeOfType<BlogPost>();
    }

    [Fact]
    public void ShouldBindComplexFrontmatter()
    {
        var yaml = """
            title: My First Post
            description: A comprehensive guide to Atoll
            pubDate: 2026-01-15
            tags:
              - csharp
              - web
            draft: false
            """;

        var result = FrontmatterBinder.Bind<BlogPost>(yaml);

        result.Title.ShouldBe("My First Post");
        result.Description.ShouldBe("A comprehensive guide to Atoll");
        result.PubDate.ShouldBe(new DateTime(2026, 1, 15));
        result.Tags.Count.ShouldBe(2);
        result.Draft.ShouldBeFalse();
    }

    [Fact]
    public void ShouldThrowOnNullTargetType()
    {
        Should.Throw<ArgumentNullException>(() => FrontmatterBinder.Bind("title: Hello", null!));
    }

    [Fact]
    public void ShouldReturnNewInstanceFromUntypedOverloadForWhitespaceYaml()
    {
        var result = FrontmatterBinder.Bind("   ", typeof(BlogPost));

        result.ShouldBeOfType<BlogPost>();
    }

    [Fact]
    public void ShouldThrowBindingExceptionFromUntypedOverloadOnInvalidYaml()
    {
        var yaml = "title: [invalid: yaml: structure";

        Should.Throw<FrontmatterBindingException>(() => FrontmatterBinder.Bind(yaml, typeof(BlogPost)));
    }
}

public sealed class FrontmatterBindingExceptionTests
{
    [Fact]
    public void ShouldStoreMessage()
    {
        var ex = new FrontmatterBindingException("test error");

        ex.Message.ShouldBe("test error");
    }

    [Fact]
    public void ShouldStoreInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new FrontmatterBindingException("outer", inner);

        ex.Message.ShouldBe("outer");
        ex.InnerException.ShouldBeSameAs(inner);
    }
}

public sealed class FrontmatterValidatorTests
{
    private sealed class ValidatedPost
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title must be 100 characters or fewer")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Date is required")]
        public DateTime? PubDate { get; set; }

        [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10")]
        public int Rating { get; set; }
    }

    private sealed class NoValidationSchema
    {
        public string Name { get; set; } = "";
    }

    [Fact]
    public void ShouldPassValidObject()
    {
        var data = new ValidatedPost { Title = "Hello", PubDate = DateTime.Now, Rating = 5 };
        var result = FrontmatterValidator.Validate(data);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldFailOnMissingRequired()
    {
        var data = new ValidatedPost { Title = "", PubDate = DateTime.Now, Rating = 5 };
        var result = FrontmatterValidator.Validate(data);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.ErrorMessage!.Contains("Title is required"));
    }

    [Fact]
    public void ShouldFailOnNullRequired()
    {
        var data = new ValidatedPost { Title = "Hello", PubDate = null, Rating = 5 };
        var result = FrontmatterValidator.Validate(data);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage!.Contains("Date is required"));
    }

    [Fact]
    public void ShouldFailOnRangeViolation()
    {
        var data = new ValidatedPost { Title = "Hello", PubDate = DateTime.Now, Rating = 15 };
        var result = FrontmatterValidator.Validate(data);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage!.Contains("Rating must be between 0 and 10"));
    }

    [Fact]
    public void ShouldFailOnStringLengthViolation()
    {
        var data = new ValidatedPost
        {
            Title = new string('x', 101),
            PubDate = DateTime.Now,
            Rating = 5
        };
        var result = FrontmatterValidator.Validate(data);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage!.Contains("100 characters"));
    }

    [Fact]
    public void ShouldCollectMultipleErrors()
    {
        var data = new ValidatedPost { Title = "", PubDate = null, Rating = 15 };
        var result = FrontmatterValidator.Validate(data);

        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void ShouldPassObjectWithoutValidationAttributes()
    {
        var data = new NoValidationSchema { Name = "" };
        var result = FrontmatterValidator.Validate(data);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ThrowIfInvalidShouldNotThrowForValidData()
    {
        var data = new ValidatedPost { Title = "Hello", PubDate = DateTime.Now, Rating = 5 };
        var result = FrontmatterValidator.Validate(data);

        Should.NotThrow(() => result.ThrowIfInvalid("test-entry"));
    }

    [Fact]
    public void ThrowIfInvalidShouldThrowForInvalidData()
    {
        var data = new ValidatedPost { Title = "", PubDate = null, Rating = 15 };
        var result = FrontmatterValidator.Validate(data);

        var ex = Should.Throw<FrontmatterValidationException>(() => result.ThrowIfInvalid("test-entry"));
        ex.Message.ShouldContain("test-entry");
        ex.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldThrowOnNullData()
    {
        Should.Throw<ArgumentNullException>(() => FrontmatterValidator.Validate(null!));
    }
}

public sealed class FrontmatterValidationResultTests
{
    [Fact]
    public void ShouldThrowOnNullErrors()
    {
        Should.Throw<ArgumentNullException>(() =>
            new FrontmatterValidationResult(true, null!));
    }

    [Fact]
    public void ThrowIfInvalidShouldDoNothingWhenValid()
    {
        var result = new FrontmatterValidationResult(true, []);

        Should.NotThrow(() => result.ThrowIfInvalid("entry-id"));
    }
}

public sealed class FrontmatterValidationExceptionTests
{
    [Fact]
    public void ShouldStoreMessageAndErrors()
    {
        var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>
        {
            new("Error 1"),
            new("Error 2"),
        };
        var ex = new FrontmatterValidationException("validation failed", errors);

        ex.Message.ShouldBe("validation failed");
        ex.Errors.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldThrowOnNullErrors()
    {
        Should.Throw<ArgumentNullException>(() =>
            new FrontmatterValidationException("message", null!));
    }
}
