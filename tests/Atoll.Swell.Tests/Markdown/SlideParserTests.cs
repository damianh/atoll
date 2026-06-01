using Atoll.Swell.Configuration;
using Atoll.Swell.Markdown;

namespace Atoll.Swell.Tests.Markdown;

public sealed class SlideParserTests
{
    [Fact]
    public void should_return_single_slide_when_no_separator_present()
    {
        const string content = "# Hello\n\nThis is slide content.";

        var deck = SlideParser.Parse(content);

        deck.Slides.Count.ShouldBe(1);
        deck.Slides[0].Body.ShouldContain("# Hello");
    }

    [Fact]
    public void should_split_on_double_blank_line_separator()
    {
        const string content = "# Slide 1\n\n---\n\n# Slide 2\n\n---\n\n# Slide 3";

        var deck = SlideParser.Parse(content);

        deck.Slides.Count.ShouldBe(3);
        deck.Slides[0].Body.ShouldContain("# Slide 1");
        deck.Slides[1].Body.ShouldContain("# Slide 2");
        deck.Slides[2].Body.ShouldContain("# Slide 3");
    }

    [Fact]
    public void should_not_split_on_bare_horizontal_rule()
    {
        // A bare --- without surrounding blank lines is a Markdown HR, not a slide separator.
        const string content = "Above\n---\nBelow";

        var deck = SlideParser.Parse(content);

        deck.Slides.Count.ShouldBe(1);
    }

    [Fact]
    public void should_extract_deck_headmatter()
    {
        const string content = """
            ---
            title: My Talk
            slideNumbers: false
            ---

            # Slide 1
            """;

        var deck = SlideParser.Parse(content);

        deck.Config.Title.ShouldBe("My Talk");
        deck.Config.SlideNumbers.ShouldBeFalse();
    }

    [Fact]
    public void should_default_deck_config_when_no_headmatter()
    {
        const string content = "# Slide 1";

        var deck = SlideParser.Parse(content);

        deck.Config.Title.ShouldBe("");
        deck.Config.SlideNumbers.ShouldBeTrue();
        deck.Config.AspectRatio.ShouldBe(AspectRatio.Ratio16x9);
    }

    [Fact]
    public void should_extract_per_slide_frontmatter()
    {
        // The first block is deck headmatter; subsequent blocks are slide frontmatter.
        const string content = "---\ntitle: Demo\n---\n\n---\nlayout: cover\n---\n\n# Cover Slide";

        var deck = SlideParser.Parse(content);

        deck.Slides.Count.ShouldBe(1);
        deck.Slides[0].Config.Layout.ShouldBe("cover");
        deck.Slides[0].Body.ShouldContain("# Cover Slide");
    }

    [Fact]
    public void should_assign_correct_zero_based_index_to_each_slide()
    {
        const string content = "# A\n\n---\n\n# B\n\n---\n\n# C";

        var deck = SlideParser.Parse(content);

        deck.Slides[0].Index.ShouldBe(0);
        deck.Slides[1].Index.ShouldBe(1);
        deck.Slides[2].Index.ShouldBe(2);
    }

    [Fact]
    public void should_extract_trailing_html_comment_as_presenter_notes()
    {
        const string content = "# Slide\n\nSome content.\n\n<!-- These are my notes -->";

        var deck = SlideParser.Parse(content);

        deck.Slides[0].Notes.ShouldBe("These are my notes");
        deck.Slides[0].Body.ShouldNotContain("<!--");
    }

    [Fact]
    public void should_concatenate_multiple_trailing_comments_as_notes()
    {
        const string content = "# Slide\n\n<!-- Part 1 -->\n<!-- Part 2 -->";

        var deck = SlideParser.Parse(content);

        deck.Slides[0].Notes.ShouldContain("Part 1");
        deck.Slides[0].Notes.ShouldContain("Part 2");
    }

    [Fact]
    public void should_not_extract_non_trailing_comment_as_notes()
    {
        // A comment in the middle of the slide should remain in the body, not be extracted as notes.
        const string content = "# Slide\n\n<!-- mid comment -->\n\nmore content";

        var deck = SlideParser.Parse(content);

        deck.Slides[0].Notes.ShouldBeEmpty();
        deck.Slides[0].Body.ShouldContain("<!-- mid comment -->");
    }

    [Fact]
    public void should_parse_deck_transition_from_headmatter()
    {
        const string content = "---\ntransition: fade\n---\n\n# Slide";

        var deck = SlideParser.Parse(content);

        deck.Config.Transition.ShouldBe(TransitionType.Fade);
    }

    [Fact]
    public void should_return_empty_deck_for_empty_input()
    {
        var deck = SlideParser.Parse("");

        deck.Slides.ShouldBeEmpty();
        deck.Config.Title.ShouldBe("");
    }

    [Fact]
    public void should_handle_headmatter_followed_by_slide_separator_then_slide()
    {
        const string content = """
            ---
            title: Demo
            ---

            ---
            layout: cover
            ---

            # Cover

            ---

            ## Second Slide
            """;

        var deck = SlideParser.Parse(content);

        deck.Config.Title.ShouldBe("Demo");
        deck.Slides.Count.ShouldBe(2);
        deck.Slides[0].Config.Layout.ShouldBe("cover");
        deck.Slides[1].Body.ShouldContain("## Second Slide");
    }
}
