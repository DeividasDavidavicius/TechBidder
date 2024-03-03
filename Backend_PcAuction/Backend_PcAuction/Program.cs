using Backend_PcAuction.Data;
using Backend_PcAuction.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<PcAuctionDbContext>();
builder.Services.AddTransient<IAuctionsRepository, AuctionsRepository>(); builder.Services.AddTransient<IAuctionsRepository, AuctionsRepository>();
builder.Services.AddTransient<IBidsRepository, BidsRepository>();

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
