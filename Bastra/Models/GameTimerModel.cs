using System.ComponentModel;

namespace Bastra.Models
{
    public abstract class GameTimerModel
    {
        private double timeLeft;
        public double TimeLeft
        {
            get => timeLeft;
            set
            {
                if (timeLeft != value)
                {
                    timeLeft = value;
                }
            }
        }

        public abstract void Start(double duration);
        public abstract void Stop();
    }
}