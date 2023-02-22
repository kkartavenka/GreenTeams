namespace GreenTeams;

internal class Program
{
    private const string IDLE_ARG = "idle";
    private const string RULES_ARG = "file";

    private const string DEFAULT_RULES_FILE = @"rules.txt";
    private static TimeSpan DEFAULT_IDLE_TIME = new TimeSpan(0, 5, 0);

    static string _rulesFile;
    static TimeSpan _idleTime;

    static DelayManager _delayManager = new DelayManager(idleDelay: 10000, minMoveDelay: 100, maxMoveDelay: 1000);

    static void Main(string[] args)
    {
        ShowInfo();
        TranslateArgs(args);

        var rules = new RulesClass(_rulesFile);
        var mouseManager = new MouseManager(rules.Rules, _idleTime);

        var mainRunner = Task.Run(() =>
        {
            while (true)
            {
                if (mouseManager.Move())
                    Thread.Sleep(_delayManager.GetNext());
                else
                    Thread.Sleep(_delayManager.GetDefaultDelay());
            }
        });

        Task.WaitAll(mainRunner);
    }

    static void ShowInfo()
    {
        Console.WriteLine("The following command line args can be specified:");
        Console.WriteLine($"Idle time can be specified via \"{IDLE_ARG}\" argument in minutes, default is set to {DEFAULT_IDLE_TIME}");
        Console.WriteLine($"Rules file can be specified via \"{RULES_ARG}\" argument, default file name is \"{DEFAULT_RULES_FILE}\"");
        Console.WriteLine(Environment.NewLine);
    }

    static void TranslateArgs(string[] args)
    {
        _idleTime = DEFAULT_IDLE_TIME;
        _rulesFile = DEFAULT_RULES_FILE;

        if (args.Length == 0)
            return;

        foreach( var arg in args )
        {
            var splittedArg = arg.Split('=');
            if (splittedArg.Length != 2)
                continue;

            switch (splittedArg[0].ToLower())
            {
                case IDLE_ARG:
                    if (int.TryParse(splittedArg[1], out var timeSpanInMinutes))
                        _idleTime = new TimeSpan(0, timeSpanInMinutes, 0);
                    else
                        throw new Exception($"Cannot convert argument value from {splittedArg[1]} to integer");

                    break;

                case RULES_ARG:
                    _rulesFile = splittedArg[1];
                    break;
                default:
                    throw new Exception($"Unknown argument name {splittedArg[0]}");
            }
        }
    }
}