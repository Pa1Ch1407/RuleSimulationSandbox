# AI usage

Fill this document with your actual development history before submission.

## Tools and models used

- Tool: Chatgpt

## Rejected or substantially rewritten suggestions
I have issue with swagger enabling So, I asked help to chatgpt as below
by default when API loads I need swaggger to be enabled I used Swashbuckle.AspNetCore 10.2.3 version. Also, In program.cs I added the configurations UsesWagger(), UseSwaggerUI() in services binding this needed to render swagger on running the code but not happening. Can you suggest more on this
### Example 1
- Suggested approach: 
To automatically open Swagger when you run the application, configure your Properties/launchSettings.json.
For example:
{
  "profiles": {
    "MyApi": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7001;http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
The important two settings are:
"launchBrowser": true,
"launchUrl": "swagger"

Then when you press F5 / Run, Visual Studio should open:

https://localhost:7001/swagger

instead of just:

https://localhost:7001/
- Why rejected: Not rejected
- Final approach: Accepted the same and did changes it worked.

## My design decisions

Already I wrote what I am aware of enabling swagger for api UI.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
});

var app = builder.Build();

// Swagger should be enabled before MapControllers
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    options.RoutePrefix = "swagger";
});
Forgot to add some configurations in launch settings.json

## Generated vs handwritten
- Generated/AI-assisted: 20%
- Handwritten/substantially rewritten: 80%
