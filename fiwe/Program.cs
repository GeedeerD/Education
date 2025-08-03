using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Configurations;
using fiwe.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer()
                .AddSwaggerGen()
                .AddCommonDependency()
                .Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

var options = builder.Configuration["MongoDbSettings:ConnectionString"];

builder.Services
    .AddIdentityMongoDbProvider<ApplicationUser, MongoRole>(identityOptions =>
    {
        identityOptions.Password.RequiredLength = 6;
        identityOptions.Password.RequireDigit = false;
        identityOptions.Password.RequireUppercase = false;
    },
    mongoIdentityOptions =>
    {
        mongoIdentityOptions.ConnectionString = builder.Configuration["MongoDbSettings:ConnectionString"];

        mongoIdentityOptions.UsersCollection = "Users";
    });



BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
var app = builder.Build();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
