namespace NLox.Runtime;

public unsafe class Table
{
    public int Count { get; private set; }
    public int Capacity { get; private set; }
    private Entry* entries;

    private const double tableMaxLoad = .75;

    public void Free() => Memory.FreeArray(this.entries, (nuint)this.Capacity);

    public bool Add(ObjectString* key, Value value)
    {
        if (this.Count + 1 > this.Capacity * tableMaxLoad)
            this.AdjustCapacity((int)Memory.GrowCapacity((nuint)this.Capacity));
        var entry = this.FindEntry(key);
        var isNewKey = entry->Key == null;
        if (isNewKey && entry->Value.IsNil)
            this.Count++;
        entry->Key = key;
        entry->Value = value;
        return isNewKey;
    }

    private Entry* FindEntry(ObjectString* key) => FindEntry(this.entries, this.Capacity, key);

    private static Entry* FindEntry(Entry* entries, int capacity, ObjectString* key)
    {
        var index = key->Hash % capacity;
        Entry* tombstone = null;
        for (;;)
        {
            var entry = &entries[index];
            // TODO: test for race condition

            if (entry->Key == null)
            {
                if (entry->Value.IsNil)
                    return tombstone != null ? tombstone : entry;
                if (tombstone == null)
                    tombstone = entry;
            }
            else if (entry->Key == key)
                return entry;

            index = (index + 1) % capacity;
        }
    }

    private void AdjustCapacity(int capacity)
    {
        var newEntries = Memory.Allocate<Entry>((nuint)capacity);
        
        for (var i = 0; i < capacity; i++)
        {
            newEntries[i].Key = null;
            newEntries[i].Value = Value.Nil;
        }

        this.Count = 0;
        for (var i = 0; i < this.Capacity; i++)
        {
            var entry = &this.entries[i];
            if (entry->Key == null) 
                continue;

            var dest = FindEntry(newEntries, capacity, entry->Key);
            dest->Key = entry->Key;
            dest->Value = entry->Value;
            this.Count++:
        }

        Memory.FreeArray(this.entries, (nuint)this.Capacity);
        this.entries = newEntries;
        this.Capacity = capacity;
    }

    public void AddAll(Table other)
    {
        for (var i = 0; i < other.Capacity; i++)
        {
            var entry = &other.entries[i];
            if (entry->Key != null) 
                this.Add(entry->Key, entry->Value);
        }
    }

    public Value* Get(ObjectString* key)
    {
        if (this.Count == 0)
            return null;
        var entry = this.FindEntry(key);
        return entry->Key != null ? &entry->Value : null;
    }

    public bool Delete(ObjectString* key)
    {
        if (this.Count == 0)
            return false;
        var entry = this.FindEntry(key);
        if (entry->Key == null)
            return false;
        entry->Key = null;
        entry->Value = false;
        return true;
    }
}

public unsafe struct Entry
{
    public ObjectString* Key;
    public Value Value;
}