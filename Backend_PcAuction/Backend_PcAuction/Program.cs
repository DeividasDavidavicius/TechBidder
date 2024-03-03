using Backend_PcAuction.Data;
using Backend_PcAuction.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>();
builder.Services.AddTransient<IAuctionsRepository, AuctionsRepository>();


var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
