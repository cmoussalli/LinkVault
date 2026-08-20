using CMouss.IdentityFramework;
using LinkVault.Portal.Components;
using LinkVault.Portal.Data;
using LinkVault.Portal.Services;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Portal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();


            // Add controllers for redirect handling
            builder.Services.AddControllers();

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<Components.App>()
                .AddInteractiveServerRenderMode();

            // Map controllers for redirect handling
            app.MapControllers();


            ConfigureIdentity(builder.Configuration);

            LinkVaultDbContext db = new();
            db.Database.EnsureCreated();
            db.InsertMasterData();
            IDFManager.RefreshIDFStorage();



            app.Run();
        }

        // Identity settings come from configuration (appsettings.json, user secrets,
        // or environment variables) so that no credentials live in source control.
        private static void ConfigureIdentity(IConfiguration configuration)
        {
            var identity = configuration.GetSection("Identity");

            var connectionString = Require(identity, "ConnectionString");
            var tokenEncryptionKey = Require(identity, "TokenEncryptionKey");
            var administratorPassword = Require(identity, "AdministratorPassword");

            IDFManager.Configure(new IDFManagerConfig
            {
                DatabaseType = Enum.Parse<DatabaseType>(
                    identity["DatabaseType"] ?? nameof(DatabaseType.MSSQL), ignoreCase: true),
                DBConnectionString = connectionString,
                DefaultListPageSize = identity.GetValue("DefaultListPageSize", 25),
                DBLifeCycle = DBLifeCycle.Both,
                IsActiveByDefault = true,
                IsLockedByDefault = false,
                DefaultTokenLifeTime = new LifeTime(identity.GetValue("TokenLifeTimeDays", 365), 0, 0),
                AllowUserMultipleSessions = identity.GetValue("AllowUserMultipleSessions", false),
                TokenEncryptionKey = tokenEncryptionKey,
                AdministratorUserName = identity["AdministratorUserName"] ?? "admin",
                AdministratorPassword = administratorPassword,
                AdministratorRoleName = identity["AdministratorRoleName"] ?? "Administrators",
                TokenValidationMode = TokenValidationMode.DecryptOnly
            });
        }

        private static string Require(IConfigurationSection section, string key)
        {
            var value = section[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Missing configuration value 'Identity:{key}'. Set it via user secrets " +
                    $"(dotnet user-secrets set \"Identity:{key}\" \"<value>\") or the " +
                    $"Identity__{key} environment variable.");
            }

            return value;
        }
    }
}
