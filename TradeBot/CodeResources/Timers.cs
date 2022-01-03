using Timer = System.Timers.Timer;

namespace TradeBot.CodeResources;

internal class Timers
{
    internal Timer SecondlySynced { get; private set; }
    internal Timer MinutelySynced { get; private set; }
    internal Timer HourlySynced { get; private set; }
    internal Timer DailySynced { get; private set; }
    internal Timer WeeklySynced { get; private set; }
    
    internal Timers()
    {
        SecondlySynced  = new Timer(1000);
        MinutelySynced  = new Timer(60*1000);
        HourlySynced  = new Timer(60*60*1000);
        DailySynced  = new Timer(60*60*24*1000);
        WeeklySynced  = new Timer(60*60*24*7*1000);
        StartTimers();
    }

    private void StartTimers()
    {
        WeeklySynced.Start();
        DailySynced.Start();
        HourlySynced.Start();
        MinutelySynced.Start();
        SecondlySynced.Start();
    }

    internal void AddSub(in Timer timer, Action methode)
    {
        timer.Elapsed += (s,e) =>
        {
            methode.Invoke();
        };
    }
    
}