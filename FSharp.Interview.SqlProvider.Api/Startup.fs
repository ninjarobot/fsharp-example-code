namespace FSharp.Interview.SqlProvider.Api

open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Swashbuckle.AspNetCore.Swagger
open FSharp.Interview.SqlProvider.Api.Domain.SqlProviderConnection

type Startup private () =
    new (configuration: IConfiguration) as me = //prefer 'me' to 'this' to distance from C# 'this'
        Startup() then
        me.Configuration <- configuration
    
    // This method gets called by the runtime. Use this method to add services to the container.
    member __.ConfigureServices(services: IServiceCollection) =

        //set conn string - if anyone has a better way of doing this, please let me know!
        let cs = __.Configuration.GetConnectionString("JobInterviewConnectionString")
        JobInterviewConnection().SetConnString cs

        //swagger
        let filePath = Path.Combine(System.AppContext.BaseDirectory, "FSharp.Interview.SqlProvider.Api.xml")
        services.AddSwaggerGen(fun c -> c.SwaggerDoc("v1",  Info(Title="Job Interview API", Version="v1"))) |> ignore
        services.AddSwaggerGen(fun c -> c.IncludeXmlComments(filePath)) |> ignore
        services.AddSwaggerGen(fun c -> c.CustomSchemaIds(fun x -> x.FullName)) |> ignore

        // Add framework services.
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2) |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member __.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =

        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        else
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts() |> ignore

        app.UseSwagger() |> ignore
        app.UseSwaggerUI(fun c -> c.SwaggerEndpoint("/swagger/v1/swagger.json", "Job Interview API V1")) |> ignore
        app.UseHttpsRedirection() |> ignore
        app.UseMvc() |> ignore

    member val Configuration : IConfiguration = null with get, set
