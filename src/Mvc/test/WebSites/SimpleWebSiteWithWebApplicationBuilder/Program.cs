// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using static Microsoft.AspNetCore.Http.Results;

var app = WebApplication.Create(args);

app.MapGet("/", () => "Hello World");

app.MapGet("/json", () => Json(new Person("John", 42)));

app.MapGet("/ok-object", () => Ok(new Person("John", 42)));

app.MapGet("/accepted-object", () => Accepted("/ok-object", new Person("John", 42)));

app.MapGet("/many-results", (int id) =>
{
    if (id == -1)
    {
        return NotFound();
    }

    return Redirect("/json", permanent: true);
});

app.MapGet("/problem", () => Results.Problem("Some problem"));

app.Run();

record Person(string Name, int Age);
