using Microbanker.Web.Services;

namespace Microbanker.Tests;

public class PostingCsvParserTests
{
    private static (List<PostingLeg> Legs, List<string> Errors) Parse(string text) => PostingCsvParser.Parse(new StringReader(text));

    [Fact]
    public void Reads_rows_with_and_without_a_header_and_narration()
    {
        var (legs, errors) = Parse("AccountNo,DrCr,Amount,Narration\nmn-sa-0000001,cr,1500.50,Salary\n\nMN-SA-0000002, DR , 20 \n");
        Assert.Empty(errors);
        Assert.Equal(2, legs.Count);
        Assert.Equal(new PostingLeg("MN-SA-0000001", "CR", 1500.50m, "Salary"), legs[0]);
        Assert.Equal(new PostingLeg("MN-SA-0000002", "DR", 20m, null), legs[1]);
    }

    [Fact]
    public void Quoted_narrations_may_contain_commas_and_quotes()
    {
        var (legs, errors) = Parse("A1,CR,10,\"Salary, March \"\"bonus\"\"\"");
        Assert.Empty(errors);
        Assert.Equal("Salary, March \"bonus\"", legs[0].Narration);
    }

    [Theory]
    [InlineData("D", "DR")]
    [InlineData("debit", "DR")]
    [InlineData("C", "CR")]
    [InlineData("Credit", "CR")]
    public void Direction_words_are_understood(string word, string expected)
    {
        var (legs, _) = Parse($"A1,{word},5");
        Assert.Equal(expected, legs[0].DrCr);
    }

    [Fact]
    public void Every_bad_row_is_reported_with_its_line_number()
    {
        var (legs, errors) = Parse("A1,CR,10\nA2,XX,10\nA3,CR,abc\nA4,CR,0\nA5,CR,1.005\nA6\n,CR,5");
        Assert.Single(legs);
        Assert.Equal(6, errors.Count);
        Assert.Contains(errors, e => e.StartsWith("Line 2") && e.Contains("DR or CR"));
        Assert.Contains(errors, e => e.StartsWith("Line 3") && e.Contains("abc"));
        Assert.Contains(errors, e => e.StartsWith("Line 4"));
        Assert.Contains(errors, e => e.StartsWith("Line 5"));
        Assert.Contains(errors, e => e.StartsWith("Line 6"));
        Assert.Contains(errors, e => e.StartsWith("Line 7") && e.Contains("account"));
    }

    [Fact]
    public void An_empty_file_and_an_oversized_file_are_errors()
    {
        Assert.Contains("no rows", Parse("").Errors.Single());
        Assert.Contains("no rows", Parse("AccountNo,DrCr,Amount\n").Errors.Single());

        var big = string.Join("\n", Enumerable.Range(1, PostingCsvParser.MaxRows + 5).Select(i => $"A{i},CR,1"));
        var (legs, errors) = Parse(big);
        Assert.Equal(PostingCsvParser.MaxRows, legs.Count);
        Assert.Contains(errors, e => e.Contains("more than"));
    }

    [Fact]
    public void Amounts_use_a_dot_as_the_decimal_separator_whatever_the_server_culture()
    {
        var old = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
            Assert.Equal(1234.5m, Parse("A1,CR,1234.50").Legs[0].Amount);
        }
        finally { System.Globalization.CultureInfo.CurrentCulture = old; }
    }
}
