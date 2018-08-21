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

                      // We are using the standard transformer from the JSON info received
                      // when calling the UserInformationEndpoint (in OnCreatingTicket below)
                      // to create the Principal claims.
                      options.ClaimActions.MapJsonKey( ClaimTypes.NameIdentifier, "id" );

                      // This is required for Github: we must call the back channel with the
                      // AccessToken to retrieve actual user informations.
                      options.Events.OnCreatingTicket = async context =>
                      {
                        using( var request = new HttpRequestMessage( HttpMethod.Get, context.Options.UserInformationEndpoint ) )
                        {
                            request.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
                            request.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", context.AccessToken );
                            using( var response = await context.Backchannel.SendAsync( request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted ) )
                            {
                                response.EnsureSuccessStatusCode();
                                string userInfo = await response.Content.ReadAsStringAsync();
                                context.RunClaimActions( JObject.Parse( userInfo ) );
                            }
                        }
                      };

                      // Final transfer to the WebFrontAuthService.
                      options.Events.OnTicketReceived = c => c.WebFrontAuthRemoteAuthenticateAsync<IUserGithubInfo>( payload =>
                      {
                          payload.GithubAccountId = c.Principal.FindFirst( ClaimTypes.NameIdentifier ).Value;
                      } );

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
            services.AddStObjMap( "CK.StObj.AutoAssembly" );
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
}
