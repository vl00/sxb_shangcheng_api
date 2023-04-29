using Autofac;
using Autofac.Extensions.DependencyInjection;
using iSchool.Infrastructure;
using iSchool.Organization.Activity.Api.Configurations;
using iSchool.Organization.Api.Auths;
using iSchool.Organization.Api.Configurations;
using iSchool.Organization.Api.Conventions;
using iSchool.Organization.Api.Filters;
using iSchool.Organization.Api.Middlewares;
using iSchool.Organization.Activity.Api.Modules.AutofacModule;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NLog.Web;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.CommonHelper;

namespace iSchool.Organization.Activity.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            //避免中文乱码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);  //避免日志中的中文输出乱码

            //加入健康检查
            services.AddHealthChecks()
                .AddSqlServer(Configuration.GetConnectionString("SqlServerConnection"), name: "sqlserver")
                .AddRedis(Configuration["HealthCheck:RedisConnection"], name: "redis");

            services.AddControllers(o =>
            {
                //添加全局错误过滤器
                o.Filters.Add(typeof(GlobalExceptionsFilter));
                o.Conventions.Add(new CommaConvQueryStringConvention());

            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            });

            //automapper
            services.AddAutoMapperSetup();

            //MediatR 
            services.AddMediatR(typeof(Startup));

            // httpcontext
            services.AddHttpContextAccessor(); //same as 'services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();'
            // 用户信息
            services.AddScoped(typeof(IUserInfo), typeof(UserInfo));

            // csredis
            services.AddSingleton(sp => new CSRedis.CSRedisClient(Configuration["redis:0"]));

            // httpclient
            services.AddHttpClient(string.Empty, (http) =>
            {
                http.Timeout = new TimeSpan(0, 5, 0);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler() { UseProxy = false });

            services.AddScoped<IActivityHelper, Activity1Helper>();

            #region config options
            services.Configure<Organization.Appliaction.BussTknOption>(option => Configuration.Bind("AppSettings:BussTkn", option));
            services.Configure<Organization.Appliaction.ElvtMainPageSizeOption>(option => Configuration.Bind("AppSettings:elvtMainPageSize", option));
            services.Configure<Organization.Appliaction.EvltCoverCreateOption>(option => 
            {
                Configuration.Bind("AppSettings:EvltCoverCreate", option, (config, o) => 
                {
                    o.FontColor = Color.FromName(config["fontColor"]);
                });
            });
            #endregion

            //appsetting文件
            var appSettingConfig = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingConfig);
            services.AddScoped(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);


#if DEBUG
            //Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "iSchool.Organization.Activity.Api",
                    Version = "v1",
                    Description = "i hushushu"
                });
                //添加中文注释                
                var basePath = Path.GetDirectoryName(typeof(Startup).Assembly.ManifestModule.FullyQualifiedName);
                var files = Directory.EnumerateFiles(basePath, "iSchool.*.xml");
                foreach (var file in files)
                {
                    c.IncludeXmlComments(file);
                }

                c.DocInclusionPredicate((docName, description) => true);
                c.ParameterFilter<iSchool.Api.Swagger.ApiDocParameterOperationFilter>();                
            });
            services.AddSwaggerGenNewtonsoftSupport();
#endif

            // authentication
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })            
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => 
            {
                // 虎叔叔的用户中心
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = async context =>
                    {
                        if (context.Request.IsAjaxRequest())
                        {
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json; charset=utf-8";
                            await context.Response.WriteAsync((new { succeed = false, status = Organization.Domain.Enum.ResponseCode.NoLogin.ToInt(), msg = "未登录" }).ToJsonString(true), Encoding.UTF8);
                            //await context.Response.WriteAsync(ResponseResult.Success(new { status = 401 }).ToJsonString(true), Encoding.UTF8);
                            return;
                        }
                        var currentUrl = new UriBuilder(context.RedirectUri);
                        var returnUrl = new UriBuilder
                        {
                            Scheme = currentUrl.Scheme,
                            Host = currentUrl.Host,
                            Port = currentUrl.Port,
                            Path = context.Request.Path,
                            Query = string.Join("&", context.Request.Query.Select(q => q.Key + "=" + q.Value))
                        };
                        var redirectUrl = new UriBuilder(Configuration["auth:cookie:loginPath"])
                        {
                            Query = QueryString.Create(context.Options.ReturnUrlParameter, returnUrl.Uri.ToString()).Value
                        };
                        context.Response.Redirect(redirectUrl.Uri.ToString());
                    }
                };
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = Configuration["auth:cookie:name"];
                options.Cookie.Domain = Configuration["auth:cookie:domain"];
                options.Cookie.Path = Configuration["auth:cookie:path"];
