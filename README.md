# unity-native-named-pipes  
a simple native implementations of `System.IO.Pipes.NamedPipeClientStream`.  

## supports
- unity IL2CPP
- Windows x64

## todo
- Linux 64  
- Mac  
- Unity Editor
- `NamedPipeServerStream`

## usage
```csharp
// on client
{
    var stream = new MessagePipeClientStream(".", "stream-s2c", PipeDirection.In);
    stream.Connect();
    stream.Read(...);
}


// on server
{
    NamedPipeServerStream stream = new NamedPipeServerStream("stream-s2c", PipeDirection.Out);
    stream.WaitForConnection();
    stream.Read(...);
}

```