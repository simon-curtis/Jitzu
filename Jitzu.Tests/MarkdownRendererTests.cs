using Jitzu.Shell.Core;
using Shouldly;

namespace Jitzu.Tests;

public class MarkdownRendererTests
{
    [Test]
    public void H1_Header_Renders_Bold_With_Color()
    {
        var lines = new[] { "# Hello World" };

        var result = MarkdownRenderer.Render(lines);

        result.Length.ShouldBe(1);
        result[0].ShouldContain("\e[1m"); // bold
        result[0].ShouldContain("Hello World");
        result[0].ShouldContain("\e[0m"); // reset
        result[0].ShouldNotContain("# "); // strip the marker
    }

    [Test]
    public void H2_And_H3_Headers_Use_Different_Colors()
    {
        var lines = new[] { "## Section", "### Subsection" };

        var result = MarkdownRenderer.Render(lines);

        result[0].ShouldContain("\e[1m");
        result[0].ShouldContain("Section");
        result[0].ShouldNotContain("## ");
        result[1].ShouldContain("Subsection");
        result[1].ShouldNotContain("### ");
        // H2 and H3 should use different color codes
        result[0].ShouldNotBe(result[1].Replace("Subsection", "Section"));
    }

    [Test]
    public void Bold_Inline_Text()
    {
        var result = MarkdownRenderer.Render(["Hello **world** today"]);

        result[0].ShouldContain("\e[1m");
        result[0].ShouldContain("world");
        result[0].ShouldNotContain("**");
    }

    [Test]
    public void Italic_Inline_Text()
    {
        var result = MarkdownRenderer.Render(["Hello *world* today"]);

        result[0].ShouldContain("\e[3m"); // italic
        result[0].ShouldContain("world");
        result[0].ShouldNotContain("*world*");
    }

    [Test]
    public void Inline_Code()
    {
        var result = MarkdownRenderer.Render(["Use `foo()` here"]);

        result[0].ShouldContain("foo()");
        result[0].ShouldNotContain("`");
    }

    [Test]
    public void Code_Block_Renders_Colored_And_Indented()
    {
        var lines = new[] { "```", "let x = 5", "let y = 10", "```" };

        var result = MarkdownRenderer.Render(lines);

        // Fences should not appear as raw ```
        result.ShouldNotContain("```");
        // Code lines should be present and colored
        var codeLine = result.First(l => l.Contains("let x = 5"));
        codeLine.ShouldContain("\e[38;2;"); // code color
        codeLine.ShouldStartWith("  "); // indented
    }

    [Test]
    public void Bullet_List_Uses_Bullet_Character()
    {
        var result = MarkdownRenderer.Render(["- Item one", "- Item two"]);

        result[0].ShouldContain("Item one");
        result[0].ShouldNotStartWith("- ");
        // Should contain a bullet-like prefix (not the raw dash)
        result[0].ShouldContain("•");
    }

    [Test]
    public void Numbered_List_Preserves_Numbers()
    {
        var result = MarkdownRenderer.Render(["1. First", "2. Second"]);

        result[0].ShouldContain("1.");
        result[0].ShouldContain("First");
        result[1].ShouldContain("2.");
    }

    [Test]
    public void Blockquote_Renders_With_Bar()
    {
        var result = MarkdownRenderer.Render(["> Some quote"]);

        result[0].ShouldContain("Some quote");
        result[0].ShouldNotStartWith("> ");
        result[0].ShouldContain("│");
    }

    [Test]
    public void Horizontal_Rule_Renders_Line()
    {
        var result = MarkdownRenderer.Render(["---"]);

        // Should render as a visual line, not literal ---
        result[0].ShouldNotBe("---");
        result[0].ShouldContain("─");
    }

    [Test]
    public void Link_Shows_Text_And_Url()
    {
        var result = MarkdownRenderer.Render(["Check [docs](https://example.com) here"]);

        result[0].ShouldContain("docs");
        result[0].ShouldContain("https://example.com");
        result[0].ShouldNotContain("[docs]");
        result[0].ShouldNotContain("](");
    }

    [Test]
    public void Table_Renders_With_Aligned_Columns()
    {
        var lines = new[]
        {
            "| Name | Age |",
            "| --- | --- |",
            "| Alice | 30 |",
            "| Bob | 25 |"
        };

        var result = MarkdownRenderer.Render(lines);

        // Header row should be bold
        var headerLine = result.First(l => l.Contains("Name"));
        headerLine.ShouldContain("\e[1m");
        // Separator should be a visual line, not pipes and dashes
        result.ShouldNotContain("| --- | --- |");
        // Data rows should contain the values
        result.Any(l => l.Contains("Alice")).ShouldBeTrue();
        result.Any(l => l.Contains("Bob")).ShouldBeTrue();
    }

    [Test]
    public void Table_Pads_Columns_To_Equal_Width()
    {
        var lines = new[]
        {
            "| Name | Age |",
            "| --- | --- |",
            "| Alice | 30 |",
        };

        var result = MarkdownRenderer.Render(lines);

        // Both header and data rows should have consistent column widths
        // Find columns with "Name" — it should be padded to match "Alice" width
        var headerLine = result.First(l => l.Contains("Name"));
        var dataLine = result.First(l => l.Contains("Alice"));
        // Both should use the │ separator
        headerLine.ShouldContain("│");
        dataLine.ShouldContain("│");
    }

