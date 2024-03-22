using Azure.Storage.Blobs;
using Backend_PcAuction.Auth;
using Backend_PcAuction.Auth.Model;
using Backend_PcAuction.Auth.Models;
using Backend_PcAuction.Data;
using Backend_PcAuction.Data.DbSeeders;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Data.Seeders;
using Backend_PcAuction.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddControllers();
builder.Services.AddDbContext<PcAuctionDbContext>();
builder.Services.AddTransient<IAuctionsRepository, AuctionsRepository>();
builder.Services.AddTransient<IBidsRepository, BidsRepository>();
builder.Services.AddTransient<IPartCategoriesRepository, PartCategoriesRepository>();
builder.Services.AddTransient<IPartsRepository, PartsRepository>();
builder.Services.AddTransient<ISeriesRepository, SeriesRepository>();
builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<AuthDbSeeder>();
builder.Services.AddScoped<PartCategorySeeder>();
builder.Services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.ResourceOwner, policy => policy.Requirements.Add(new ResourceOwnerRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, ResOwnerAuthHandler>();

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<PcAuctionDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters.ValidAudience = builder.Configuration["JWT:ValidAudience"];
        options.TokenValidationParameters.ValidIssuer = builder.Configuration["JWT:ValidIssuer"];
        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]));
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

var blobServiceClient = new BlobServiceClient(builder.Configuration["AzureBlob:ConnectionString"]);
var blobContainerClient = blobServiceClient.GetBlobContainerClient(builder.Configuration["AzureBlob:ContainerName"]);
builder.Services.AddScoped(provider => blobContainerClient);

var app = builder.Build();

app.UseCors("AllowReactApp");
app.UseRouting();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

using var scope = app.Services.CreateScope();
var dbSeeder = scope.ServiceProvider.GetRequiredService<AuthDbSeeder>();
await dbSeeder.SeedAsync();
var categorySeeder = scope.ServiceProvider.GetRequiredService<PartCategorySeeder>();
await categorySeeder.SeedAsync();

app.Run();
