using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using CK.Auth;
using CK.AspNet.Auth;
using CK.DB.AspNet.Auth;
using CK.DB.User.UserGithub;
using CK.Text;

namespace WebApp
{
    public class Startup
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
                 } )
                 .AddOAuth( "GitHub", options =>
                  {
                      options.ClientId = _configuration["Authentication:GitHub:ClientId"];
                      options.ClientSecret = _configuration["Authentication:GitHub:ClientSecret"];
                      options.CallbackPath = new PathString( "/signin-github" );

                      options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                      options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                      options.UserInformationEndpoint = "https://api.github.com/user";

                      options.ClaimActions.MapJsonKey( ClaimTypes.NameIdentifier, "id" );
                      options.ClaimActions.MapJsonKey( ClaimTypes.Name, "name" );
                      options.ClaimActions.MapJsonKey( "urn:github:login", "login" );
                      options.ClaimActions.MapJsonKey( "urn:github:url", "html_url" );
                      options.ClaimActions.MapJsonKey( "urn:github:avatar", "avatar_url" );

                      options.Events = new OAuthEventHandler
                      {
                          OnCreatingTicket = async context =>
                          {
                              var request = new HttpRequestMessage( HttpMethod.Get, context.Options.UserInformationEndpoint );
                              request.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
                              request.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", context.AccessToken );

                              var response = await context.Backchannel.SendAsync( request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted );
                              response.EnsureSuccessStatusCode();

                              var user = JObject.Parse( await response.Content.ReadAsStringAsync() );

                              context.RunClaimActions( user );
                          }
                      };
                  } );

            if( _env.IsDevelopment() )
            {
                NormalizedPath dllPath = _configuration["StObjMap:Path"];
                if( !dllPath.IsEmpty )
                {
                    var solutionPath = new NormalizedPath( AppContext.BaseDirectory ).RemoveLastPart( 4 );
                    dllPath = solutionPath.Combine( dllPath );
                    File.Copy( dllPath, Path.Combine( AppContext.BaseDirectory, "CK.StObj.AutoAssembly.dll" ), overwrite: true );
                }
            }
            services.AddDefaultStObjMap( "CK.StObj.AutoAssembly" );
            services.AddCors();
            services.AddSingleton<IAuthenticationTypeSystem, StdAuthenticationTypeSystem>();
            services.AddSingleton<IWebFrontAuthLoginService, SqlWebFrontAuthLoginService>();
        }

        public void Configure( IApplicationBuilder app, IHostingEnvironment env )
        {
            app.UseRequestMonitor();
            app.UseCors( c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials() );
            app.UseAuthentication();
        }
    }

    internal class OAuthEventHandler : OAuthEvents
    {
        public override Task TicketReceived( TicketReceivedContext c )
        {
            var authService = c.HttpContext.RequestServices.GetRequiredService<WebFrontAuthService>();
            return authService.HandleRemoteAuthentication<IUserGithubInfo>( c, payload =>
            {
                payload.GithubAccountId = c.Principal.FindFirst( ClaimTypes.NameIdentifier ).Value;
            } );
        }
    }
}
