using Android;

namespace Bastra.Platforms.Android
{
    public class NotificationPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        {
            get
            {
                List<(string,bool)> result = [];//new List<(string androidPermission, bool isRuntime)>();
                if (OperatingSystem.IsAndroidVersionAtLeast(33))
                    result.Add((Manifest.Permission.PostNotifications, true));
                return [.. result]; //result.ToArray();
            }
        }
    }
}
