using Raven.Client.Listeners;
using Raven.Json.Linq;

namespace FunWithRavenDB {
    public class DocumentStoreListener : IDocumentStoreListener {
        public const string IS_ROUTABLE = "IsRoutable";

        public bool BeforeStore(string key, object entityInstance, RavenJObject metadata, RavenJObject original) {
            var document = entityInstance as IRoutable;
            if (document == null) {
                return false;
            }
            if (metadata.ContainsKey(IS_ROUTABLE) && metadata.Value<bool>(IS_ROUTABLE)) {
                return false;
            }
            metadata.Add(IS_ROUTABLE, true);
            return true;
        }

        public void AfterStore(string key, object entityInstance, RavenJObject metadata) {
        }
    }
}