using System.Text;
using fiwe.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Configurations.Extensions;
using Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                  optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Services.AddAppConfigurations(builder.Configuration);
var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()!;

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                // 👉 сюда можно поставить breakpoint
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                // 👉 сюда тоже breakpoint
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                // 👉 здесь увидишь ошибки валидации токена
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer()
                .AddSwaggerGen()
                .AddCommonDependency()
                .Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"))
                .AddSignalR();
builder.Services.AddTransient<IUserService, UserService>()
    .AddTransient<IMessageService, MessageService>()
    .AddTransient<IContactService, ContactService>();

var options = builder.Configuration["MongoDbSettings:ConnectionString"];

//builder.Services
//    .AddIdentityMongoDbProvider<ApplicationUser, MongoRole>(identityOptions =>
//    {
//        identityOptions.Password.RequiredLength = 6;
//        identityOptions.Password.RequireDigit = false;
//        identityOptions.Password.RequireUppercase = false;
//    },
//    mongoIdentityOptions =>
//    {
//        mongoIdentityOptions.ConnectionString = builder.Configuration["MongoDbSettings:ConnectionString"];
//        mongoIdentityOptions.UsersCollection = "Users";
//    });


//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.Cookie.HttpOnly = true;
//    options.ExpireTimeSpan = TimeSpan.Zero;
//    options.SlidingExpiration = false;
//    options.Events.OnRedirectToLogin = ctx =>
//    {
//        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
//        return Task.CompletedTask;
//    };
//});


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
var app = builder.Build();

app.UseHttpsRedirection();

//app.UseRouting();
//app.UseEndpoints(routeBuilder =>
//{
//    routeBuilder.MapControllerRoute(
//        name: "default",
//        pattern: "{controller=Home}/{action=Index}/{id?}");
//});

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}



app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chatHub");
app.MapControllers();

app.Run();
