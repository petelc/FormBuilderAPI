using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace FormBuilderAPI.GraphQL;

public class MyDictionaryType : ObjectType<MyCustomDictionary>
{
    protected override void Configure(IObjectTypeDescriptor<MyCustomDictionary> descriptor)
    {
        descriptor.Field("entries")
                  .Resolve(context =>
                  {
                      var dictionary = context.Parent<MyCustomDictionary>();
                      // Transform dictionary entries into your custom DictionaryEntry GraphQL type
                      return dictionary.Select(kvp => new DictionaryEntry { Key = kvp.Key, Value = kvp.Value });
                  });
    }
}

public class MyCustomDictionary : IDictionary<string, object>
{
    private readonly Dictionary<string, object> _dict = new();

    public object this[string key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }

    public ICollection<string> Keys => _dict.Keys;

    public ICollection<object> Values => _dict.Values;

    public int Count => _dict.Count;

    public bool IsReadOnly => ((IDictionary<string, object>)_dict).IsReadOnly;

    public void Add(string key, object value) => _dict.Add(key, value);

    public void Add(KeyValuePair<string, object> item) => ((IDictionary<string, object>)_dict).Add(item);

    public void Clear() => _dict.Clear();

    public bool Contains(KeyValuePair<string, object> item) => ((IDictionary<string, object>)_dict).Contains(item);

    public bool ContainsKey(string key) => _dict.ContainsKey(key);

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => ((IDictionary<string, object>)_dict).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dict.GetEnumerator();

    public bool Remove(string key) => _dict.Remove(key);

    public bool Remove(KeyValuePair<string, object> item) => ((IDictionary<string, object>)_dict).Remove(item);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => _dict.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();
}
