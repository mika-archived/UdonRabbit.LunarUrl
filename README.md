# UdonRabbit.LunarUrl

URL parser for VRChat Udon and UdonSharp

## Requirements

- VRCSDK3 WORLD 2021.05.21.12.13 or greater
- UdonSharp v0.19.11 or greater

## Usage

You have to copy and paste the following files:

- `Assets/NatsunekoLaboratory/UdonRabbit/LunarUrl/SimpleDictionary.cs`
- `Assets/NatsunekoLaboratory/UdonRabbit/LunarUrl/UrlParser.cs`

and write code like below example:

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
  _parser.GetHost(); // www.youtube.com
  _parser.GetAbsolutePath(); // /watch

  var dict = _parser.GetQuery();
  dict.GetItem("v"); // r2jgQuOmO48
  dict.GetItem("t"); // 929s
}
```

## ScreenShot

<img src="https://user-images.githubusercontent.com/10832834/119802656-af3c8a00-bf19-11eb-853b-22490435ba44.png" />

## License

MIT by [@6jz](https://twitter.com)
