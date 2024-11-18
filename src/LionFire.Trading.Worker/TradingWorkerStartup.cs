using LionFire.AspNetCore;
using LionFire.Trading.Worker.Components;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;

public class TradingWorkerStartup //: WebHostConfig
{
    //public TradingWorkerStartup(IConfiguration configuration) : base(configuration)
    //{
    //}

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorComponents()
          .AddInteractiveServerComponents()
          ;

        //services.AddRazorPages();
        services.AddServerSideBlazor();

        services.AddMudServices();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }
        // From .NET 8 template
        //if (!app.Environment.IsDevelopment())
        //{
        //    app.UseExceptionHandler("/Error", createScopeForErrors: true);
        //}

        //if (env.IsDevelopment())
        //{
        //    //app.UseWebAssemblyDebugging();
        //}
        //else
        //{
        //    app.UseExceptionHandler("/Error", createScopeForErrors: true);
        //}


        if (!env.IsDevelopment())
        {
            StaticWebAssetsLoader.UseStaticWebAssets(env, app.ApplicationServices.GetRequiredService<IConfiguration>());
        }
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAntiforgery(); // Must go between Routing and Endpoints

        //app.UseAuthentication();
        //app.UseAuthorization();

        app.UseEndpoints(e =>
        {
            //    endpoints.MapBlazorHub();
            //e.MapFallbackToPage("/_Host");
            e.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                //.AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(
                    typeof(FireLynx.Blazor.Public.Pages.TradingAlertsDashboard).Assembly,
                    typeof(LionFire.Trading.Automation.Blazor.Optimization.OneShotOptimize).Assembly
                );
        });
    }
}

