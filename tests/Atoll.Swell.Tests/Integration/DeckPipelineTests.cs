using Atoll.Swell.Configuration;
using Atoll.Swell.Layouts;
using Atoll.Swell.Markdown;

namespace Atoll.Swell.Tests.Integration;

/// <summary>
/// End-to-end tests covering the full deck parsing pipeline:
/// headmatter → slide splitting → frontmatter → notes → layout resolution.
/// </summary>
public sealed class DeckPipelineTests
{
    private const string SampleDeck = """
        ---
        title: Integration Test Talk
        aspectRatio: 16/9
        transition: fade
        slideNumbers: true
        ---

        ---
        layout: cover
        ---

        # Welcome

        Subtitle line.

        <!-- Cover presenter note -->

        ---

        ## Agenda

        - Point one
        - Point two

        ---

        ---
        layout: two-cols
        ---

        ## Left Column

        Content here.

        ::right::

        ## Right Column

        More content.

        ---

        ---
        layout: section
        ---

        ## Part Two

        ---

        ---
        layout: end
        ---

        # Thank You!
        """;

    [Fact]
    public void should_parse_five_slides_from_sample_deck()
    {
        var deck = SlideParser.Parse(SampleDeck);

        deck.Slides.Count.ShouldBe(5);
    }

    [Fact]
    public void should_extract_deck_config_correctly()
    {
        var deck = SlideParser.Parse(SampleDeck);

        deck.Config.Title.ShouldBe("Integration Test Talk");
        deck.Config.AspectRatio.ShouldBe(AspectRatio.Ratio16x9);
        deck.Config.Transition.ShouldBe(TransitionType.Fade);
        deck.Config.SlideNumbers.ShouldBeTrue();
    }

    [Fact]
    public void should_assign_cover_layout_to_first_slide()
    {
        var deck = SlideParser.Parse(SampleDeck);

        deck.Slides[0].Config.Layout.ShouldBe("cover");
    }

    [Fact]
    public void should_assign_default_layout_to_agenda_slide()
    {
        var deck = SlideParser.Parse(SampleDeck);

        deck.Slides[1].Config.Layout.ShouldBe("default");
    }

    [Fact]
    public void should_assign_two_cols_layout_to_third_slide()
    {
        var deck = SlideParser.Parse(SampleDeck);

        deck.Slides[2].Config.Layout.ShouldBe("two-cols");
    }

    [Fact]
    public void should_extract_presenter_notes_from_cover_slide()
    {
        var deck = SlideParser.Parse(SampleDeck);

        deck.Slides[0].Notes.ShouldBe("Cover presenter note");
    }

    [Fact]
    public void should_not_include_notes_in_slide_body()
    {
        var deck = SlideParser.Parse(SampleDeck);

        deck.Slides[0].Body.ShouldNotContain("<!--");
        deck.Slides[0].Body.ShouldNotContain("presenter note");
    }

    [Fact]
    public void should_resolve_all_layout_types_without_throwing()
    {
        var deck = SlideParser.Parse(SampleDeck);

        foreach (var slide in deck.Slides)
        {
            var act = () => SlideLayoutResolver.Resolve(slide.Config.Layout);
            act.ShouldNotThrow();
        }
    }

    [Fact]
    public void should_index_slides_sequentially_from_zero()
    {
        var deck = SlideParser.Parse(SampleDeck);

        for (var i = 0; i < deck.Slides.Count; i++)
        {
            deck.Slides[i].Index.ShouldBe(i);
        }
    }
}
