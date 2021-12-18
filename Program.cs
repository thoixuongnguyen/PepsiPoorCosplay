using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PepsiCompetitive.App.Databases;
using PepsiCompetitive.App.Mappings;
using PepsiCompetitive.App.Ultilities;
using PepsiCompetitive.Modules.Beats.Requests;
using PepsiCompetitive.Modules.Beats.Services;
using PepsiCompetitive.Modules.Beats.Validations;
using PepsiCompetitive.Modules.Players.Requests;
using PepsiCompetitive.Modules.Players.Services;
using PepsiCompetitive.Modules.Players.Validations;
using PepsiCompetitive.Modules.Videos.Requests;
using PepsiCompetitive.Modules.Videos.Services;
using PepsiCompetitive.Modules.Videos.Validations;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
var tokenValidationParams = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
    RequireExpirationTime = false
};
// Add services to the container.


#region Add ElasticSearch
builder.Services.AddElasticsearch(builder.Configuration);
builder.Services.AddScoped<IPlayerServices,PlayerServices>() ;
#endregion

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IValidator<PlayerLoginRequest>, PlayerLoginValidation>();
builder.Services.AddTransient<IValidator<PlayerUpdateRequest>, PlayerUpdateValidation>();
builder.Services.AddTransient<IValidator<BeatStoreRequest>, BeatStoreValidation>();
builder.Services.AddTransient<IValidator<BeatUpdateRequest>, BeatUpdateValidation>();
builder.Services.AddTransient<IValidator<VideoUpdateRequest>, VideoUpdateValidation>();
builder.Services.AddTransient<IValidator<VideoUploadRequest>, VideoUploadValidation>();

builder.Services.AddControllers().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<PlayerLoginRequest>());
builder.Services.AddControllers().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<BeatStoreRequest>());

builder.Services.AddScoped<IAmazonS3Utility, AmazonS3Utility>();
builder.Services.AddScoped<IBeatServices, BeatServices>();
builder.Services.AddScoped<IVideoServices, VideoServices>();

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwt =>
{
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = tokenValidationParams;
});

#region Authorize Button
builder.Services.AddSwaggerGen(
    swagger =>
    {
        swagger.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "JWT Token Authentication API",
            Description = ""
        });
        swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        });
        swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}

            }
        });
    }
    );
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
