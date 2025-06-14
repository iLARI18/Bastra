using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Bastra.ModelsLogic;
using Bastra.Utilities;

namespace Bastra.Platforms.Android
{
    [Service]
    public class DeleteOldDocsService : Service
    {
        private bool running = true;
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            ThreadStart ts = new(DeleteOldDocumentsAsync);
            Thread thread = new (ts);
            thread.Start();
            return base.OnStartCommand(intent, flags, startId);
        }

        private async void DeleteOldDocumentsAsync()
        {
            FbData fbData = new();
            DateTime cutoffDate;
            List<string> oldDocumentsId;
            while (running)
            {
                cutoffDate = DateTime.UtcNow.AddDays(-2);
                oldDocumentsId = await fbData.GetOldDocumentsAsync(Constants.collectionName, cutoffDate);
                foreach (string docId in oldDocumentsId)
                    await fbData.DeleteDocumentAsync(Constants.collectionName, docId);
                Thread.Sleep(43200000);
            }
            StopSelf();
        }

        public override void OnDestroy()
        {
            running = false;
            base.OnDestroy();
        }

        public override IBinder? OnBind(Intent? intent) => null;
    }
}
