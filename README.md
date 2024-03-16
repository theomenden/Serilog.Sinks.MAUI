# Serilog.Sinks.MAUI

### Hi all! Thanks for taking the time to read this

### I'd like to first of all give lots of credits to [Serilog, Serilog Contributors](https://serilog.net/), and the many developers that lead me to this point.
_Disclaimer: I am not a part of, nor am I affiliated with Serilog in any way._
- When it comes down to it, this project was based out of frustration that I couldn't find a direct DotNet MAUI implementation for Serilog across platforms.
So I took a look at a few sources of code
1. [Serilog.Sinks.EventLog](https://github.com/serilog/serilog-sinks-eventlog)
2. [Serilog.Sinks.Xamarin](https://github.com/serilog/serilog-sinks-xamarin)

These two provided the launching point for our exploration into this package, and hopefully provides a well based solution using the combination of these sources to provide logging for
1. NSLog via Apple
2. AndroidLog via Android
3. EventLog via Windows

This should allow you to have multi-targeting platforms using the same serilog setup. 

When it comes to installation you can either
- Simply download the package from nuget
- Using Visual Studio Command prompt ```Package-Install Serilog.Sinks.MAUI```

Then to implement it throughout your project you can set up the extensions to work via the `Serilog` namespace by adding `using serilog;` to your `program.cs` file. 
You can then add `WriteTo.AndroidLog(<configure-options>)`, `WriteTo.NSLog(<configure-options>)`, or `WriteTo.EventLog(<configure-options>)` for the platform(s) of your choice.
```csharp
Log.Logger = new LoggerConfiguration()
.MinimumLevel.Verbose()
.Enrich.FromLogContext()
.WriteTo.AndroidLog()
.CreateLogger();
```
And with [Serilog.Sinks.Async](https://github.com/serilog/serilog-sinks-async) package
```csharp
Log.Logger = new LoggerConfiguration()
.MinimumLevel.Verbose()
.Enrich.FromLogContext()
.WriteTo.Async(a => a.AndroidLog())
.CreateLogger();
```
This should also be configurable via the [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) package in an `appsettings.json` file
```json
"Serilog":{
  "Using": [
  /*using declarations*/
  "Serilog.Sinks.MAUI"
  ]
  "WriteTo": [
  {"Name":"AndroidLog", "Args":{ /* other parameters ... */}},
  {"Name": "Console"},
  {"Name": "File", "Args": { "path": "Logs/log.txt" } }
  /*other logging sinks*/
  ]
}
```
You should also be able to set it up with the [Serilog.Sinks.Async](https://github.com/serilog/serilog-sinks-async) package as well
```json
  {
  "Serilog": {
    "WriteTo": [{
      "Name": "Async",
      "Args": {
        "configure": [{
          "Name": "AndroidLog"
        }]
      }
    }]
  }
}
```
## Note This will only work with .NET8 project targets
