using System.Text;

namespace InvertedIndex.Root.Services;

public class ResponseFormatter
{
    private const string DataPath = @"C:\Users\sabir\RiderProjects\InvertedIndex\InvertedIndex\Data";
    
    public string FormatResponse(List<string> indexes, string word)
    {
        var response = new StringBuilder();

        foreach (var index in indexes)
        {
            var path = $"{DataPath}\\{index}";
            var text = File.ReadAllText(path);
            var sentences = text.Split(new[]{'.', '!', '?'}, StringSplitOptions.RemoveEmptyEntries);
            
            var matchingSentences = sentences.Where(sentence => 
                sentence.Contains(word, StringComparison.OrdinalIgnoreCase));
            
            foreach (var sentence in matchingSentences)
            {
                var highlightedSentence = System.Text.RegularExpressions.Regex.Replace(
                    sentence.Trim(),
                    $@"\b{System.Text.RegularExpressions.Regex.Escape(word)}\b",
                    match => $"<b>{match.Value}</b>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

                response.Append($"{index}:<br>{highlightedSentence}<br><br>");
            }
        }

        return response.ToString();
    }
}