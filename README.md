# UniMob.UI [![Github license](https://img.shields.io/github/license/codewriter-packages/UniMob.UI.svg?style=flat-square)](#) [![Unity 2019.3](https://img.shields.io/badge/Unity-2019.3+-2296F3.svg?style=flat-square)](#) ![GitHub package.json version](https://img.shields.io/github/package-json/v/codewriter-packages/UniMob.UI?style=flat-square)

A declarative library for building reactive user interface. Built over [UniMob](https://github.com/codewriter-packages/UniMob).

## A quick example

So what does code that uses UniMob.UI look like?

```csharp
using UniMob;
using UniMob.UI;
using UniMob.UI.Widgets;
using UnityEngine;

public class CounterApp : UniMobUIApp
{
    [Atom] private int Counter { get; set; }

    protected override Widget Build(BuildContext context)
    {
        return new Container
        {
            BackgroundColor = Color.cyan,
            Child = new UniMobButton
            {
                OnClick = () => Counter += 1,
                Child = new UniMobText(WidgetSize.Fixed(400, 100))
                {
                    Value = "Tap count: " + Counter,
                    FontSize = 40,
                    MainAxisAlignment = MainAxisAlignment.Center,
                    CrossAxisAlignment = CrossAxisAlignment.Center,
                }
            }
        };
    }
}
```

More code samples are located in  [UniMob.UI Samples](https://github.com/codewriter-packages/UniMob.UI-Samples) repository.

## How to Install
Minimal Unity Version is 2019.3.

Library distributed as git package ([How to install package from git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html))
<br>Git URL (UniMob.UI): `https://github.com/codewriter-packages/UniMob.UI.git`
<br>Git URL (UniMob): `https://github.com/codewriter-packages/UniMob.git`

## License

UniMob.UI is [MIT licensed](./LICENSE.md).

## Credits

UniMob.UI inspired by [Flutter](https://github.com/flutter/flutter).
