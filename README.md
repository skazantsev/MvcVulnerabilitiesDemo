# ASP.NET - security fundamentals

Statistics shows that most of security breaches occur due to defects in software, so it is software developers who should be aware of security vulnerabilities and ways for detecting and preventing them.

It demonstrates some fundamentals of web security by the example of a simple ASP.NET MVC application which is vulnerable to several attacks.

## XSS
According to wikipedia cross-site scripting is accounted for 84% of security vulnerabilities.
Almost every server-side framework has built-in tools for preventing XSS; however, just some of them are secure by default.
Moreover, due to the growth in adoption of JavaScript and the explosion of client-side libraries, many of which are not secure by default, the number of XSS vulnerabilities will grow even further.

Even though ASP.NET MVC was developed as secure by default, ASP.NET developers must have awareness of the underlying mechanisms and use them properly.

### Explaining HTML-encoding

The ASP.NET MVC's Razor engine html-encodes strings by default:
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

It means that in unfavourable scenarios some malicious code can be inserted and executed by a victim's browser
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

### Server-side XSS
As it has been said before, in ASP.NET MVC output strings are html-encoded.
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

Invoking this helper on a page:
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
ASP.NET MVC outputs MvcHtmlString and any other class that implements IHtmlString without html encoding
because it has a load of built-in html helpers returning pieces of html which should be rendered without html encoding.

So, what we need to do is to encode only content of div by using a special method *SetInnerText*:
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

### XSS rendering JavaScript
If you html-encode every string on a page you might feel protected against XSS but it turns out that there is another weak spot when dealing with server-side rendering which is rendering JavaScript code.

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

One more important point is that in the aforementioned scenario we should use *text()* instead of *html()*, we'll talk about that in the next section.

### Client-side XSS
Client-side rendering has become really popular due to modern JavaScript libraries and frameworks allowing creating layouts on the fly in browser rather that rendering it on the server side.
However, developers have to use those libraries carefully and don't forget about security issues they might cause.

First of all, it's important to distinguish methods for working with html elements and methods dealing with text.
For example, in a popular JavaScript framework **jQuery** there are methods *text()* and *html()*.
The *text()* method html-escapes input and so should be used for inserting data.
Contrary, use *html()* if you want to build parts of the DOM (Document Object Model), but make sure that you still escape content of html elements.

To illustrate these points:
``` JavaScript
// WRONG
var userData = // get the user data from an untrusted source
$('#el').html(userData); // XSS!!!
```
``` html
<div id="el">
  <script>alert('Hello XSS')</script> // will be executed by browser
</div>
```

Use *text()*:
``` JavaScript
// CORRECT
var userData = // get the user data from an untrusted source
$('#el').text(userData);
```
``` html
<div id="el">
  &lt;script&gt;alert('Hello XSS')&lt;/script&gt; // will be displayed as text
</div>
```

Another significant caution is working with client-side templates.
Usually they're not safe by default and it's important to check documentation and use the correct syntax for XSS prevention.

It seems so wrong that the majority of tutorials for learning JavaScript templates use a syntax without html-encoding causing this code to be copied & pasted to real projects and leading to security breaches.

Let's illustrate it on the example of underscore.js templates:
``` html
<!-- WRONG -->
<script type="text/template" id="myTmpl">
  <div><%= item.text %></div> <!-- XSS -->
</script>
```

``` html
<!-- CORRECT -->
<script type="text/template" id="myTmpl">
  <div><%- item.text %></div>
</script>
```

Developers should consider using the safe syntax wherever it's possible (it may vary from library to library so, make sure you've checked the documentation first).

