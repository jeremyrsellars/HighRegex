---
layout: post
category : lessons
tagline: "Getting Started with Class"
tags : [intro, beginner, tutorial, class]
---
Classic Regex has several built-in character classes, like the digit class (`\d`), the word character class, `\w`, whitespace class `\s`.  Regexes are composed of classes with repetition, alternation, etc..

Today we'll implement a class that the HighRegex engine can use.  You write the Class, and HighRegex provides the alternation, repetition, etc..

### The Data
Let's consider a contrived example: You have a stream of go-cart steering wheel position data that is geo-tagged.  The data points are captured 4 times per second.  You have a series of data from 100 tracks from around the world and you want to find a track for a race that has particular features.

```c#
public enum Direction
{
  Left = -1,
  Straight = 0,
  Right = 1,
}
public class WheelSample
{
  public float Rotation{get;}
  public GeoLocation Location{get;}
  public Direction Direction
  {
    get
    {
      return
        Rotation < -.1 ? Direction.Left :
        Rotation > .1 ? Direction.Right : Direction.Straight;
    }
  }
}

/*  Example data:
    t=0   Left(-.1)
    t=1   Left(-.2)
    t=2   Straight(-.01)
    t=3   Straight(.01)
    t=4   Straight(.05)
    t=5   Right(.14)
    t=6   Right(.1)
    t=7   Right(.1)
    t=8   Straight(-.03)
    t=9   Straight(-.06)
*/
```

### The class

The HighRegex engine uses expression classes that implement `IExpression<T>`.  IExpression answers the question "Does this expression match input at a particular index, and how long is the match?".  `IClass<T>` implements `IExpression<T>` and adds `bool IsMatch(T item)`.  The abstract `Class<T>` implements `IClass<T>` and by extension `IExpression<T>`, so all you have to do is override `override bool IsMatch(Direction direction)` with your specific logic.  Let's do that now, for the WheelSample class.

```c#
public class DirectionClass : Class<WheelSample>
{
  readonly Direction direction;
  public DirectionClass(string directionName)
  {
    direction = ParseDirection(direction);
  }
  public override bool IsMatch(Direction direction)
  {
    return this.Direction == direction;
  }
}

```

This class can be configured to match a particular direction, as in:

```c#
var right = new DirectionClass("right");
var left = new DirectionClass("left");
var straight = new DirectionClass("straight");
// straight for 30 samples
var straightAway =
  new RepeatExpression<WheelSample>(straight, 30);
```

__So why does the constructor take a `string` parameter, instead of the `enum Direction`?__  Learn why in our next installment (hint: it has to do with parsing expressions in HighRegex format).

### In Closing

Well, that was easy.  Extend a class and then HighRegex can use your class and add alternation, repetition, atomic lookaround, etc..

Next time, we'll look at parsing expressions so that you can use Regex syntax with your own custom classes.
