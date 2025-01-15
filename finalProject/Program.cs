using FINALPROJECT.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

    });


});

// Register repository services
builder.Services.AddScoped<interfaceRepastory, UserRepastory>();
builder.Services.AddScoped<CarsinterfaceRepastory, CarsRepastory>();
builder.Services.AddScoped<RentCarInterFace, RentCarRepastory>();
builder.Services.AddScoped<MaintenanceInterFace, MaintenanceRepository>();
builder.Services.AddScoped<SparePartInterface, SparePartRepositry>();




// Ensure the TokenKey is available in configuration
string? tokenKeyString = builder.Configuration.GetValue<string>("AppSittings:TokenKey");
if (string.IsNullOrEmpty(tokenKeyString))
{
    throw new InvalidOperationException("TokenKey is not set in the configuration.");
}

SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKeyString));
var credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha512Signature);

TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
{
    IssuerSigningKey = tokenKey,
    ValidateIssuer = false,
    ValidateIssuerSigningKey = true,
    ValidateAudience = false
};

// JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = tokenValidationParameters;
    });

var app = builder.Build();
app.UseCors("DevCors");
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
 
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