    [Test]
    public void Table_With_Trailing_Whitespace_Still_Detected()
    {
        var lines = new[]
        {
            "| Name | Age |  ",
            "| --- | --- |  ",
            "| Alice | 30 |  ",
        };

        var result = MarkdownRenderer.Render(lines);

        // Should still be detected as table, not raw pipes
        var headerLine = result.First(l => l.Contains("Name"));
        headerLine.ShouldContain("│");
        headerLine.ShouldContain("\e[1m");
    }

    [Test]
    public void Table_With_Bold_Cells_Aligns_On_Text_Width()
    {
        var lines = new[]
        {
            "| Metric | Before | After | Change |",
            "| --- | --- | --- | --- |",
            "| NCI count | 15 | 5 | **-67%** |",
            "| Storage | 59.5 GB | 5.0 GB | **-92%** |",
        };

        var result = MarkdownRenderer.Render(lines);

        // All data rows should have box-drawing borders
        var nciRow = result.First(l => l.Contains("NCI count"));
        var storageRow = result.First(l => l.Contains("Storage"));
        nciRow.ShouldContain("│");
        storageRow.ShouldContain("│");
        // Bold markers should be stripped, content rendered bold
        nciRow.ShouldNotContain("**");
        nciRow.ShouldContain("-67%");
        // All 4 columns should be present in every row
        result.First(l => l.Contains("Metric")).ShouldContain("Change");
    }

    [Test]
    public void Table_Without_Trailing_Pipe_Still_Detected()
    {
        var lines = new[]
        {
            "| Name | Age",
            "| --- | ---",
            "| Alice | 30",
        };

        var result = MarkdownRenderer.Render(lines);

        var headerLine = result.First(l => l.Contains("Name"));
        headerLine.ShouldContain("│");
    }

    [Test]
    public void Table_With_Dashed_Separator_No_Spaces()
    {
        // Exact format from real markdown files
        var lines = new[]
        {
            "| Metric | Before | After | Change |",
            "|--------|--------|-------|--------|",
            "| Index count (NCI) | 15 | 5 | -67% |",
            "| Total NCI storage | 59.5 GB | 5.0 GB | **-92%** |",
            "| Total storage (incl. PK) | 82.9 GB | 28.4 GB | **-66%** |",
            "| Indexes maintained per INSERT | 16 | 6 | **-63%** |",
        };

        var result = MarkdownRenderer.Render(lines);

        // Should have: top border, header, separator, 4 data rows, bottom border = 8 lines
        result.Length.ShouldBe(8);
        // Top border
        result[0].ShouldContain("┌");
        result[0].ShouldContain("┐");
        // Header should be bold with all 4 columns
        result[1].ShouldContain("Metric");
        result[1].ShouldContain("Change");
        result[1].ShouldContain("\e[1m");
        // No raw pipes should leak through
        result[1].ShouldNotContain(" | ");
        // Data rows should have box-drawing borders
        result[3].ShouldContain("│");
        result[3].ShouldContain("Index count (NCI)");
        // Bold in cells should render
        result[4].ShouldContain("-92%");
        result[4].ShouldNotContain("**");
        // Last data row
        result[6].ShouldContain("Indexes maintained per INSERT");
        result[6].ShouldContain("-63%");
        // Bottom border
        result[7].ShouldContain("└");
        result[7].ShouldContain("┘");
    }

    [Test]
    public void Table_With_Wide_Columns_Aligns_Correctly()
    {
        var lines = new[]
        {
            "| Index | Size (GB) | Filter | Status |",
            "|-------|-----------|--------|--------|",
            "| PK_MessageStore (clustered) | 23.43 | - | KEPT |",
            "| IDX_MS_MID | 22.28 | - | SLIMMED |",
        };

        var result = MarkdownRenderer.Render(lines);

        // All rows should have box-drawing characters
        foreach (var line in result)
            (line.Contains('│') || line.Contains('├') || line.Contains('┤')
             || line.Contains('┌') || line.Contains('┐') || line.Contains('└') || line.Contains('┘')).ShouldBeTrue();

        // Data should be present
        result.Any(l => l.Contains("PK_MessageStore (clustered)")).ShouldBeTrue();
        result.Any(l => l.Contains("SLIMMED")).ShouldBeTrue();
    }

    [Test]
    public void Table_With_Many_Columns()
    {
        var lines = new[]
        {
            "| Query | Phase | i1 | i2 | i3 | i4 | i5 |",
            "|-------|-------|-----|-----|-----|-----|-----|",
            "| Q1 MID (nvarchar) | BEFORE | 75 | 1 | 1 | 0 | 0 |",
        };

        var result = MarkdownRenderer.Render(lines);

        result.Length.ShouldBe(5); // top + header + sep + 1 data + bottom
        result[1].ShouldContain("Query");
        result[1].ShouldContain("i5");
        result[3].ShouldContain("Q1 MID (nvarchar)");
        result[3].ShouldContain("75");
    }

    [Test]
    public void Plain_Text_Passes_Through()
    {
        var result = MarkdownRenderer.Render(["Just plain text."]);

        result[0].ShouldBe("Just plain text.");
    }
}
