using System;
using System.Timers;

namespace Aggregator
{
    //hac-ky-di-hack 
    public class TimerFactory
    {
        public event Action<Myargs> TimerExpiredEvent;
        public void CreateTimer(string name, int runTime){
            JustTimer justTimer = new JustTimer();
            justTimer.SomeTimer.Elapsed += (obj, ea) =>{
                var args = new Myargs();
                args.Name = name;
                sendEvent(args);
            };
            justTimer.SetAndGo(name, runTime);
        }

        private void sendEvent(Myargs args){
            if(TimerExpiredEvent != null)
                TimerExpiredEvent.Invoke(args);
        }
    }

    // extending eventargs class to include name prop.
    public class Myargs : EventArgs
    {
        public string Name { get; set; }
    }

    //extending timer class to include name -> differentiate between timer events
    public class JustTimer : Timer
    {
        public Timer SomeTimer = new Timer();
        public string Name { get; set; }
        public void SetAndGo(string name, int runTime){
            SomeTimer.Interval = runTime;
            SomeTimer.AutoReset = false;
            Name = name;
            SomeTimer.Start();
        }
    }
}