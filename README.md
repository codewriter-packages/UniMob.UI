# UniMob.UI [![Github license](https://img.shields.io/github/license/codewriter-packages/UniMob.UI.svg?style=flat-square)](#) [![Unity 2019.3](https://img.shields.io/badge/Unity-2019.3+-2296F3.svg?style=flat-square)](#) ![GitHub package.json version](https://img.shields.io/github/package-json/v/codewriter-packages/UniMob.UI?style=flat-square)

A declarative library for building reactive user interface. Built over [UniMob](https://github.com/codewriter-packages/UniMob).

## Core concepts

Widgets are the building blocks of a app’s user interface, and each widget is an immutable declaration of part of the user interface.
Widgets form a hierarchy based on composition.

UniMob.UI comes with a suite of powerful basic widgets, of which the following are commonly used:

> **[Row](./Runtime/Widgets/Row.cs), [Column](./Runtime/Widgets/Column.cs)**<br/>
> These widgets let you create layouts in both the horizontal (Row) and vertical (Column) directions. 
>
>**[ZStack](./Runtime/Widgets/ZStack.cs)**<br/>
>A Stack widget lets you place widgets on top of each other in paint order.
>
>**[Container](./Runtime/Widgets/Container.cs)**<br/>
>The Container widget lets you create a rectangular visual element that has background color and custom size.

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
        return new Container {
            BackgroundColor = Color.cyan,
            Child = new Column {
                Children = {
                    new UniMobText(WidgetSize.Fixed(400, 50)) {
                        Value = "Tap count: " + Counter,
                        FontSize = 40,
                    },
                    new UniMobButton {
                        OnClick = () => Counter += 1,
                        Child = new UniMobText(WidgetSize.Fixed(400, 50)) {
                            Value = "Increment",
                            FontSize = 40,
                        }
                    }
                }
            }
        };
    }
}
```

More code samples are located in  [UniMob.UI Samples](https://github.com/codewriter-packages/UniMob.UI-Samples) repository.

## Custom widgets

Built-in widgets provide basic functionality, but modern games requires more complex and unique interfaces. You can create your own widgets to implement it.

#### 1. Create widget

The widget contains the immutable data that is required for a user interface part.

```csharp
using UniMob.UI;

public class RealCounterWidget : StatefulWidget
{
    public int IncrementStep { get; set; }

    public override State CreateState() => new RealCounterState();
}
```

#### 2. Create state

The State provides data for the View and optionally contains mutable state of this interface part.

```csharp
using UniMob;
using UniMob.UI;

public class RealCounterState : ViewState<RealCounterWidget>
{
    public override WidgetViewReference View => WidgetViewReference.Resource("Prefabs/Real Counter View");

    [Atom] public int Counter { get; private set; }

    public void Increment()
    {
        Counter += Widget.IncrementStep;
    }
}
```

#### 3. Create view

View describes how data provided by State should be rendered on the screen.

```csharp
using UniMob.UI;

public class RealCounterView : View<RealCounterState>
{
    public UnityEngine.UI.Text counterText;
    public UnityEngine.UI.Button incrementButton;

    protected override void Awake()
    {
        base.Awake();

        incrementButton.Click(() => State.Increment);
    }

    protected override void Render()
    {
        counterText.text = State.Counter.ToString();
    }
}
```

## How to Install
Minimal Unity Version is 2019.3.

Library distributed as git package ([How to install package from git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html))
<br>Git URL (UniMob.UI): `https://github.com/codewriter-packages/UniMob.UI.git`
<br>Git URL (UniMob): `https://github.com/codewriter-packages/UniMob.git`

## License

UniMob.UI is [MIT licensed](./LICENSE.md).

## Credits

UniMob.UI inspired by [Flutter](https://github.com/flutter/flutter).
