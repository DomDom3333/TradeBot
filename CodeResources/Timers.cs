using System.Timers;
using Timer = System.Timers.Timer;

namespace AlpacaExample.CodeResources;

public class Timers
{
    public Timer MinutelySynced { get; private set; }
    public Timer HourlySynced { get; private set; }
    public Timer DailySynced { get; private set; }
    public Timer WeeklySynced { get; private set; }
    public Timer YearlySynced { get; private set; }
    
    internal Timers()
    {
        MinutelySynced  = new Timer(60*1000);
        HourlySynced  = new Timer(60*60*1000);
        DailySynced  = new Timer(60*60*24*1000);
        WeeklySynced  = new Timer(60*60*24*7*1000);
        YearlySynced  = new Timer(60*60*24*7*52);
        Timer syncTimer = new Timer(100);
        syncTimer.Elapsed += new ElapsedEventHandler(AttemptSync);
        syncTimer.Start();

    }

    private void AttemptSync(object source, ElapsedEventArgs e)
    {
        if (DateTime.Now.Minute == 0)
        {
            
        }
    }
}