using System.Drawing;
using System.Runtime.InteropServices;

namespace GreenTeams;

internal class MouseManager
{
    [StructLayout(LayoutKind.Sequential)]
    struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    private const int MAX_MOUSE_STEP_SIZE = 10;


    private readonly Dictionary<DayOfWeek, List<ActiveRuleModel>> _rules;
    private readonly TimeSpan _idleTime = new();

    private DateTime _idleStartTime;

    private Random _random = new();

    private Point _previousMousePosition = default;


    private int _directionX = 1, _directionY = 1;
    private double _changeOfDirectionChange = 0.1;

    internal MouseManager(Dictionary<DayOfWeek, List<ActiveRuleModel>> rules, TimeSpan idleTime)
    {
        GetCursorPos(ref _previousMousePosition);
        (_rules, _idleTime) = (rules, idleTime);
    }

    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(ref Point point);

    [DllImport("user32.dll")]
    static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

    private bool CheckRules()
    {
        if (_rules.TryGetValue(DateTime.Now.DayOfWeek, out var rules))
        {
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);
            foreach (var rule in rules)
                if (rule.Min <= currentTime && rule.Max >= currentTime)
                    return true;
        }

        return false;
    }

    internal bool Move()
    {
        var applicableRule = CheckRules();
        if (applicableRule == false)
        {
            _idleStartTime = DateTime.Now;
            return false;
        }

        if (DateTime.Now - _idleStartTime < _idleTime)
            return false;

        Point currentCursorPosition = default;
        GetCursorPos(ref currentCursorPosition);

        if (currentCursorPosition.X != _previousMousePosition.X || currentCursorPosition.Y != _previousMousePosition.Y)
        {
            _previousMousePosition = currentCursorPosition;
            _idleStartTime = DateTime.Now;
            return false;
        }

        var screenResolution = GetScreenResolution();
        var newPosition = GetNewCursorPosition(currentCursorPosition, screenResolution.width, screenResolution.height);

        SetCursorPos(newPosition.X, newPosition.Y);
        GetCursorPos(ref _previousMousePosition);

        return true;
    }

    private (int height, int width) GetScreenResolution()
    {
        DEVMODE devMode = default;
        devMode.dmSize = (short)Marshal.SizeOf(devMode);
        EnumDisplaySettings(null, -1, ref devMode);

        return (devMode.dmPelsHeight, devMode.dmPelsWidth);
    }

    private Point GetNewCursorPosition(Point currentPos, int screenW, int screenH)
    {
        var stepX = _random.Next(0, MAX_MOUSE_STEP_SIZE);
        var stepY = _random.Next(0, MAX_MOUSE_STEP_SIZE);

        if (_random.NextDouble() > 1 - _changeOfDirectionChange)
            _directionX = -1 * _directionX;

        if (_random.NextDouble() > 1 - _changeOfDirectionChange)
            _directionY = -1 * _directionY;

        var (potentialX, potentialY) = (currentPos.X + _directionX * stepX, currentPos.Y + _directionY * stepY);

        if (potentialX > screenW || potentialX < 0)
            _directionX = -1 * _directionX;

        if (potentialY > screenH || potentialY < 0)
            _directionY = -1 * _directionY;

        return new Point(currentPos.X + _directionX * stepX, currentPos.Y + _directionY * stepY);
    }
}
