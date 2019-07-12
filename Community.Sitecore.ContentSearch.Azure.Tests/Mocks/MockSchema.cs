using Sitecore.ContentSearch.Azure.Models;
using Sitecore.ContentSearch.Azure.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Community.Sitecore.ContentSearch.Azure.Tests.Mocks
{
    public class MockSchema : ICloudSearchIndexSchema
    {
        private List<IndexedField> fields = new List<IndexedField>();

        public void Add(IndexedField field) =>
            fields.Add(field);

        public IEnumerable<IndexedField> AllFields => throw new NotImplementedException();

        public ICollection<string> AllFieldNames => throw new NotImplementedException();

        public IndexedField GetFieldByCloudName(string cloudName)
        {
            return fields.FirstOrDefault(x => x.Name == cloudName);
        }
    }
}
