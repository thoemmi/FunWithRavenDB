using System.Threading;
using NUnit.Framework;
using Raven.Client.Embedded;

namespace FunWithRavenDB {
    [TestFixture]
    public class RoutingServiceTests {
        private EmbeddableDocumentStore _store;

        public class Document : IRoutable {
            public string Id { get; set; }
            public string Path { get; set; }
        }

        [SetUp]
        public void Setup() {
            _store = new EmbeddableDocumentStore {
                RunInMemory = true
            };
            _store.RegisterListener(new DocumentStoreListener());
            _store.Initialize();
            new IRoutable_ByPath().Execute(_store);

            using (var session = _store.OpenSession()) {
                session.Store(new Document { Path = "" });
                session.Store(new Document { Path = "a" });
                session.Store(new Document { Path = "a/b" });
                session.Store(new Document { Path = "a/b/c" });
                session.Store(new Document { Path = "a/d" });
                session.Store(new Document { Path = "a/d/e" });
                session.Store(new Document { Path = "a/f" });
                session.SaveChanges();
            }

            while (_store.DocumentDatabase.Statistics.StaleIndexes.Length > 0) {
                Thread.Sleep(50);
            }
        }

        [Test]
        public void RoutableFlagIsSetInMetadata() {
            using (var session = _store.OpenSession()) {
                foreach (var document in session.Query<Document>()) {
                    Assert.IsTrue(session.Advanced.GetMetadataFor(document).Value<bool>("IsRoutable"));
                }
            }
        }

        [Test]
        public void CanGetDocumentAtRoot() {
            using (var session = _store.OpenSession()) {
                var sut = new RoutableResolver(session);
                var result = sut.GetRoutable("");

                Assert.IsNotNull(result);
                Assert.AreEqual("", result.Path);
            }
        }

        [Test]
        public void CanGetDocumentAtFirstLevel() {
            using (var session = _store.OpenSession()) {
                var sut = new RoutableResolver(session);
                var result = sut.GetRoutable("a");

                Assert.IsNotNull(result);
                Assert.AreEqual("a", result.Path);
            }
        }

        [Test]
        public void CanGetDocumentAtThirdLevel() {
            using (var session = _store.OpenSession()) {
                var sut = new RoutableResolver(session);
                var result = sut.GetRoutable("a/b/c");

                Assert.IsNotNull(result);
                Assert.AreEqual("a/b/c", result.Path);
            }
        }

        [Test]
        public void GetClosestParent() {
            using (var session = _store.OpenSession()) {
                var sut = new RoutableResolver(session);
                var result = sut.GetRoutable("a/b/c/x/y");

                Assert.IsNotNull(result);
                Assert.AreEqual("a/b/c", result.Path);
            }
        }

        [Test]
        public void CannotFindNonExistingDocumentAtRoot() {
            using (var session = _store.OpenSession()) {
                var sut = new RoutableResolver(session);
                var result = sut.GetRoutable("x");

                Assert.IsNull(result);
            }
        }

        [Test]
        public void CannotFindNonExistingDocumentAndNoParent() {
            using (var session = _store.OpenSession()) {
                var sut = new RoutableResolver(session);
                var result = sut.GetRoutable("x/y/z");

                Assert.IsNull(result);
            }
        }
    }
}