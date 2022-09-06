using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Templates.MinimalApi;
using Templates.MinimalApi.Auth;
using Templates.MinimalApi.Data;
using Templates.MinimalApi.Endpoints;
using Templates.MinimalApi.Models;
using Templates.MinimalApi.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
  Args = args,
  //WebRootPath = "./wwwroot",
  //EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
  //ApplicationName = "Library.Api"
});

builder.Services.AddCors(options =>
{
  options.AddPolicy("AnyOrigin", x => x.AllowAnyOrigin());
});

builder.Services.Configure<JsonOptions>(options =>
{
  options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
  options.JsonSerializerOptions.IncludeFields = true;
});

builder.Configuration.AddJsonFile(
  "appsettings.Local.json", true, true);

// Authentication
builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
  .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

// Swashbuckle specific services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Db Setup
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
  new SQLServerConnectionFactory(
    builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<DatabaseInitializer>();

// Application Services
//builder.Services.AddSingleton<IBookService, BookService>();
builder.Services.AddLibraryEndpoints();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// let's build some middleware
var app = builder.Build();

app.UseCors();

// Swagger at https://localhost/swagger
app.UseSwagger(options => { });
app.UseSwaggerUI(options => { });

app.UseAuthorization();

app.UseLibraryEndpoints();

//.RequireCors("AnyOrigin");// added with attribute above
//.ExcludeFromDescription();// perfect to remove from swagger

// Db initialation
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

// application start
app.Run();
