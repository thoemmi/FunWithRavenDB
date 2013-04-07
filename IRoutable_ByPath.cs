using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace FunWithRavenDB {
public class IRoutable_ByPath : AbstractIndexCreationTask {
    public override IndexDefinition CreateIndexDefinition() {
        return new IndexDefinition {
            Map = @"from doc in docs where doc[""@metadata""][""" + DocumentStoreListener.IS_ROUTABLE + @"""].ToString() == ""True"" select new { doc.Path }"
        };
    }
}
}