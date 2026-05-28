using DegeneraTube.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDatabase(builder.Configuration)
    .AddRepositories()
    .AddAppServices(builder.Configuration)
    .AddJwt(builder.Configuration)
    .AddSwagger()
    .AddControllers();

var app = builder.Build();

app.UseAppSwagger();
app.UseAppMiddleware();
app.UseCors();
app.MapControllers();

app.Run();

//TODO UNIT TESTS!!!