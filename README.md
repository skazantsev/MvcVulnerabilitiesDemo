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

Furthermore, ASP.NET MVC has a built-in HtmlHelper's method *Html.Raw* for rendering strings without encoding which should be used only for trusted data.

###XSS rendering JavaScript
If you html-encode every string on a page you might feel protected against XSS but it turns out that there is another weak spot when dealing with server-side rendering is rendering JavaScript.

Look at the following piece of code:
``` csharp
<script>
  $(function () {
    var name = '@Model.Name'; // Model.Name = "<script>alert('Hello XSS')</script>"
    $('#name').html(name);
  });
</script>
```
=>
``` html
<script>
  $(function () {
    var name = '&lt;script&gt;alert('Hello XSS')&lt;/script&gt';
    $('#name').html(name);
  });
</script>
```
As you can see the output is html-encoded; however, this code is still vulnerable to XSS.
By using hexadecimal representation of some characters we can pass a malicious script to Model.Name and it will be ignored by Html.Encode method and interpreted as an html string in browser so we end up with XSS.
``` csharp
<script>
  $(function () {
    var name = '@Model.Name'; // Model.Name = "\x3cscript\x3ealert(\x27Hello XSS\x27)\x3c/script\x3e"
    $('#name').html(name);
  });
</script>
```
=>
``` html
<script>
  $(function () {
    var name = '\x3cscript\x3ealert(\x27Hello XSS\x27)\x3c/script\x3e';
    $('#name').html(name); // insert and execute <script>alert('Hello XSS')</script> !!!
  });
</script>
```

To prevent this we should use @Ajax.JavaScriptStringEncode (just a wrapper for HttpUtility.JavaScriptStringEncode) which will correctly encode backward slashes.
The following code is secure to this type of vulnerability:
``` csharp
<script>
  $(function () {
    var name = '@Ajax.JavaScriptStringEncode(Model.Name)'; // Model.Name = "\x3cscript\x3ealert(\x27Hello XSS\x27)\x3c/script\x3e"
    $('#name').html(name);
  });
</script>
```
=>
``` html
<script>
  $(function () {
    var name = '\\x3cscript\\x3ealert(\\x27Hello\\x27)\\x3c/script\\x3e';
    $('#name').html(name);
  });
</script>
```

One more important point is that in the aforementioned scenario we should use $.text instead of $.html, we'll talk about that in the next section.
