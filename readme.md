# Higher Order Regular Expressions for .Net

### Higher order

"Higher-order" speaks to the complexity of the data and of the expression.
While classic regex matches sequences of characters, HighRegex supports sequences of any type, for example a token in text processing or a datapoint.

In Functional Programming, "Higher Order Functions" are functions that take other functions as a parameter and/or return functions.  Similarly, higher order regular expressions are composed of other regular expressions.

#### HighRegex supports:

* Classes of arbitrary data types.  Matches exactly 1 position. (See [IClass](blob/master/HighRegex/IClass.cs))
* Expressions of arbitrary data types.  Matches exactly 0 or more positions. (See [IExpression](blob/master/HighRegex/IExpression.cs))
* Custom classes `[left straight]`, `[^right]` (See [SetClass](blob/master/HighRegex/SetClass.cs) and [NotClass](blob/master/HighRegex/NotClass.cs))
* Any class `.` (See [AnyClass](blob/master/HighRegex/AnyClass.cs))
* Repetition `straight+`, 'left{3,6}' (See [GreedyRepeatExpression](blob/master/HighRegex/GreedyRepeatExpression.cs))
* Optionals and lazy repetition `straight?`, `straight??`, 'left{3,6}?'  (See [RepeatExpression](blob/master/HighRegex/RepeatExpression.cs))
* Alternation `left|right`, `right|straight'  (See [AlternationExpression](blob/master/HighRegex/AlternationExpression.cs))
* Sequential `a b c`, `right left`  (See [ListExpression](blob/master/HighRegex/ListExpression.cs))
* Atomic anchors begining `^` and end `$` of sequence.  (See [StartExpression](blob/master/HighRegex/StartExpression.cs) and [EndExpression](blob/master/HighRegex/EndExpression.cs))
* Atomic look ahead `(?=left)` and negatives (?!right)`.  (See [LookAheadExpression](blob/master/HighRegex/LookAheadExpression.cs) and [NegativeLookAheadExpression](blob/master/HighRegex/NegativeLookAheadExpression.cs))
* Atomic look behind `(?<=left)` and negatives `(?<!right)`.  (See [LookBackExpression](blob/master/HighRegex/LookBackExpression.cs) and [NegativeLookBackExpression](blob/master/HighRegex/NegativeLookBackExpression.cs))

#### HighRegex does not support:

* [Grouping and back-referencing](http://www.regular-expressions.info/brackets.html) `(?<lastName>name)`
    - Grouping would be a great feature... Hopefully that is coming soon.
    - Back-referencing `\1` would be a nice feature, but may add complexity. Ideally, if breaking changes are necessary to support this, it would be opt-in.
* [Modifiers](http://www.regular-expressions.info/modifiers.html) `(?i)`, etc.
    - Since you have complete control over the classes, modifiers are probably not necessary.
* [Atomic Grouping](http://www.regular-expressions.info/atomic.html) `(?>right|left)` is not supported.
    - This would be a nice feature, and probably not too difficult.
* [Continuing matches](http://www.regular-expressions.info/continue.html)  `\G` is not supported.
    - Indirect support with [IExpression.IsMatchAt](blob/master/HighRegex/IExpression.cs)
* [Comments](http://www.regular-expressions.info/comments.html).  `(?# not supported)`
    - you'll probably be using an expression composed of self-documenting, named expressions, making comments unnecessary

