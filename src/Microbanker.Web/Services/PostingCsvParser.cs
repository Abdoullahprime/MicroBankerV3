// src/Microbanker.Web/Services/PostingCsvParser.cs
using System.Globalization;

namespace Microbanker.Web.Services;

/// <summary>
/// Reads a bulk-posting file: AccountNo,DrCr,Amount[,Narration]. A header row is optional. Quotes are
/// understood, so a narration may contain commas. Every problem is reported with its line number.
/// </summary>
public static class PostingCsvParser
{
    public const int MaxRows = 500;

    public static (List<PostingLeg> Legs, List<string> Errors) Parse(TextReader reader)
    {
        var legs = new List<PostingLeg>();
        var errors = new List<string>();
        var lineNo = 0;
        var first = true;

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            lineNo++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cells = SplitLine(line);
            if (first)
            {
                first = false;
                if (cells[0].Trim().Equals("AccountNo", StringComparison.OrdinalIgnoreCase)) continue;   // header
            }

            if (legs.Count >= MaxRows)
            {
                errors.Add($"The file has more than {MaxRows} rows.");
                break;
            }
            if (cells.Count < 3)
            {
                errors.Add($"Line {lineNo}: expected AccountNo, DrCr and Amount.");
                continue;
            }

            var account = cells[0].Trim().ToUpperInvariant();
            var dir = cells[1].Trim().ToUpperInvariant() switch { "DR" or "D" or "DEBIT" => "DR", "CR" or "C" or "CREDIT" => "CR", _ => null };
            var narration = cells.Count > 3 && !string.IsNullOrWhiteSpace(cells[3]) ? cells[3].Trim() : null;

            if (account.Length == 0) { errors.Add($"Line {lineNo}: the account number is empty."); continue; }
            if (dir == null) { errors.Add($"Line {lineNo}: the direction must be DR or CR."); continue; }
            if (!decimal.TryParse(cells[2].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                errors.Add($"Line {lineNo}: \"{cells[2].Trim()}\" is not a valid amount.");
                continue;
            }
            if (amount <= 0 || decimal.Round(amount, 2) != amount)
            {
                errors.Add($"Line {lineNo}: the amount must be greater than zero with at most two decimal places.");
                continue;
            }
            legs.Add(new PostingLeg(account, dir, amount, narration));
        }

        if (legs.Count == 0 && errors.Count == 0) errors.Add("The file has no rows.");
        return (legs, errors);
    }

    /// <summary>Splits one CSV line, honouring double quotes ("" inside quotes is a literal quote).</summary>
    internal static List<string> SplitLine(string line)
    {
        var cells = new List<string>();
        var sb = new System.Text.StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (inQuotes)
            {
                if (ch == '"' && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else if (ch == '"') inQuotes = false;
                else sb.Append(ch);
            }
            else if (ch == '"') inQuotes = true;
            else if (ch == ',') { cells.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(ch);
        }
        cells.Add(sb.ToString());
        return cells;
    }
}
