using System.Timers;
using Timer = System.Timers.Timer;

namespace AlpacaExample.CodeResources;

internal class Timers
{
    internal Timer MinutelySynced { get; private set; }
    internal Timer HourlySynced { get; private set; }
    internal Timer DailySynced { get; private set; }
    internal Timer WeeklySynced { get; private set; }
    
    internal Timers()
    {
        MinutelySynced  = new Timer(60*1000);
        HourlySynced  = new Timer(60*60*1000);
        DailySynced  = new Timer(60*60*24*1000);
        WeeklySynced  = new Timer(60*60*24*7*1000);
    }

    private void StartTimers()
    {
        WeeklySynced.Start();
        DailySynced.Start();
        HourlySynced.Start();
        MinutelySynced.Start();
    }

    internal void AddSub(Timer timer, Action methode)
    {
        timer.Elapsed += delegate(object? sender, ElapsedEventArgs args)
        {
            methode.Invoke();
        };
    }
    
}