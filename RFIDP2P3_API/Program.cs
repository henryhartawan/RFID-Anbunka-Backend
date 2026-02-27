using RFIDP2P3_API.Middleware;
using Microsoft.OpenApi.Models;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Services.Implementations;
using RFIDP2P3_API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => 
{
    
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("https://localhost:7144", "https://127.0.0.1:7144") 
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
}
);
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

var app = builder.Build();

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
