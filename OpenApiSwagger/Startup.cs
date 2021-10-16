using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using OpenApiSwagger.Authentication;
using OpenApiSwagger.Contexts;
using OpenApiSwagger.OperationFilters;
using OpenApiSwagger.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

//[assembly: ApiConventionType(typeof(DefaultApiConventions))] //status code convention - globally(preferred)
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
                setupAction.Filters.Add(
                    new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
                setupAction.Filters.Add(
                    new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
                setupAction.Filters.Add(
                    new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
                setupAction.Filters.Add(
                    new ProducesDefaultResponseTypeAttribute());
                setupAction.Filters.Add(
                    new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized));
                setupAction.Filters.Add(
                   new AuthorizeFilter());

                setupAction.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                //var jsonOutputFormatter = setupAction.OutputFormatters
                //    .OfType<SystemTextJsonOutputFormatter>().FirstOrDefault();

                var jsonOutputFormatter = setupAction.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                    // remove text/json as it isn't the approved media type
                    // for working with JSON at API level
                    if (jsonOutputFormatter.SupportedMediaTypes.Contains("text/json"))
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

            //services.AddAutoMapper();//todo

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //to necessary services to the container - versioning
            services.AddVersionedApiExplorer(setupAction =>
            {
                setupAction.GroupNameFormat = "'v'VV";
            });


            //Authentication -basic is default scheme name
            services.AddAuthentication("Basic")
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);

            //registering versioning
            services.AddApiVersioning(setupAction =>
            {
                setupAction.AssumeDefaultVersionWhenUnspecified = true;
                setupAction.DefaultApiVersion = new ApiVersion(1, 0);
                setupAction.ReportApiVersions = true;
                //setupAction.ApiVersionReader = new HeaderApiVersionReader("api-version");
                //setupAction.ApiVersionReader = new MediaTypeApiVersionReader();
            });

            var apiVersionDescriptionProvider =
               services.BuildServiceProvider().GetService<IApiVersionDescriptionProvider>();

            //reg/configuring the swagger generator 
            services.AddSwaggerGen(setupAction =>
            {
                //to run through available api versions on apiVersionDescriptionProvider, instead of one
                //so that multiple swagger docs will be generated with a group name as doc name
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    setupAction.SwaggerDoc($"LibraryOpenAPISpecification{description.GroupName}", //part of URI where OpenApi spec can be found
                        new Microsoft.OpenApi.Models.OpenApiInfo()
                        {
                            Title = "Library Api",//other properties can also be set up here like description, extenions etc
                        Version = description.ApiVersion.ToString(),
                            Description = "Through this API you can access authors and books.",
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
                }

                setupAction.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    Description = "Input your username and password to access this API"
                });
                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basicAuth" }
                        }, new List<string>() }
                });
                //for selecting actions, it compares action's version with doc name (which will have version)
                setupAction.DocInclusionPredicate((documentName, apiDescription) =>
                {
                    var actionApiVersionModel = apiDescription.ActionDescriptor
                    .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }

                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v =>
                        $"LibraryOpenAPISpecificationv{v}" == documentName);
                    }
                    return actionApiVersionModel.ImplementedApiVersions.Any(v =>
                        $"LibraryOpenAPISpecificationv{v}" == documentName);
                });

                //manipulate each operation with diff media input/output
                setupAction.OperationFilter<CreateBookOperationFilter>();

                //when 2 get actions with same route (but diff reponse media types ) use this 
                setupAction.ResolveConflictingActions(apiDescriptions =>
                {
                    return apiDescriptions.First();
                });

                //setting up description section of doc
                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";//OpenApiSwagger.xml
                var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);
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
        //[Obsolete]
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env
            , IApiVersionDescriptionProvider apiVersionDescriptionProvider)
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
                //forach apiDescription create end point for each of them
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    setupAction.SwaggerEndpoint($"/swagger/" +
                        $"LibraryOpenAPISpecification{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
                //setupAction.SwaggerEndpoint("/swagger/LibraryOpenApiSpecification/swagger.json", "Library API");

                setupAction.RoutePrefix = "";//to make doc available at the route
            });            

            app.UseStaticFiles();

            //adding authentication middleware to req pipeline

            app.UseAuthentication();
            app.UseMvc();


        }
    }
}
