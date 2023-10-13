using MessagePack;
using Newtonsoft.Json;
using StackExchange.Redis;
namespace CasCap.Extensions;

public static class Helpers
{
    public static string ToJSON(this object obj) => JsonConvert.SerializeObject(obj, Formatting.None);

    public static string ToJSON(this object obj, Formatting formatting) => JsonConvert.SerializeObject(obj, formatting);

    public static T? FromJSON<T>(this string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch// (Exception ex)
        {
            //Debug.WriteLine(ex);
            throw;
        }
    }

    public static T? FromJSON<T>(this RedisValue result) => FromJSON<T>(result.ToString());

    public static byte[] ToMessagePack<T>(this T data) => MessagePackSerializer.Serialize(data);

    public static T FromMessagePack<T>(this byte[] bytes) => MessagePackSerializer.Deserialize<T>(bytes);

    public static T FromMessagePack<T>(this RedisValue result) => MessagePackSerializer.Deserialize<T>((byte[])result);
}