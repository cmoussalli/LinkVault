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


            IDFManager.Configure(new IDFManagerConfig
            {
                DatabaseType = DatabaseType.MSSQL,
                //DBConnectionString = "Data Source=LinkVault.db",
                DBConnectionString = "Server=10.38.38.199;Database=LinkVault;User Id=sa;Password=SMedi@33333;TrustServerCertificate=True",
                DefaultListPageSize = 25,
                DBLifeCycle = DBLifeCycle.Both,
                IsActiveByDefault = true,
                IsLockedByDefault = false,
                DefaultTokenLifeTime = new LifeTime(365, 0, 0),
                AllowUserMultipleSessions = false,
                TokenEncryptionKey = "123456",
                AdministratorUserName = "admin",
                AdministratorPassword = "admin",
                AdministratorRoleName = "Administrators",
                TokenValidationMode = TokenValidationMode.DecryptOnly

            });

            LinkVaultDbContext db = new();
            db.Database.EnsureCreated();
            db.InsertMasterData();
            IDFManager.RefreshIDFStorage();



            app.Run();
        }
    }
}
