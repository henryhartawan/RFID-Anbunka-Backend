using RFIDP2P3_API.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => 
{
    //options.AddPolicy(name: MyAllowSpecificOrigins,
    //  policy =>
    //  {
    //	  policy.WithOrigins("https://localhost:5144");
    //  });
    //options.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    options.AddDefaultPolicy(
        builder =>
        {
            //builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(origin => true).AllowAnyOrigin();
            builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(origin => true).AllowAnyOrigin();
        }
    );
}
);
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(
     options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; }
 );
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
app.UseCors();
app.UseMiddleware<APIKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToController("Index", "Login");

app.Run();
