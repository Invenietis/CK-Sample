using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Common.Tools.MSBuild;
using Cake.Core;
using Cake.Core.Diagnostics;
using SimpleGitVersion;
using System;
using System.Linq;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Publish;

namespace CodeCake
{
    [AddPath( "%UserProfile%/.nuget/packages/**/tools*" )]
    public partial class Build : CodeCakeHost
    {

        public Build()
        {
            Cake.Log.Verbosity = Verbosity.Diagnostic;

            const string solutionName = "CK-Sample";
            const string solutionFileName = solutionName + ".sln";

            var releasesDir = Cake.Directory( "CodeCakeBuilder/Releases" );
            var projects = Cake.ParseSolution( solutionFileName )
                                       .Projects
                                       .Where( p => !(p is SolutionFolder)
                                                    && p.Name != "CodeCakeBuilder" );

            // We do not publish .Tests projects for this solution.
            var projectsToPublish = projects
                                        .Where( p => !p.Path.Segments.Contains( "Tests" ) );

            // The SimpleRepositoryInfo should be computed once and only once.
            SimpleRepositoryInfo gitInfo = Cake.GetSimpleRepositoryInfo();
            // This default global info will be replaced by Check-Repository task.
            // It is allocated here to ease debugging and/or manual work on complex build script.
            CheckRepositoryInfo globalInfo = new CheckRepositoryInfo { Version = gitInfo.SafeNuGetVersion };

            Task( "Check-Repository" )
                .Does( () =>
                {
                    globalInfo = StandardCheckRepository( projectsToPublish, gitInfo );
                } );

            Task( "Clean" )
                .Does( () =>
                 {
                     Cake.CleanDirectories( projects.Select( p => p.Path.GetDirectory().Combine( "bin" ) ) );
                     Cake.CleanDirectories( releasesDir );
                     Cake.DeleteFiles( "Tests/**/TestResult*.xml" );
                 } );

            Task( "Build" )
                .IsDependentOn( "Check-Repository" )
                .IsDependentOn( "Clean" )
                .Does( () =>
                 {
                     StandardSolutionBuild( solutionFileName, gitInfo, globalInfo.BuildConfiguration );
                 } );

            Task( "Unit-Testing" )
                .IsDependentOn( "Build" )
                .WithCriteria( () => Cake.InteractiveMode() == InteractiveMode.NoInteraction
                                     || Cake.ReadInteractiveOption( "RunUnitTests", "Run Unit Tests?", 'Y', 'N' ) == 'Y' )
               .Does( () =>
                 {
                     StandardUnitTests( globalInfo.BuildConfiguration, projects.Where( p => p.Name.EndsWith( ".Tests" ) ) );
                 } );

            Task( "Build-And-Push-WebApp" )
                .IsDependentOn( "Unit-Testing" )
                .WithCriteria( () => gitInfo.IsValidRelease )
                .Does( () =>
                {
                    // TODO
                } );

            // The Default task for this script can be set here.
            Task( "Default" )
                .IsDependentOn( "Build-And-Push-WebApp" );
        }

    }
}
