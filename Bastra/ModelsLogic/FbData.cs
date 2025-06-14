using Bastra.Models;
using Bastra.Utilities;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Plugin.CloudFirestore;
using System.Text.Json;

namespace Bastra.ModelsLogic
{
    /// <summary>
    /// Provides an implementation of <see cref="FbDataModel"/> that connects to Firebase Authentication
    /// and Firestore services. Handles user authentication, Firestore data operations, and batch updates.
    /// </summary>
    public class FbData : FbDataModel
    {
        /// <summary>
        /// Firebase authentication client used for user sign-in and registration.
        /// </summary>
        private readonly FirebaseAuthClient facl;

        /// <summary>
        /// Firestore database instance used to perform data operations.
        /// </summary>
        private readonly IFirestore db;

        /// <summary>
        /// Write batch object used for grouping multiple Firestore write operations.
        /// </summary>
        private IWriteBatch batch;

        /// <summary>
        /// Object holding Firebase credentials and configuration data.
        /// </summary>
        private readonly FirebaseJson fbj;

        /// <summary>
        /// Builds and returns JSON serialization options that ignore case sensitivity in property names.
        /// </summary>
        /// <returns>A <see cref="JsonSerializerOptions"/> object configured to ignore case sensitivity.</returns>
        private static JsonSerializerOptions BuildSerializerSettings()
        {
            return new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FbData"/> class.
        /// Loads Firebase configuration from an embedded file, initializes the Firebase authentication client,
        /// and gets an instance of the Firestore database.
        /// </summary>
        public FbData()
        {
            JsonSerializerOptions options = BuildSerializerSettings();
            string fbJson = FirebaseJson.ReadEmbededTextFile();
            fbj = JsonSerializer.Deserialize<FirebaseJson>(fbJson, options);
            FirebaseAuthConfig fac = new()
            {
                ApiKey = fbj.ApiKey,
                AuthDomain = fbj.AuthDomain,
                Providers = new[] { new EmailProvider() }
            };
            facl = new FirebaseAuthClient(fac);
            db = CrossCloudFirestore.Current.Instance;
        }

        /// <summary>
        /// Creates a new user in Firebase Authentication using email, password, and display name.
        /// Invokes a callback action upon completion.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="name">The display name to assign to the new user.</param>
        /// <param name="OnComplete">A callback to invoke when the task completes.</param>
        public override async void CreateUserWithEmailAndPasswordAsync(string email, string password, string name, Action<Task> OnComplete)
        {
            await facl.CreateUserWithEmailAndPasswordAsync(email, password, name).ContinueWith(OnComplete);
        }

        /// <summary>
        /// Signs in an existing user using Firebase Authentication with the specified email and password.
        /// Invokes a callback action upon completion.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="OnComplete">A callback to invoke when the task completes.</param>
        public override async void SignInWithEmailAndPasswordAsync(string email, string password, Action<Task> OnComplete)
        {
            await facl.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(OnComplete);
        }

        /// <summary>
        /// Gets the display name of the currently signed-in user.
        /// Returns an empty string if no user is signed in.
        /// </summary>
        public string DisplayName => facl.User != null ? facl.User.Info.DisplayName : string.Empty;

        /// <summary>
        /// Gets the unique user ID (UID) of the currently signed-in user.
        /// </summary>
        public string UserId => facl.User.Uid;

        /// <summary>
        /// Writes the specified object to a Firestore document in the given collection with the provided ID.
        /// Invokes a callback action upon completion.
        /// </summary>
        /// <param name="obj">The object to store in Firestore.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="id">The ID of the document.</param>
        /// <param name="OnComplete">A callback to invoke when the task completes.</param>
        public override void SetDocument(object obj, string collectionName, string id, Action<Task> OnComplete)
        {
            IDocumentReference dr = db.Collection(collectionName).Document(id);
            dr.SetAsync(obj).ContinueWith(OnComplete);
        }

        /// <summary>
        /// Generates a new, unique document ID for a document in the specified Firestore collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection.</param>
        /// <returns>A new unique document ID as a string.</returns>
        public override string GetNewDocId(string collectionName)
        {
            return db.Collection(collectionName).Document().Id;
        }


        /// <summary>
        /// Adds a real-time snapshot listener to a Firestore collection.
        /// Invokes the provided handler whenever changes occur in the collection.
        /// </summary>
        /// <param name="collectionName">The name of the Firestore collection to listen to.</param>
        /// <param name="OnChange">The handler to invoke when the collection changes.</param>
        /// <returns>An <see cref="IListenerRegistration"/> instance used to manage the listener.</returns>
        public override IListenerRegistration AddSnapshotListener(string collectionName, QuerySnapshotHandler OnChange)
        {
            ICollectionReference cr = db.Collection(collectionName);
            return cr.AddSnapshotListener(OnChange);
        }

        /// <summary>
        /// Adds a real-time snapshot listener to a specific Firestore document.
        /// Invokes the provided handler whenever changes occur in the document.
        /// </summary>
        /// <param name="collectionName">The name of the Firestore collection.</param>
        /// <param name="id">The ID of the document to listen to.</param>
        /// <param name="OnChange">The handler to invoke when the document changes.</param>
        /// <returns>An <see cref="IListenerRegistration"/> instance used to manage the listener.</returns>
        public override IListenerRegistration AddSnapshotListener(string collectionName, string id, DocumentSnapshotHandler OnChange)
        {
            IDocumentReference dr = db.Collection(collectionName).Document(id);
            return dr.AddSnapshotListener(OnChange);
        }

        /// <summary>
        /// Deletes a document from a specified Firestore collection by its ID.
        /// Invokes a callback action upon completion.
        /// </summary>
        /// <param name="collectionName">The name of the collection containing the document.</param>
        /// <param name="id">The ID of the document to delete.</param>
        /// <param name="OnComplete">A callback to invoke when the deletion task completes.</param>
        public override void DeleteDocument(string collectionName, string id, Action<Task> OnComplete)
        {
            IDocumentReference dr = db.Collection(collectionName).Document(id);
            dr.DeleteAsync().ContinueWith(OnComplete);
        }

        /// <summary>
        /// Retrieves a list of document IDs from a specified collection where the "Created" timestamp
        /// is older than the provided cutoff date.
        /// </summary>
        /// <param name="collectionName">The name of the collection to query.</param>
        /// <param name="cutoffDate">The date used as the cutoff for filtering old documents.</param>
        /// <returns>A list of document IDs created before the cutoff date.</returns>
        public override async Task<List<string>> GetOldDocumentsAsync(string collectionName, DateTime cutoffDate)
        {
            ICollectionReference collection = db.Collection(collectionName);
            IQuerySnapshot snapshot = await collection.GetAsync();
            List<string> oldDocumentIds = [];
            foreach (IDocumentSnapshot doc in snapshot.Documents)
                if (doc.Data != null)
                {
                    Timestamp createdValue = (Timestamp)doc.Data["Created"];
                    if (createdValue is Timestamp ts)
                    {
                        DateTime createdDateTime = DateTimeOffset.FromUnixTimeSeconds(ts.Seconds).UtcDateTime;
                        createdDateTime = createdDateTime.AddTicks(ts.Nanoseconds / 100);
                        if (createdDateTime < cutoffDate)
                            oldDocumentIds.Add(doc.Id);
                    }
                }
            return oldDocumentIds;
        }

        /// <summary>
        /// Asynchronously deletes a document from a Firestore collection by its ID.
        /// </summary>
        /// <param name="collectionName">The name of the collection containing the document.</param>
        /// <param name="documentId">The ID of the document to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task DeleteDocumentAsync(string collectionName, string documentId)
        {
            IDocumentReference dr = db.Collection(collectionName).Document(documentId);
            await dr.DeleteAsync();
        }


        /// <summary>
        /// Retrieves all documents from a Firestore collection where a specific field matches the given value.
        /// Invokes a callback with the resulting query snapshot.
        /// </summary>
        /// <param name="collectionName">The name of the collection to query.</param>
        /// <param name="fName">The field name to compare.</param>
        /// <param name="fValue">The value to match against the field.</param>
        /// <param name="OnComplete">A callback that receives the query snapshot.</param>
        public override async void GetDocumentsWhereEqualTo(string collectionName, string fName, object fValue, Action<IQuerySnapshot> OnComplete)
        {
            ICollectionReference cr = db.Collection(collectionName);
            IQuerySnapshot qs = await cr.WhereEqualsTo(fName, fValue).GetAsync();
            OnComplete(qs);
        }

        /// <summary>
        /// Increments a numeric field in a specific Firestore document by a given amount.
        /// Invokes a callback when the operation completes.
        /// </summary>
        /// <param name="collectionName">The name of the collection containing the document.</param>
        /// <param name="id">The ID of the document to update.</param>
        /// <param name="fName">The name of the field to increment.</param>
        /// <param name="incrementBy">The amount to increment by.</param>
        /// <param name="OnComplete">A callback to invoke when the task completes.</param>
        public override void IncrementField(string collectionName, string id, string fName, long incrementBy, Action<Task> OnComplete)
        {
            IDocumentReference dr = db.Collection(collectionName).Document(id);
            dr.UpdateAsync(fName, FieldValue.Increment(incrementBy)).ContinueWith(OnComplete);
        }

        /// <summary>
        /// Updates a single field in a Firestore document with a new value.
        /// </summary>
        /// <param name="collectionName">The name of the collection containing the document.</param>
        /// <param name="id">The ID of the document to update.</param>
        /// <param name="fName">The name of the field to update.</param>
        /// <param name="fValue">The new value to assign to the field.</param>
        public override void UpdateField(string collectionName, string id, string fName, object fValue)
        {
            IDocumentReference dr = db.Collection(collectionName).Document(id);
            dr.UpdateAsync(fName, fValue);
        }

        /// <summary>
        /// Updates multiple fields in a Firestore document using a dictionary of field names and values.
        /// </summary>
        /// <param name="collectionName">The name of the collection containing the document.</param>
        /// <param name="id">The ID of the document to update.</param>
        /// <param name="dict">A dictionary containing field names and their corresponding new values.</param>
        public override void UpdateFields(string collectionName, string id, Dictionary<string, object> dict)
        {
            IDocumentReference dr = db.Collection(collectionName).Document(id);
            dr.UpdateAsync(dict);
        }

        /// <summary>
        /// Starts a new Firestore batch operation, allowing multiple updates to be committed together.
        /// </summary>
        public override void StartBatch()
        {
            batch = db.Batch();
        }

        /// <summary>
        /// Adds a field update operation to the current Firestore batch for a specific document.
        /// </summary>
        /// <param name="collectionName">The name of the collection containing the document.</param>
        /// <param name="id">The ID of the document to update.</param>
        /// <param name="fName">The name of the field to update.</param>
        /// <param name="fValue">The new value to assign to the field.</param>
        public override void BatchUpdateField(string collectionName, string id, string fName, object fValue)
        {
            IDocumentReference dr = db.Collection(collectionName).Document(id);
            batch?.Update(dr, fName, fValue);
        }

        /// <summary>
        /// Adds an increment operation to a numeric field in a document as part of the current Firestore batch.
        /// </summary>
        /// <param name="collectionName">The name of the collection containing the document.</param>
        /// <param name="id">The ID of the document to update.</param>
        /// <param name="fName">The name of the numeric field to increment.</param>
        /// <param name="incrementBy">The amount by which to increment the field.</param>
        public override void BatchIncrementField(string collectionName, string id, string fName, long incrementBy)
        {
            IDocumentReference dr = db.Collection(collectionName).Document(id);
            batch?.Update(dr, fName, FieldValue.Increment(incrementBy));
        }

        /// <summary>
        /// Commits all pending operations in the current Firestore batch.
        /// Invokes a callback action when the commit operation completes.
        /// </summary>
        /// <param name="OnComplete">A callback to invoke when the batch commit task completes.</param>
        public override void CommitBatch(Action<Task> OnComplete)
        {
            batch?.CommitAsync().ContinueWith(OnComplete);
        }

    }
}
