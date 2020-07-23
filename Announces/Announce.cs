using BCA.Common;
using System;
using System.Timers;

namespace AstralBot.Announces
{
    public class Announce
    {
        public event Action<string> SendAnnounce;
        public event Action<Announce> EndAnnounce;

        public PlayerInfo Creator { get; set; }
        public short Repetition { get; set; }
        public string Txt { get; set; }
        public TimeSpan Interval { get; set; }

        private Timer _timer;

        public Announce(PlayerInfo creator, string txt, short repetition, short interval)
        {
            Creator = creator;
            Txt = txt;
            Repetition = repetition;
            Interval = TimeSpan.FromMinutes(interval);

            _timer = new Timer();
            _timer.Interval = Interval.TotalMilliseconds;
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendAnnounce?.Invoke(Txt);
            Repetition--;
            if (Repetition == 0)
            {
                _timer.Stop();
                EndAnnounce?.Invoke(this);
            }
        }

        public override string ToString()
        {
            return string.Format("Responsable : {0} | Répétitions : {1} | Interval (en minutes) : {2} | Texte : {3}", Creator.Username, Repetition, Interval.TotalMinutes, Txt);
        }
    }
}
