using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace backend.Infrastructure.Firebase;

public class FirebaseService
{
    private readonly FirebaseApp _app;
    private readonly FirestoreDb _firestore;

    public FirebaseService()
    {
        var privateKey = Environment.GetEnvironmentVariable("FIREBASE_PRIVATE_KEY")?.Replace("\\n", "\n");
        var clientEmail = Environment.GetEnvironmentVariable("FIREBASE_CLIENT_EMAIL");
        var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");

        if (string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(clientEmail) || string.IsNullOrEmpty(projectId))
        {
            throw new Exception("Firebase environment variables are not properly set.");
        }

        var config = new
        {
            type = Environment.GetEnvironmentVariable("FIREBASE_TYPE"),
            project_id = projectId,
            private_key_id = Environment.GetEnvironmentVariable("FIREBASE_PRIVATE_KEY_ID"),
            private_key = privateKey,
            client_email = clientEmail,
            client_id = Environment.GetEnvironmentVariable("FIREBASE_CLIENT_ID"),
            auth_uri = Environment.GetEnvironmentVariable("FIREBASE_AUTH_URI"),
            token_uri = Environment.GetEnvironmentVariable("FIREBASE_TOKEN_URI"),
            auth_provider_x509_cert_url = Environment.GetEnvironmentVariable("FIREBASE_AUTH_PROVIDER_X509_CERT_URL"),
            client_x509_cert_url = Environment.GetEnvironmentVariable("FIREBASE_CLIENT_X509_CERT_URL"),
            universe_domain = Environment.GetEnvironmentVariable("FIREBASE_UNIVERSE_DOMAIN")
        };

        var json = JsonConvert.SerializeObject(config);

        var credential = GoogleCredential.FromJson(json);

        _app = FirebaseApp.Create(new AppOptions
        {
            Credential = credential
        });

        _firestore = new FirestoreDbBuilder
        {
            ProjectId = projectId,
            Credential = credential
        }.Build();
    }

    public FirebaseAdmin.Auth.FirebaseAuth GetAuth() => FirebaseAdmin.Auth.FirebaseAuth.GetAuth(_app);
    public FirestoreDb GetFirestore() => _firestore;
}
