using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using OpenApiSwagger.Contexts;
using OpenApiSwagger.Services;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: ApiConventionType(typeof(DefaultApiConventions))] //status code convention - globally(preferred)
namespace OpenApiSwagger
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
            //to add status codes globally to all controllers
                //setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
                //setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
                //setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));

                //setupAction.ReturnHttpNotAcceptable = true; //--> when an unsupported media type is passed
                //through postman, the above code when commneted returns response in default format(Xml)
                //else 406 Not acceptable

                setupAction.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                //var jsonOutputFormatter = setupAction.OutputFormatters
                //    .OfType<SystemTextJsonOutputFormatter>().FirstOrDefault();

                var jsonOutputFormatter = setupAction.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                    // remove text/json as it isn't the approved media type
                    // for working with JSON at API level
                    if (jsonOutputFormatter.SupportedMediaTypes.Contains("application/json"))
                    {
                        jsonOutputFormatter.SupportedMediaTypes.Remove("application/json");
                    }
                }
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["ConnectionStrings:LibraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));
            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var actionExecutingContext =
                        actionContext as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                    // if there are modelstate errors & all keys were correctly
                    // found/parsed we're dealing with validation errors
                    if (actionContext.ModelState.ErrorCount > 0
                        && actionExecutingContext?.ActionArguments.Count == actionContext.ActionDescriptor.Parameters.Count)
                    {
                        return new UnprocessableEntityObjectResult(actionContext.ModelState);
                    }

                    // if one of the keys wasn't correctly found / couldn't be parsed
                    // we're dealing with null/unparsable input
                    return new BadRequestObjectResult(actionContext.ModelState);
                };
            });

            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            
            services.AddAutoMapper();

            //reg/configuring the swagger generator 
            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc("LibraryOpenApiSpecification", //part of URI where OpenApi spec can be found
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "Library Api",//other properties can also be set up here like description, extenions etc
                        Version = "1",
                        Description = "Through this API you can access authors and their books.",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                        {
                            Email = "gayathriu64@gmail.com",
                            Name = "Gayathri U",
                            Url = new Uri("https://github.com/Its-Gayathri")

                        },
                        License = new Microsoft.OpenApi.Models.OpenApiLicense()
                        {
                            Name = "abc License",
                            Url = new Uri("https://github.com/Its-Gayathri")
                        },
                       // etc TermsOfService
                    });

                //when 2 get actions with same route (but diff reponse media types ) use this 
                setupAction.ResolveConflictingActions(apiDescriptions =>
                {
                    return apiDescriptions.First();
                });

                //setting up description section of doc
                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";//OpenApiSwagger.xml
                var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory,xmlCommentsFile);
                setupAction.IncludeXmlComments(xmlCommentsFullPath);
            });
            // services.AddMvc().AddNewtonsoftJson();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddNewtonsoftJson(options => {
                options.SerializerSettings.ContractResolver =
new DefaultContractResolver();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. 
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            //redirect to encrypted vesrion

            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint("/swagger/LibraryOpenApiSpecification/swagger.json", "Library API");
                setupAction.RoutePrefix = "";//to make doc available at the route
            });
            app.UseStaticFiles();
            app.UseMvc();


        }
    }
}
