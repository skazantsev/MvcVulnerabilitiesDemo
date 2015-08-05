# Web-application security fundamentals

Statistics shows that most of security breaches occur due to defects in software, so it is software developers who should be aware of security vulnerabilities and ways for detecting and preventing them.

This project is built for a web-security training at [Kaspersky Lab](http://www.kaspersky.com/about).
It demonstrates some fundamentals of web security by the example of simple ASP.NET MVC application which is vulnerable to several attacks.

##XSS
According to wikipedia cross-site scripting is accounted for 84% of security vulnerabilities.
Almost every server-side framework has built-in tools for preventing XSS, however just some of them are secure by default.
Moreover, due to the growth in adoption of JavaScript and the explosion of client-side libraries many of which are not secure by default, the number of XSS vulnerabilities will grow even further.

Even though ASP.NET MVC was developed as secure by default, ASP.NET developers must have awareness of the underlying mechanisms and use them properly.

###Explaining HTML-encoding

ASP.NET MVC Razor engine html encodes strings by default:
``` csharp
@{
    var text = "<HTML> is awesome!";
}
<div>
    @text
</div>
```

Output:
``` html
<div>
    &lt;HTML&gt; is awesome!
</div>
```
<hr>

Without encoding symbols '<', '>' will be interpreted by browser as beginnings and endings of html tags rather than text:
``` csharp
@{
    var text = "<HTML> is awesome!";
}
<div>
    @Html.Raw(text)
</div>
```

Output:
``` html
<div>
    <HTML> is awesome!
</div>
```
<hr>

That means that in unfavourable scenarios some malicious code can be inserted and executed by a victim's browser
``` csharp
@{
    var text = "<script>var cookies = document.cookie; // send them to a hacker </script>";
}
<div>
    @Html.Raw(text)
</div>
```

Output:
``` html
<div>
    <script>
        // get cookies
        var cookies = document.cookie;
        // send them to the hacker
        // ....
    </script>
</div>
```

###Server-side XSS
As it has been said before in ASP.NET MVC output strings are html-encoded.
However, it doesn't mean that developers are fully protected against XSS.
Look at the following html helper:
``` csharp
public static class InsecureHtmlHelpers
{
    public static MvcHtmlString FormatAccount(this HtmlHelper html, string username)
    {
        var tagBuilder = new TagBuilder("div");
        tagBuilder.InnerHtml = username;
        return MvcHtmlString.Create(tagBuilder.ToString());
    }
}
```

Invoking this helper on a page
``` csharp
@Html.FormatAccount("<b>John Smith</b>")
```
Its output will be:
``` html
<div><b>John Smith</b></div>
```
and not:
``` html
<div>&lt;b&gt;John Smith&lt;/b&gt;</div>
```
or:
``` html
&lt;div&gt;&lt;b&gt;John Smith&lt;/b&gt;&lt;/div&gt;
```
But why is that?

Well, let's look at the description of MvcHtmlString:
``` csharp
/// <summary>
/// Represents an HTML-encoded string that should not be encoded again.
/// </summary>
public sealed class MvcHtmlString : HtmlString
{
  // ...
}
```
ASP.NET MVC outputs MvcHtmlString and any other class implemented IHtmlString without html encoding
because it has a load of built-in html helpers returning pieces of html which should be rendered without html encoding.

So what we need to do is to encode only content of div by using a special method *SetInnerText*:
``` csharp
public static class InsecureHtmlHelpers
{
    public static MvcHtmlString FormatAccount(this HtmlHelper html, string username)
    {
        var tagBuilder = new TagBuilder("div");
        tagBuilder.SetInnerText(username);
        return MvcHtmlString.Create(tagBuilder.ToString());
    }
}
```

Now the output will be:
``` html
<div>&lt;b&gt;John Smith&lt;/b&gt;</div>
```
