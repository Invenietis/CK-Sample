using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CK.AspNet.Auth;
using CK.Auth;
using CK.DB.AspNet.Auth;

namespace WebApp
{
    class Startup
    {
        readonly IConfiguration _configuration;
        readonly IHostingEnvironment _env;

        public Startup( IConfiguration configuration, IHostingEnvironment env )
        {
            _env = env;
            _configuration = configuration;
        }

        public void ConfigureServices( IServiceCollection services )
        {
            services.AddOptions();
            services.AddAuthentication( WebFrontAuthOptions.OnlyAuthenticationScheme )
                .AddWebFrontAuth( options =>
                 {
                     options.ExpireTimeSpan = TimeSpan.FromHours( 1 );
                     options.SlidingExpirationTime = TimeSpan.FromHours( 1 );
                 } );

            if( _env.IsDevelopment() )
            {
                string dllPath = _configuration["StObjMap:Path"];
                if( dllPath != null )
                {
                    var parentPath = Path.GetDirectoryName( Path.GetDirectoryName( Path.GetDirectoryName( Path.GetDirectoryName( _env.ContentRootPath ) ) ) );
                    dllPath = Path.Combine( parentPath, dllPath );
                    File.Copy( dllPath, Path.Combine( AppContext.BaseDirectory, "CK.StObj.AutoAssembly.dll" ), overwrite: true );
                }
            }
            services.AddDefaultStObjMap( "CK.StObj.AutoAssembly" );

            services.AddSingleton<IAuthenticationTypeSystem, StdAuthenticationTypeSystem>();
            services.AddSingleton<IWebFrontAuthLoginService, SqlWebFrontAuthLoginService>();
        }

        public void Configure( IApplicationBuilder app, IHostingEnvironment env )
        {
            app.UseRequestMonitor();

            app.UseAuthentication();
        }

    }
}
