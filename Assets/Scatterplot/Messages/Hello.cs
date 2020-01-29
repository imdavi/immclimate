using Newtonsoft.Json;

[JsonObject]
public class Hello : Message
{
    public Hello() : base(MessageType) { }

    public static string MessageType { get; } = "hello";

    
    public static Message Message { get; private set; } = new Hello();
}