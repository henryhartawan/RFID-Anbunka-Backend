using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Services.Implementations;
using RFIDP2P3_API.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;

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

var requireAuthPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter(requireAuthPolicy)); 
}).AddJsonOptions(options => 
{ 
    options.JsonSerializerOptions.PropertyNamingPolicy = null; 
});

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
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Masukkan token JWT dengan format: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement 
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var jwtConfig = builder.Configuration.GetSection("JWT");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtConfig["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
        };
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
//app.UseMiddleware<APIKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToController("Index", "Login");

app.Run();
