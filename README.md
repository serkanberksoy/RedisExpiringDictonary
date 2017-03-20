# RedisExpiringDictonary
An expiring and sliding dictionary wrapper with Stackexchange.Redis

# Usage
```csharp
var configurationOptions = new ConfigurationOptions
{
    EndPoints ={
        { "127.0.0.1", 6379 }
    },
    SyncTimeout = 10000
    // Ssl = true,
    // Password = "mypassword"
};

RedisExpiringDictionary<string, TestObject> dic = new RedisExpiringDictionary<string, TestObject>(
    configurationOptions,
    3,
    new TimeSpan(0, 0, 5),
    true);

// SET
dic.Set(id.ToString(), new TestObject
{
    Id = id,
    Name = temp
});

// GET
var testObj = dic.Get(id.ToString())

// REMOVE
dic.Remove(id.ToString());
```
