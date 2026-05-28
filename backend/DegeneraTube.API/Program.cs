using DegeneraTube.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDatabase(builder.Configuration)
    .AddRepositories()
    .AddAppServices(builder.Configuration)
    .AddJwt(builder.Configuration)
    .AddSwagger()
    .AddControllers();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.WithOrigins(builder.Configuration["Cors:Origin"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

app.UseAppSwagger();
app.UseAppMiddleware();
app.UseCors();
app.MapControllers();

app.Run();