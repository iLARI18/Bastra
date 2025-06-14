using System.Reflection;

namespace Bastra.Utilities
{
    public class FirebaseJson
    {
        public ProjectInfo? Project_Info { get; set; }
        public Client[]? Client { get; set; }
        public string? Configuration_Version { get; set; }
        public string ProjectId
        {
            get
            {
                return Project_Info.Project_Id;
            }
        }
        public static string JsonPath
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] names = assembly.GetManifestResourceNames();
                return names[0];
            }
        }
        public string AuthDomain
        {
            get
            {
                return ProjectId + ".firebaseapp.com";
            }
        }

        public string StorageBucket
        {
            get
            {
                return Project_Info.Storage_Bucket;
            }
        }

        public string ApiKey
        {
            get
            {
                return Client[0].Api_Key[0].Current_Key;
            }
        }
        public static string ReadEmbededTextFile()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            using Stream? stream = assembly.GetManifestResourceStream(names[0]);
            using StreamReader reader = new(stream);
            string fileData = reader.ReadToEnd();
            return fileData;
        }
    }

    public class ProjectInfo
    {
        public string? Project_Number { get; set; }
        public string? Project_Id { get; set; }
        public string? Storage_Bucket { get; set; }
    }

    public class Client
    {
        public ClientInfo? Client_Info { get; set; }
        public object[]? Oauth_Client { get; set; }
        public ApiKey[] Api_Key { get; set; }
        public Services? Services { get; set; }
    }
    public class ClientInfo
    {
        public string? Mobilesdk_App_Id { get; set; }
        public AndroidClientInfo? Android_Client_Info { get; set; }
    }

    public class AndroidClientInfo
    {
        public string? Package_Name { get; set; }
    }

    public class Services
    {
        public AppinviteService? Appinvite_Service { get; set; }
    }

    public class AppinviteService
    {
        public object[]? Other_Platform_Oauth_Client { get; set; }
    }

    public class ApiKey
    {
        public string? Current_Key { get; set; }
    }
}
