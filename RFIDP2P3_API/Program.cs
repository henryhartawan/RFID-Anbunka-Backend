using RFIDP2P3_API.Middleware;
using Microsoft.OpenApi.Models;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Services.Implementations;
using RFIDP2P3_API.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins(allowedOrigins) 
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(
     options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; }
 );

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IMfaService, MfaService>();
builder.Services.AddScoped<IEmailService, EmailService>();
//Rate Limit
builder.Services.AddSingleton<System.Collections.Concurrent.ConcurrentDictionary<string, (
    int Count, System.DateTime WindowStartUtc, System.DateTime LastHitUtc)>>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "RFIDP2P3_API", Version = "v1" });
    options.AddSecurityDefinition("XApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key Must Apper in Header",
        In = ParameterLocation.Header,
        Name = "XApiKey",
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement 
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "XApiKey",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "XApiKey"
                },
            },
            new string[]{ }
        }
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseMiddleware<APIKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToController("Index", "Login");

app.Run();
