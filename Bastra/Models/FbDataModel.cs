using Firebase.Auth;
using Plugin.CloudFirestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastra.Models
{
    public abstract class FbDataModel
    {
        protected readonly FirebaseAuthClient facl;
        protected readonly IFirestore db;
        protected IWriteBatch batch;

        public abstract void CreateUserWithEmailAndPasswordAsync(string email, string password, string name, Action<Task> OnComplete);
        public abstract void SignInWithEmailAndPasswordAsync(string email, string password, Action<Task> OnComplete);
        public abstract void SetDocument(object obj, string collectionName, string id, Action<Task> OnComplete);
        public abstract string GetNewDocId(string collectionName);
        public abstract IListenerRegistration AddSnapshotListener(string collectionName, QuerySnapshotHandler OnChange);
        public abstract IListenerRegistration AddSnapshotListener(string collectionName, string id, DocumentSnapshotHandler OnChange);
        public abstract void DeleteDocument(string collectionName, string id, Action<Task> OnComplete);
        public abstract Task<List<string>> GetOldDocumentsAsync(string collectionName, DateTime cutoffDate);
        public abstract Task DeleteDocumentAsync(string collectionName, string documentId);
        public abstract void GetDocumentsWhereEqualTo(string collectionName, string fName, object fValue, Action<IQuerySnapshot> OnComplete);
        public abstract void IncrementField(string collectionName, string id, string fName, long incrementBy, Action<Task> OnComplete);
        public abstract void UpdateField(string collectionName, string id, string fName, object fValue);
        public abstract void UpdateFields(string collectionName, string id, Dictionary<string, object> dict);
        public abstract void StartBatch();
        public abstract void BatchUpdateField(string collectionName, string id, string fName, object fValue);
        public abstract void BatchIncrementField(string collectionName, string id, string fName, long incrementBy);
        public abstract void CommitBatch(Action<Task> OnComplete);
    }
}
