---
layout: post
category : lessons
tagline: "API Comparison"
tags : [intro, beginner, tutorial]
---
Today's let's talk about differences between APIs.

HighRegex is intended to provide a familiar interface to users of System.Text.RegularExpressions.

<table>
  <thead><tr><td></td><td><strong>System.Text.RegularExpressions.<br>Regex</td><td><strong>HighRegex.<br>IExpression</td></tr></thead>
  <tr><td><strong>regex.Match(input)</td><td><code></td><td><code></td></tr>
  <tr><td><li>defined as</td><td><code>Member Method</td><td><code>Extension Method</td></tr>
  <tr><td><li>input</td><td><code>String</td><td><code>IExpressionItemSource&lt;T&gt;</td></tr>
  <tr><td><li>returns</td><td><code>Match</td><td><code>Match&lt;T&gt;</td></tr>
  <tr>
  <tr><td><strong>regex.Matches(input)</td><td><code></td><td><code></td></tr>
  <tr><td><li>defined as</td><td><code>Member Method</td><td><code>Extension Method</td></tr>
  <tr><td><li>input</td><td><code>String</td><td><code>IExpressionItemSource&lt;T&gt;</td></tr>
  <tr><td><li>returns</td><td><code>MatchCollection</td><td><code>MatchCollection&lt;T&gt;</td></tr>
  <tr>
  <tr><td><strong>Regex type(s)</td><td></td><td></td></tr>
  <tr><td><li>defined as</td><td><code>class Regex</td><td><code>interface IExpression&lt;T&gt;</td></tr>
  <tr>
  <tr><td><strong>public class Match</td><td></td><td></td></tr>
  <tr><td><li>defined as</td><td><code>class Match : Group</td><td><code>class Match&lt;T&gt;</td></tr>
  <tr><td><li>success indicator</td><td><code>bool Success{get;}</td><td><code>bool Success{get;}</td></tr>
  <tr><td><li>length property</td><td><code>int Length{get;}</td><td><code>int Length{get;}</td></tr>
  <tr><td><li>match begins at</td><td><code>int Index{get;}</td><td><code>int Index{get;}</td></tr>
  <tr><td><li>matched items</td><td><code>string Value{get;}</td><td><code>IList&lt;T&gt; Items{get;}</tr>
  <tr><td><li>next match</td><td><code>match.NextMatch()</td><td><code>N/A</td></tr>
  <tr><td><li>replace</td><td><code>match.Result(replacement)</td><td><code>N/A</td></tr>
  <tr><td><li>groups</td><td><code>match.Groups</td><td><code>N/A</td></tr>
  <tr><td><li>captures</td><td><code>match.Captures</td><td><code>N/A</td></tr>
</table>

You may also be interested in the list of [supported expressions](../../../../../#HighRegex-Supports).

### In Closing

I think you'll find a familiar API for most of your regex needs.

Before we can really get started, we'll need to create an implementation of `IClass<T>`.  We'll do that next time.
