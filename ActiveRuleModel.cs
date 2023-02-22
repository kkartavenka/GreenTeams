namespace GreenTeams;

internal readonly struct ActiveRuleModel
{
    internal ActiveRuleModel(TimeOnly min, TimeOnly max)
    {
        if (min > max)
            (Min, Max) = (max, min);

        (Min, Max) = (min, max);
    }

    internal TimeOnly Min { get; }
    internal TimeOnly Max { get; }
}
