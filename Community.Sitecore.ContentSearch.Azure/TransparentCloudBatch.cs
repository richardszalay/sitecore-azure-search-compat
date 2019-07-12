using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sitecore.ContentSearch.Azure;

namespace Community.Sitecore.ContentSearch.Azure
{
    public class TransparentCloudBatch : CloudBatch
    {
        [DebuggerStepThrough]
        public TransparentCloudBatch(IEnumerable<Dictionary<string, object>> documents) : base(documents)
        {
            this.Documents = documents.ToList();
        }

        public ICollection<Dictionary<string, object>> Documents { get; }
    }
}
