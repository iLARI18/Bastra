using CommunityToolkit.Mvvm.Messaging;
using Bastra.Models;

namespace Bastra.ModelsLogic
{
    public class GameTimer : GameTimerModel
    {
        public GameTimer()
        {
            WeakReferenceMessenger.Default.Register<AppMessage<long>>(this, (r, m) =>
            {
                OnMessageReceived(m.Value);
            });
        }

        public override void Start(double duration)
        {
            TimerSetting timerSetting = new()
            {
                CountDownInterval = 1000,
                MillisInFuture = (long)(duration * 1000)
            };
            WeakReferenceMessenger.Default.Send(new AppMessage<TimerSetting>(timerSetting));
        }

        public override void Stop()
        {
            WeakReferenceMessenger.Default.Send(new AppMessage<string>("StopTimer"));
        }

        private void OnMessageReceived(long timeLeft)
        {
            TimeLeft = (double)(timeLeft / 1000);
        }
    }
}
