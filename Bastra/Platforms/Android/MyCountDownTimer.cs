using Android.OS;
using Bastra.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace Bastra.Platforms.Android
{
    public class MyCountDownTimer : CountDownTimer
    {
        public MyCountDownTimer(long millisInFuture, long countDownInterval) : base(millisInFuture, countDownInterval)
        {
            Start();
        }

        public override void OnFinish()
        {
            WeakReferenceMessenger.Default.Send(new AppMessage<long>(0));
        }

        public override void OnTick(long millisUntilFinished)
        {
            WeakReferenceMessenger.Default.Send(new AppMessage<long>(millisUntilFinished));
        }
    }
}
