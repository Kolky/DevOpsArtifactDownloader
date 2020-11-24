using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace DevOpsArtifactDownloader
{
    internal class Program
    {
        private enum ExitCode : int
        {
            Success = 0,
            NoBuildsAvailable = 1,
            SavingResultFailed = 2,
        }

        private class Options
        {
            [Option('o', "organization", Required = true, HelpText = "Your Azure DevOps organization.")]
            public string Organization { get; set; }

            [Option('t', "pat", Required = true, HelpText = "Your Personal Access Token.")]
            public string PersonalAccessToken { get; set; }

            [Option('p', "project", Required = true, HelpText = "The specified project.")]
            public string Project { get; set; }

            [Option('d', "definition", Required = true, HelpText = "The pipeline definition-id, get it from the browser url.")]
            public int Defintion { get; set; }

            [Option('b', "branch", HelpText = "The specified branch, use git refs format. For example: /refs/heads/develop")]
            public string Branch { get; set; }

            [Option('a', "artifact", Required = true, HelpText = "The specified artifact name. Use {buildId}, {buildNumber} or {revision} to replace with associated details of the build.")]
            public string Artifact { get; set; }

            [Option('r', "result", Default = "Build.zip", HelpText = "The output file name, must end in '.zip'. Use {buildId}, {buildNumber} or {revision} to replace with associated details of the build.")]
            public string Result { get; set; }
        }

        private static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(Execute);
        }

        private static async Task Execute(Options options)
        {
            // Create instance of VssConnection using Personal Access Token
            var connection = new VssConnection(new Uri($"https://dev.azure.com/{options.Organization}"), new VssBasicCredential(string.Empty, options.PersonalAccessToken));
            var client = await connection.GetClientAsync<BuildHttpClient>();

            // Find latest build for project, definition and branch.
            var builds = await client.GetBuildsAsync(
                options.Project,
                definitions: new[] { options.Defintion },
                branchName: options.Branch,
                statusFilter: BuildStatus.Completed,
                resultFilter: BuildResult.Succeeded,
                queryOrder: BuildQueryOrder.FinishTimeDescending,
                top: 1);
            if (!builds.Any())
            {
                Environment.Exit((int)ExitCode.NoBuildsAvailable);
                return;
            }

            var build = builds.Single();

            try
            {
                using (var zip = await client.GetArtifactContentZipAsync(options.Project, build.Id, BuildArtifactName(options.Artifact, build)))
                using (var fileStream = File.OpenWrite(BuildResultFile(options.Result, build)))
                {
                    await zip.CopyToAsync(fileStream);
                }
            }
            catch (IOException)
            {
                Environment.Exit((int)ExitCode.SavingResultFailed);
            }
        }

        private static string BuildArtifactName(string artifactName, Build build)
        {
            return artifactName
                .Replace("{buildId}", build.Id.ToString())
                .Replace("{buildNumer}", build.BuildNumber)
                .Replace("{revision}", build.BuildNumberRevision?.ToString() ?? string.Empty);
        }

        private static string BuildResultFile(string result, Build build)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return "Build.zip";
            }

            if (!result.EndsWith(".zip"))
            {
                return $"{BuildArtifactName(result, build)}.zip";
            }

            return BuildArtifactName(result, build);
        }
    }
}