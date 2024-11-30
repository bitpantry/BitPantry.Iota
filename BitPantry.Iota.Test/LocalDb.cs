using System;
using System.Diagnostics;
using System.Net;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BitPantry.Iota.Test
{
    public class LocalDb
    {
        private readonly ILogger<LocalDb> _logger;

        public string ConnectionString { get; }
        public string InstanceName => ConnectionStringBuilder.DataSource.Substring("(localdb)\\".Length);
        public string DatabaseName => ConnectionStringBuilder.InitialCatalog;

        private SqlConnectionStringBuilder ConnectionStringBuilder { get; }
        private SqlConnectionStringBuilder MasterDbConnectionStringBuilder { get; }

        public LocalDb(ILogger<LocalDb> logger, AppSettings appSettings)
        {
            _logger = logger;

            ConnectionString = appSettings.ConnectionStrings.EntityDataContext ?? throw new ArgumentNullException(nameof(appSettings.ConnectionStrings.EntityDataContext));
            ConnectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
            MasterDbConnectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };

            if (string.IsNullOrEmpty(ConnectionStringBuilder.DataSource) || string.IsNullOrEmpty(ConnectionStringBuilder.InitialCatalog))
            {
                throw new ArgumentException("The connection string must specify both the instance name and database name.");
            }

            // Extract instance and database names
            var dataSource = ConnectionStringBuilder.DataSource;
            if (!dataSource.StartsWith("(localdb)\\", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The connection string must use a LocalDB data source.");
            }
        }

        public async Task Deploy(bool recreateDatabase = false)
        {
            _logger.LogDebug("Ensuring setup of local db :: {ConnectionString}", ConnectionString);

            if (!IsInstalled())
                await Install();

            if (!InstanceExists())
                CreateInstance();

            if (recreateDatabase && DatabaseExists())
                DropDatabase();

            if (!DatabaseExists())
                CreateDatabase();
        }

        public void CreateInstance(string version = "15.0")
        {
            ExecuteCommand($"sqllocaldb create \"{InstanceName}\" {version} -s");
            _logger.LogDebug("LocalDB instance '{InstanceName}' created and started", InstanceName);
        }

        public void DeleteInstance()
        {
            ExecuteCommand($"sqllocaldb delete \"{InstanceName}\"");
            _logger.LogDebug("LocalDB instance '{InstanceName}' deleted", InstanceName);
        }

        public bool InstanceExists()
        {
            var output = ExecuteCommand("sqllocaldb info");
            return output.Contains(InstanceName, StringComparison.OrdinalIgnoreCase);
        }

        public void CreateDatabase()
        {
            using (var connection = new SqlConnection(MasterDbConnectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"CREATE DATABASE [{DatabaseName}]";
                    command.ExecuteNonQuery();
                }
            }

            _logger.LogDebug("Database '{DatabaseName}' created in instance '{InstanceName}'", DatabaseName, InstanceName);
        }

        public void DropDatabase()
        {
            using (var connection = new SqlConnection(MasterDbConnectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"
                        ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        DROP DATABASE [{DatabaseName}];";
                    command.ExecuteNonQuery();
                }
            }

            _logger.LogDebug("Database '{DatabaseName}' dropped from instance '{InstanceName}'", DatabaseName, InstanceName);
        }

        public bool DatabaseExists()
        {
            using (var connection = new SqlConnection(MasterDbConnectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT database_id FROM sys.databases WHERE name = @DatabaseName";
                    command.Parameters.AddWithValue("@DatabaseName", DatabaseName);

                    var result = command.ExecuteScalar();
                    return result != null;
                }
            }
        }
    

        public bool IsInstalled()
        {
            try
            {
                ExecuteCommand("sqllocaldb info");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task Install()
        {
            _logger.LogInformation("Installing LocalDB");

            var downloadUrl = "https://go.microsoft.com/fwlink/?linkid=2189792"; // Replace with the updated link if needed
            var installerFileName = "SqlLocalDB.msi";

            string tempPath = Path.GetTempPath();
            string installerPath = Path.Combine(tempPath, installerFileName);

            if (!File.Exists(installerPath))
            {
                _logger.LogDebug("Downloading SQL Server LocalDB installer");
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();

                    await using (var fs = new FileStream(installerPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }
                _logger.LogDebug("Download complete");
            }

            _logger.LogDebug("Installing SQL Server LocalDB");
            Process process = new Process();
            process.StartInfo.FileName = "msiexec";
            process.StartInfo.Arguments = $"/i \"{installerPath}\" /quiet";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                _logger.LogDebug("SQL Server LocalDB installation complete");
            }
            else
            {
                _logger.LogError("Installation failed with exit code: {ExitCode}", process.ExitCode);
            }
        }

        private static string ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Error executing command: {command}");
                    Console.WriteLine(error);
                    throw new Exception($"Command failed: {command}\n{error}");
                }

                return output;
            }
        }
    }
}
