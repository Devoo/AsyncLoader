###![Icon](https://raw.githubusercontent.com/Devoo/AsyncLoader/master/Build/AsyncLogo.small.png) AsyncLoader: Dynamic Ajax Requests Made Easy

Helper classes to dynamic load content in an ASP.NET View using JQuery using **one line of code**

### How to Install
AsyncLoader are only two helper classes with no external dependencies (except jquery and asp.mvc)
Is distributed as a [NuGet](https://nuget.org/packages/Devoo.AsyncLoader) package.
It can be installed issuing the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

	PM> Install-Package Devoo.AsyncLoader

### Sample View Code

```csharp
 // Plain Text Async
 @Html.LoadAsync(() => "Plain Text")

 
 // Autorefresh async text
 // Each second an ajax call will be made to reload the new string
 @Html.LoadAsync(() => "Current Time: " + DateTime.Now.ToString(), TimeSpan.FromSeconds(1))
 
 // Delayed load of the async string
 @Html.LoadAsync(() =>
                {
                    Thread.Sleep(2000);
                    return "After slow function you get the result...";
                })

```
