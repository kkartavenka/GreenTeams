namespace GreenTeams;

internal class RulesClass
{
    internal RulesClass(string file)
    {
        ValidateFileExistance(file);
        TranslateRules(file);
    }

    private void ValidateFileExistance(string file)
    {
        if (!File.Exists(file))
            throw new FileNotFoundException(file);
    }

    private string[] ValidateRule(string ruleString)
    {
        var rule = ruleString.Split(';');
        if (rule.Length != 3)
            ShowInfo(null);            

        return rule;
    }

    private DayOfWeek TranslateDayOfWeek(string input) => input.ToLower() switch
    {
        "monday" or "mo" => DayOfWeek.Monday,
        "tuesday" or "tu" => DayOfWeek.Tuesday,
        "wednesday" or "we" => DayOfWeek.Wednesday,
        "thursday" or "th" => DayOfWeek.Thursday,
        "friday" or "fr" => DayOfWeek.Friday,
        "saturday" or "sa" => DayOfWeek.Saturday,
        "sunday" or "su" => DayOfWeek.Sunday,
        _ => throw new Exception($"Day of the week \"{input}\" cannot be recognized")
    };

    private TimeOnly TranslateTime(string time)
    {
        if (TimeOnly.TryParse(time, out var formattedTime))
            return formattedTime;

        throw new Exception($"Time is not specified correctly via the following expression: {time}");
    }

    private void TranslateRules(string file)
    {
        var rulesLines = File.ReadAllLines(file);

        if (!rulesLines.Any())
            ShowInfo(file);

        foreach (var rule in rulesLines)
        {
            var splittedRule = ValidateRule(rule);

            var dayOfWeek = TranslateDayOfWeek(splittedRule[0]);
            var (timeRange1, timeRange2) = (TranslateTime(splittedRule[1]), TranslateTime(splittedRule[2]));

            if (Rules.ContainsKey(dayOfWeek))
                Rules[dayOfWeek].Add(new(timeRange1, timeRange2));
            else
                Rules.TryAdd(dayOfWeek, new()
                {
                    new (timeRange1, timeRange2)
                });
        }
    }

    internal void ShowInfo(string? file)
    {
        Console.WriteLine($"The following format is expected {file ?? $"in {file}"}");
        Console.WriteLine("Day of the week;HH:mm;HH:mm");
        Console.WriteLine("Example: Monday;9:00;17:30");
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        Environment.Exit(0);
    }

    internal Dictionary<DayOfWeek, List<ActiveRuleModel>> Rules { get; private set; } = new();
}
