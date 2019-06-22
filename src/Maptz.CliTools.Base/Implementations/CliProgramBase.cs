using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
namespace Maptz.CliTools
{

    /// <summary>
    /// A very simple base class for Cli programs which use dependency injections.
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    public abstract class CliProgramBase<TSettings> where TSettings : class
    {
        public int ExitCode { get; set; }

        /* #region Protected Methods */
        protected virtual void AddServices(IServiceCollection services)
        {
            //if (addLogging)
            //{
            //services.AddLogging(loggingBuilder => loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"))
            //                               .AddConsole()
            //                               .AddDebug());
            //}
            services.Configure<TSettings>(p => this.Configuration.GetSection("App").Bind(p));
            services.AddTransient<IConsoleInstance, ConsoleInstance>();
        }
        protected void Run()
        {
            var consoleInstance = this.ServiceProvider.GetService<IConsoleInstance>();
            var program = this.ServiceProvider.GetService<ICliProgramRunner>();
            try
            {
                program.RunAsync(this.Args).GetAwaiter().GetResult();
                
            }
            catch (CommandParsingException ex)
            {
                consoleInstance.WriteErrorLine(ex.Message.ToString());
                this.ExitCode = -2;
            }
            catch (Exception ex)
            {
                consoleInstance.WriteErrorLine(ex.Message.ToString());
                this.ExitCode = -1; 
            }

        }
        /* #endregion Protected Methods */
        /* #region Public Properties */
        public string[] Args { get; }
        public IConfiguration Configuration { get; set; }
        public ServiceProvider ServiceProvider { get; }
        /* #endregion Public Properties */
        /* #region Public Constructors */
        public CliProgramBase(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"); 
            this.Args = args;  
            var maptzSettingsPathUnexpanded = $"%AppData%/Maptz/{this.GetType().Assembly.GetName().Name}.json";
            var maptzSettingsPath = Environment.ExpandEnvironmentVariables(maptzSettingsPathUnexpanded);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (maptzSettingsPath != maptzSettingsPathUnexpanded)
            {
                builder.AddJsonFile(maptzSettingsPath, optional: true);
            }


            this.OnConfiguringBuilder(builder);

            this.Configuration = builder.Build(); //Configuration["Option1"] or Configuration["subsection:suboption1"]}
            var services = new ServiceCollection();
            this.AddServices(services);


            var serviceProvider = services.BuildServiceProvider();
            this.ServiceProvider = serviceProvider;

            this.Run();
        }

        protected virtual void OnConfiguringBuilder(IConfigurationBuilder builder)
        {
            
        }
        /* #endregion Public Constructors */
    }
}