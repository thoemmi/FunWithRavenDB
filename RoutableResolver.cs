using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Linq;

namespace FunWithRavenDB {
    public class RoutableResolver {
        private readonly IDocumentSession _documentSession;

        public RoutableResolver(IDocumentSession documentSession) {
            _documentSession = documentSession;
        }

        public IRoutable GetRoutable(string path) {
            var query = _documentSession
                .Query<IRoutable, IRoutable_ByPath>();

            if (!String.IsNullOrEmpty(path)) {
                var pathParts = path.Split('/');
                for (var i = 1; i <= pathParts.Length; ++i) {
                    var shortenedPath = String.Join("/", pathParts, startIndex : 0, count : i);
                    query = query.Search(doc => doc.Path, shortenedPath, boost : i, options : SearchOptions.Or);
                }
            } else {
                query = query.Where(doc => doc.Path == String.Empty);
            }

            var document = query.Take(1).FirstOrDefault();
            return document;
        }
    }

    public interface IRoutable {
        string Id { get; set; }
        string Path { get; set; }
    }
}