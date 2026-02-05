using System.Text;
using Laboratorio.Api;
using Laboratorio.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtOptions = new JwtOptions();
builder.Configuration.GetSection("Jwt").Bind(jwtOptions);
if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) || jwtOptions.SigningKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");
}

var apiKeyCryptoOptions = new ApiKeyCryptoOptions();
builder.Configuration.GetSection("ApiKeyCrypto").Bind(apiKeyCryptoOptions);

var securityOptions = new SecurityOptions
{
    SessionCookieName = builder.Environment.IsDevelopment() ? "lab_session" : "__Host-Session",
    JwtCookieName = builder.Environment.IsDevelopment() ? "lab_jwt" : "__Host-Access",
    XsrfCookieName = "XSRF-TOKEN",
    XsrfHeaderName = "X-XSRF-TOKEN",
    SessionMinutes = builder.Configuration.GetValue<int>("Security:SessionMinutes", 60)
};

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton(apiKeyCryptoOptions);
builder.Services.AddSingleton(securityOptions);
builder.Services.AddSingleton<SessionStore>();
builder.Services.AddSingleton<ApiKeyUserStore>();
builder.Services.AddSingleton<ApiKeyCryptoService>();
builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(securityOptions.JwtCookieName, out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Session", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new SessionRequirement());
    });
});

builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, SessionHandler>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200", "https://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
