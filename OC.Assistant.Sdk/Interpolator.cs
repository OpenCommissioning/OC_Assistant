namespace OC.Assistant.Sdk;

/// <summary>
/// Interpolating a value with a constant update rate.
/// </summary>
public class Interpolator
{
    private DateTime _startTime = DateTime.Now;
    private readonly double _stepTime;
    private double _target;
    private double _actual;
    private double _delta;

    /// <summary>
    /// New Instance of the <see cref="Interpolator"/> class.
    /// </summary>
    /// <param name="stepTime">The expected update rate of the value in seconds.</param>
    public Interpolator(double stepTime)
    {
        _stepTime = stepTime;
    }

    /// <summary>
    /// Calculates the current value.
    /// </summary>
    /// <param name="value">The value to be interpolated.</param>
    /// <returns></returns>
    public double Calculate(double value)
    {
        var timeNow = DateTime.Now;

        if (HasValueChanged(value))
        {
            _target = value;
            _startTime = timeNow;
            _delta = _target - _actual;
        }

        var deltaTime = (timeNow - _startTime).TotalSeconds;

        if (deltaTime >= _stepTime)
        {
            _actual = _target;
            return _actual;
        }
        
        _actual = _target - _delta + _delta * (deltaTime / _stepTime);
        return _actual;
    }

    private bool HasValueChanged(double value)
    {
        return Math.Abs(value - _target) > 0.000001;
    }
}