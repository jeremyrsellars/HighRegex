---
layout: page
title: Higher-order Regular Expressions
tagline: Higher-order Regular Expressions for .Net
---
{% include JB/setup %}

#### So, what is a higher-order Regex?
"Higher order" speaks to the complexity of both the data and of the expression declaration. [Read more](lessons/2013/06/15/what-is-it)

Coming soon: Compare ideas from System.Text.RegularExpressions to HighRegex.

### More Information

<ul class="posts">
  {% for post in site.posts %}
    <li><span>{{ post.date | date_to_string }}</span> &raquo; <a href="{{ BASE_PATH }}{{ post.url }}">{{ post.title }}</a></li>
  {% endfor %}
</ul>

