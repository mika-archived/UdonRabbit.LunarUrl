# UdonRabbit.LunarUrl

URL parser for VRChat Udon and UdonSharp

## Requirements

- VRCSDK3 WORLD 2021.05.21.12.13 or greater
- UdonSharp v0.19.11 or greater

## Usage

```csharp
[SerializeField]
private VRCUrlInputField _input;

[SerializeField]
private UrlParser _parser;

// Called by uGUI
public void OnHandleUrlInput()
{
   var url = _input.GetUrl(); // https://www.youtube.com/watch?v=r2jgQuOmO48&t=929s
  _parser.Parse(url);

  _parser.GetScheme(); // https
  _parser.GetHost(); // www.;youtube.com
  _parser.GetAbsolutePath(); // watch

  var dict = _parser.GetQuery();
  dict.GetItem("v"); // r2jgQuOmO48
  dict.GetItem("t"); // 929s
}
```

## License

MIT by [@6jz](https://twitter.com)
