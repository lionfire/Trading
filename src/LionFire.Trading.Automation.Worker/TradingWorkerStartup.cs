using LionFire.AspNetCore;
using LionFire.Trading.Automation.Worker.Components;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;

public class TradingWorkerStartup //: WebHostConfig
{
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
            e.MapRazorComponents<App>()
                //.AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(
                //    //typeof(FireLynx.Blazor.Public.Pages.TradingAlertsDashboard).Assembly,
                    typeof(LionFire.Trading.Automation.Blazor.Optimization.OneShotOptimize).Assembly,
                    typeof(LionFire.Trading.Blazor.Exchanges.Exchanges).Assembly
                )
                .AddInteractiveServerRenderMode()
                ;
            e.MapBlazorHub().WithOrder(-1);
        });
    }
}