#if !DEBUG
                options.DataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(Configuration["auth:cookie:dataProtectionDir"]));
#else
                options.DataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.ManifestModule.FullyQualifiedName), Configuration["auth:cookie:dataProtectionDir"])));
#endif
            })
            .AddScheme<JobsAuthenticationSchemeOptions, JobsAuthenticationHandler>("jobs", options =>
            {
                Configuration.GetSection("auth:jobs").Bind(options);
            });

            // authorization
            services.AddAuthorization(options => 
            {
                options.AddPolicy("jobs", builder => builder.AddAuthenticationSchemes("jobs").AddRequirements(new JobsAuthorizationRequirement()));
            })
            .AddSingleton<IAuthorizationHandler, JobsAuthorizationHandler>();

            services.AddCors(options => 
            {
                options.AddDefaultPolicy(builder => 
                {
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                    builder.SetIsOriginAllowed(_ => true);
                    builder.AllowCredentials();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IHostApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStarted.Register(o =>
            {
                var t = ((Tuple<Startup, IServiceProvider>)o);
                t.Item1.OnApplicationStarted(t.Item2);
            }, Tuple.Create(this, app.ApplicationServices), false);
            applicationLifetime.ApplicationStopping.Register(o =>
            {
                var t = ((Tuple<Startup, IServiceProvider>)o);
                t.Item1.OnApplicationStopping(t.Item2);
            }, Tuple.Create(this, app.ApplicationServices), false);

            //使用NLog作为日志记录工具
            if (env.IsDevelopment())
            {
                NLogBuilder.ConfigureNLog("nlog.Development.config");
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsProduction())
            {
                NLogBuilder.ConfigureNLog("nlog.config");
            }
            //在nlog 配置文件中配置链接日志连接字符串
            NLog.LogManager.Configuration.Variables["connectionString"] = Configuration["ConnectionStrings:LogSqlServerConnection"];

            app.UseHttpsRedirection();           
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseCors();

            app.UseAuthentication();
            app.Use(async (ctx, next) => 
            {
                if (ctx.User.Identity.IsAuthenticated)
                {
                    var user = ctx.RequestServices.GetService<IUserInfo>() as UserInfo;
                    if (user != null)
                    {
                        user.SetCtxUser(ctx.User);
                        user.HeadImg ??= ctx.RequestServices.GetService<IConfiguration>()["AppSettings:UserDefaultHeadImg"];
                    }
                }
                await next();
            });
            app.UseRouting();

            app.UseAuthorization();

            //添加日志中间件
            app.UseLoggerMiddleware();

#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // /swagger
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "iSchool.Organization.Api v1");
            });
#endif

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = ResponseExtension.WriteResponse
                });
                //endpoints.MapRazorPages();
            });
        }

        /// <summary>
        /// 注册autofac module
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new MediatorModule());
            builder.RegisterModule(new InfrastructureModule(Configuration.GetConnectionString("SqlServerConnection")));
            #region 测试环境二维码使用
            builder.RegisterModule(new WXInfrastructureModule(Configuration.GetConnectionString("WXSqlServerConnection")));
            builder.RegisterModule(new Openid_WXInfrastructureModule(Configuration.GetConnectionString("Openid_WXSqlServerConnection")));
            #endregion
            builder.RegisterModule(new UserInfrastructureModule(Configuration.GetConnectionString("UserSqlServerConnection")));
            builder.RegisterModule(new DomainModule());
        }

        /// <summary>
        /// on app start
        /// </summary>
        private void OnApplicationStarted(IServiceProvider services)
        {
            this.AutofacContainer = services.GetAutofacRoot();
            var serviceScopeFactory = services.GetService<IServiceScopeFactory>();
            AsyncUtils.SetServiceScopeFactory(serviceScopeFactory);
            SimpleQueue_Extension.ServiceScopeFactory = serviceScopeFactory;
            //RedisHelper.Initialization(services.GetService<CSRedis.CSRedisClient>());
        }

        /// <summary>
        /// on app stop
        /// </summary>
        private void OnApplicationStopping(IServiceProvider services)
        {
            //...
        }
    }
}
