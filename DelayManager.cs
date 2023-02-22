using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenTeams;

internal class DelayManager
{
    private const double ACCELERATION_DIRECTION_CHANGE_CHANCE = 0.1; 
    
    private readonly int _idleDelay, _minMoveDelay, _maxMoveDelay;
    private bool _accelarationDirection;

    private int _delay;
    
    private readonly Random _random = new();

    

    internal DelayManager(int idleDelay, int minMoveDelay, int maxMoveDelay) =>
        (_idleDelay, _minMoveDelay, _maxMoveDelay) = (idleDelay, minMoveDelay, maxMoveDelay);

    internal int GetNext()
    {
        if(_delay < _minMoveDelay || _delay > _maxMoveDelay)
        {
            _delay = _random.Next(_minMoveDelay, _maxMoveDelay);
            return _delay;
        }

        if (_random.NextDouble() > 1 - ACCELERATION_DIRECTION_CHANGE_CHANCE)
            _accelarationDirection = !_accelarationDirection;

        var directionMultiplier = _accelarationDirection ? 1 : -1;
        _delay = (int)(_delay + (_random.Next(0, 10) / 100d) * _delay * directionMultiplier);

        return _delay;
    }

    internal int GetDefaultDelay() => _idleDelay;
}
