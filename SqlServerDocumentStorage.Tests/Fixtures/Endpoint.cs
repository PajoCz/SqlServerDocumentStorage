using System.Collections.Generic;

namespace SqlServerDocumentStorage.Tests.Fixtures
{
    public class Endpoint
    {
        public HttpMethod Method { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Request Request { get; set; }
        public ICollection<Response> Responses { get; set; } = new List<Response>();
    }
}