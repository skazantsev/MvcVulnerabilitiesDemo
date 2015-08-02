# Web-application security fundamentals

Statistics shows that most of security breaches occur due to defects in software, so it is software developers who should be aware of security vulnerabilities and ways for detecting and preventing them.

This project is built for a web-security training at [Kaspersky Lab](http://www.kaspersky.com/about).
It demonstrates some fundamentals of web security by the example of simple ASP.NET MVC application which is vulnerable to several attacks.

##XSS
According to wikipedia cross-site scripting is accounted for 84% of security vulnerabilities.
Almost every server-side framework has built-in tools for preventing XSS, however just some of them are secure by default.
Moreover, due to the growth in adoption of JavaScript and the explosion of client-side libraries which are not secure by default the number of XSS vulnerabilities will grow even further.

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

Without encoding symbols '<', '>' will be interpreted by browser as beginings and endings of html tags rather than text:
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
