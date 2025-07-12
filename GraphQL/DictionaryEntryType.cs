using System;

namespace FormBuilderAPI.GraphQL;

public class DictionaryEntryType : ObjectType<System.Collections.DictionaryEntry>
{
    protected override void Configure(IObjectTypeDescriptor<System.Collections.DictionaryEntry> descriptor)
    {
        descriptor.Field("key")
                  .Resolve(context => context.Parent<System.Collections.DictionaryEntry>().Key);
        descriptor.Field("value")
                  .Resolve(context => context.Parent<System.Collections.DictionaryEntry>().Value);
    }
}
