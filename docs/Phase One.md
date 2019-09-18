# Phase One High-Level Concepts and Planning

Focus on low-level queue reading scenario(s) where we simply need the message ID, receipt handle and the body.

- Define a lightweight message type
- Define a ReceiveSingleMessage method
- Define a ReceiveMultipleMessages method

## Future ideas

- Include a helper type which can take custom `Span` based parsing logic to read the body bytes and deserialise / create the <T> type of the user wants. This may avoid unneccesary storage of data the consumer does not require.

For example:

A client needs to read the JSON message body for two properties

```json
{
    "PropA": "ValueA",
    "PropB": "ValueB",
    "PropC": "ValueC",
    "PropD": "ValueD"
}
```

```csharp
public class MyType
{
    public string PropertyA { get; set; }
    public string PropertyB { get; set; }
}
```

Rather than store the raw body bytes, if we know up-front that the consumer needs the MyType object + the message ID and receipt handle. Was can probably parse the bytes in one pass via the reader/parser.
