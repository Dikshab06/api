using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;


namespace MinhaApi
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
        public DbSet<Conta> contas { get; set; }
         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurações adicionais, se necessário
        }
    }
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {   services.AddControllers();
           //services.AddLogging();
            services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

            // Adicionar suporte ao Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minha API", Version = "v1" });

                // Configuração para incluir a autenticação JWT no Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                });

                 c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                         Array.Empty<string>()
                    }
                });
            });

            services.AddDbContext<MyDbContext>(options =>
                options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")));


            // Configure JWT Authentication
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors();
                 app.UseSwagger(options =>
            {
            options.SerializeAsV2 = true;
            });
        app.UseSwaggerUI(options => 
        {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Minha API v1");
        });
            
    }

           app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
//using Microsoft.AspNetCore.Builder;
 //using Microsoft.AspNetCore.Hosting;
 //using Microsoft.EntityFrameworkCore;
 //using Microsoft.Extensions.Configuration;
 //using Microsoft.Extensions.DependencyInjection;
 //using Microsoft.Extensions.Hosting;

//namespace MinhaApi
//{

//    public class MyDbContext : DbContext
//    {
//        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
//        public DbSet<User> Users { get; set; }
//    }

//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public IConfiguration Configuration { get; }


//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddDbContext<MyDbContext>(options =>
//                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 21))));
//            services.AddControllers();
//            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
//                {
//                    options.Authority = "https://localhost:5001";
//                    options.RequireHttpsMetadata = false;
//                    options.Audience = "api1";
//                });
//        }

//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {
//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//            }
//            else
//            {
//                app.UseExceptionHandler("/Home/Error");
//                app.UseHsts();
//            }

//            app.UseHttpsRedirection();
//            app.UseStaticFiles();

//            app.UseRouting();

//            app.UseAuthorization();

//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapControllers();
//            });
//        }
//    }
//}


////using Microsoft.EntityFrameworkCore;

////public class MyDbContext : DbContext
////{
////    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
////    public DbSet<User> Users { get; set; }
////}

////public class User
////{
////    public int Id { get; set; }
////    public string Username { get; set; }
////    public string Password { get; set; }
////    public string Role { get; set; }
////}

////public void ConfigureServices(IServiceCollection services)
////{
////    services.AddDbContext<MyDbContext>(options =>
////        options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 21))));
////    services.AddControllers();
////    services.AddAuthentication("Bearer")
////        .AddJwtBearer("Bearer", options =>
////        {
////            options.Authority = "https://localhost:5001";
////            options.RequireHttpsMetadata = false;
////            options.Audience = "api1";
////        });
////}
