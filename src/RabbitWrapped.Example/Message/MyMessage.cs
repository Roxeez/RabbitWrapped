using System.Text.Json.Serialization;

namespace RabbitWrapped.Example.Message;

public class MyMessage : IMessage
{
    [JsonPropertyName("username")] public string Username { get; init; }
}