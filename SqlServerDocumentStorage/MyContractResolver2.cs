using System;
using Newtonsoft.Json.Serialization;

namespace SqlServerDocumentStorage
{
    public class MyContractResolver2<T> : IContractResolver
    {
        private readonly IUpdateContext<T> context;
        private readonly DefaultContractResolver ContractResolver = new DefaultContractResolver();

        public MyContractResolver2(IUpdateContext<T> context)
        {
            this.context = context;
        }

        public JsonContract ResolveContract(Type type)
        {
            var contract = ContractResolver.ResolveContract(type);

            if (typeof (T) == type)
            {
                var joc = contract as JsonObjectContract;

                foreach (var index in context.Indices)
                {
                    var property = joc.Properties.GetClosestMatchProperty(index.Name);
                    if (property != null)
                    {
                        joc.Properties.Remove(property);
                    }
                }

                //remove id from Data column
                var idProperty = joc.Properties.GetClosestMatchProperty("Id");
                if (idProperty != null)
                {
                    joc.Properties.Remove(idProperty);
                }
            }


            return contract;
        }
    }
}