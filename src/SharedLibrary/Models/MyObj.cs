namespace CasCap.Models;

[MessagePackObject(true)]
public class MyObj : IEquatable<MyObj?>
{
    public MyObj(string? str) => this.str = str;

    public Guid id { get; set; } = Guid.NewGuid();
    public DateTime date { get; set; } = DateTime.UtcNow;
    public string? str { get; set; }

    public override bool Equals(object? obj) => Equals(obj as MyObj);

    public bool Equals(MyObj? other)
    {
        return other != null &&
               id.Equals(other.id) &&
               date == other.date &&
               str == other.str;
    }

    public override int GetHashCode()
    {
        var hashCode = -1135332943;
        hashCode = hashCode * -1521134295 + id.GetHashCode();
        hashCode = hashCode * -1521134295 + date.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(str);
        return hashCode;
    }
}