### Best practices for prevention XSS in ASP.NET
* Escape all the output strings by default
* Escape values in cookies and headers
* Generally, avoid generating html in plain C# classes, use razor helpers instead
* Use *html()* and *text()* jQuery methods appropriately
* Consider using [HtmlSanitizer](https://github.com/mganss/HtmlSanitizer) for cleaning html
* Know the difference between *InnerHtml* and *SetInnerText()* in html helpers
* Use *Ajax.JavaScriptStringEncode()* before rendering values in JavaScript on the server side
* Read [OWASP guide](https://www.owasp.org/index.php/XSS_%28Cross_Site_Scripting%29_Prevention_Cheat_Sheet)

## CSRF
CSRF or Cross-Site Request Forgery is one of the least underestimated kind of attack in web.
It can't be prevented by standard tools in browsers or OS and many web developers have never heard about it.
Moreover, the damage caused by this attack can be really huge in some scenarios.

### How does it work?
1. A user is signed in to a VULNERABLE-SITE1.COM
2. The user visits a MALICIOUS-SITE2.COM (there are many ways to get the user visit the malicious site)
3. MALICIOUS-SITE2.COM silently executes JavaScript making a request to VULNERABLE-SITE1.COM on-behalf of the user by posting a form with malicious parameters.
4. The victim's browser sends a request from the malicious site to the VULNERABLE-SITE1.COM with all needed cookies and malicious parameters
5. The request successfully executed by a hacker on-behalf of the user

An example:
``` html
<!-- VULNERABLE-SITE1.COM -->
<!-- a simple form for transferring money. What could possibly go wrong? -->
<form action="/Transfer" method="POST">
  <input name="credit-card-no" type="text" />
  <input name="amount" type="text" />
  <button type="submit">SUBMIT</button>
</form>
```

``` html
<!-- MALICIOUS-SITE2.COM -->
<!-- the form's action points to the vulnerable site -->
<form id="myForm" action="https://VULNERABLE-SITE1.COM/Transfer" method="POST" style="display:none;">
  <!-- malicious data -->
  <input name="credit-card-no" type="hidden" value="HACKER-CREDIT-CARD-NO" />
  <input name="amount" type="hidden" value="1000" />
</form>

<!-- auto-post the form when the page is loaded -->
<script>
  document.getElementById("myForm").submit();
</script>
```
When a user visits MALICIOUS-SITE2.COM it will transfer $1000 from the user's account to any credit card specified by a hacker!
This type of attack can be applied to any type of site requiring authentication (blogs, social networks, online banks, etc.) and almost for any action (posting a comment, changing a password, following a friend)

### CSRF protection
The method of protection against CSRF is fairly simple and it's supported by many web frameworks.
Rendering a form, a special token should be put in a hidden field and in user's cookies.
When the user submits that form to the server the value in the posted data will be compared to the value in the cookies and the request will be executed only if the values are equal.

A token is usually called Anti CSRF token and implementing this mechanism by itself might be tricky and prone to security vulnerabilities.
Hopefully, ASP.NET MVC has a built-in support for it by the html helper's extension *Html.AntiForgeryToken* and the attribute for verifying tokens - *ValidateAntiForgeryToken*.

We can protect our form as follows:
``` csharp
@using (Html.BeginForm("Tranfer"))
{
    @Html.AntiForgeryToken()
    @Html.TextBoxFor(x => x.CreditCardNo)
    @Html.TextBoxFor(x => x.Amount)
    <button type="submit">SUBMIT</button>
}

``` csharp
[ValidateAntiForgeryToken]
public ActionResult Transfer(TransferViewModel model)
{
    // ...
}
```

Now if a hacker gets a victim to submit malicious form the server will return the 500 error because the hacker doesn't know the valid token value.

### CSRF and AJAX requests
What about AJAX requests? The same-origin policy disallows execution of AJAX requests to another domain so we don't need any tokens schmokens, right?

Yes, you need. The same-origin policy certainly prevents CSRF from another domains (unless you [allowed them](https://en.wikipedia.org/wiki/Cross-origin_resource_sharing));
however, if your site is exploited to XSS the same-origin policy can be bypassed and all your AJAX requests will be vulnerable to CSRF attacks.

So you have to use CSRF tokens for AJAX requests.

### Best practices for prevention CSRF
* Use Anti CSRF tokens for unsafe actions
* Use Anti CSRF tokens for AJAX requests
* Use built-in mechanisms, don't reinvent the wheel

## License
The code and text is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt) for more details.
