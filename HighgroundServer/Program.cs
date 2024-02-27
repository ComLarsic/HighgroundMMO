using System.Reflection;
using HGPlayers;
using HGRooms;
using HGSocketManager;
using HGChat;
using HGWorld;
using HGScript;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/// <summary>
/// Configure the Kestrel server.
/// </summary>
/// <param name="builder">The web application builder</param>
static void ConfigureKestrel(WebApplicationBuilder builder)
{
    builder.WebHost.UseKestrel(options =>
    {
        options.ListenAnyIP(5189);
    });
}

/// <summary>
/// Add the message handlers to the service collection.
/// This is done automatically by scanning the assembly for types that implement IMessageHandler.
/// </summary>
/// <param name="services">The service collection</param>
/// <returns></returns>
static void AddMessageHandlers(IServiceCollection services)
{
    var messageHandlerTypes = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.GetInterfaces().Contains(typeof(IMessageHandler)));
    foreach (var messageHandlerType in messageHandlerTypes)
    {
        services.AddSingleton(typeof(IMessageHandler), messageHandlerType);
    }
}

// Configure the Kestrel server
ConfigureKestrel(builder);

// Add the services
builder.Services.AddControllers();
builder.Services.AddSingleton<IPlayerManager, PlayerManager>();
builder.Services.AddSingleton<ISessionManager, SessionManager>();
builder.Services.AddHostedService<SocketService>();
builder.Services.AddSingleton<IRoomManager, RoomManager>(o =>
{
    // Create some initial rooms
    var roomManager = new RoomManager();
    roomManager.CreateRoomAsync("Heck", 10);
    roomManager.CreateRoomAsync("Midgaurd", 10);
    roomManager.CreateRoomAsync("SeaOfNightmares", 10);
    return roomManager;
});
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddSingleton<IWorldManager, WorldManager>();
builder.Services.AddHostedService<GameWorldService>();

// Add the message handlers
AddMessageHandlers(builder.Services);

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

// Set up the websockets
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2),
});

// Set up the controllers
app.UseRouting();
app.MapControllers();

app.Run();