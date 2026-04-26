using Armali.Horizon.Blazor.Services;
using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.Core.Logs;
using Armali.Horizon.Identity.Components;
using Armali.Horizon.Identity.Handlers;
using Armali.Horizon.Identity.Services;
using Armali.Horizon.IO;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Identity;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Logging centralizado con Serilog + Seq
        builder.Host.UseHorizonLogging();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddScoped<HorizonSessionService>();

        builder.Services.AddDbContextFactory<IdentityDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetSection("Horizon")["ConnectionStrings:Identity"]));

        // Servicio de dominio Identity (resuelto en cada scope creado por el bus IO)
        builder.Services.AddScoped<IdentityService>();
        
        // Limpieza periódica de tokens caducados/revocados
        builder.Services.AddHostedService<TokenCleanupService>();

        // Bus de eventos Horizon: registramos todos los handlers de Identity en el canal "identity"
        builder.Host.UseHorizonEvents(events =>
        {
            events
                .HandleRequest<LoginHandler, LoginRequest, LoginResponse>(IdentityChannels.Channel)
                .HandleRequest<LogoutHandler, LogoutRequest, LogoutResponse>(IdentityChannels.Channel)
                .HandleRequest<WhoAmIHandler, WhoAmIRequest, WhoAmIResponse>(IdentityChannels.Channel)
                .HandleRequest<ChangePasswordHandler, ChangePasswordRequest, ChangePasswordResponse>(IdentityChannels.Channel)
                .HandleRequest<ListUsersHandler, ListUsersRequest, ListUsersResponse>(IdentityChannels.Channel)
                .HandleRequest<CreateUserHandler, CreateUserRequest, CreateUserResponse>(IdentityChannels.Channel)
                .HandleRequest<UpdateUserHandler, UpdateUserRequest, UpdateUserResponse>(IdentityChannels.Channel)
                .HandleRequest<DeleteUserHandler, DeleteUserRequest, DeleteUserResponse>(IdentityChannels.Channel)
                .HandleRequest<SetUserRolesHandler, SetUserRolesRequest, SetUserRolesResponse>(IdentityChannels.Channel)
                .HandleRequest<ListRolesHandler, ListRolesRequest, ListRolesResponse>(IdentityChannels.Channel)
                .HandleRequest<CreateTokenHandler, CreateTokenRequest, CreateTokenResponse>(IdentityChannels.Channel)
                .HandleRequest<ListTokensHandler, ListTokensRequest, ListTokensResponse>(IdentityChannels.Channel)
                .HandleRequest<RevokeTokenHandler, RevokeTokenRequest, RevokeTokenResponse>(IdentityChannels.Channel);
        });

        var app = builder.Build();

        // Aplicar migraciones pendientes automáticamente (también en dev: la BD aún no existe).
        using (var db = app.Services.GetRequiredService<IDbContextFactory<IdentityDbContext>>().CreateDbContext())
        {
            db.Database.Migrate();
        }

        // Seed inicial: armali/armali (configurable). Sólo crea el usuario si la BD está vacía.
        using (var scope = app.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IdentityService>();
            var seedSection = builder.Configuration.GetSection("Horizon:Seed");
            var seedUser = seedSection["UserName"] ?? "armali";
            var seedPwd = seedSection["Password"] ?? "armali";
            svc.EnsureSeedAsync(seedUser, seedPwd).GetAwaiter().GetResult();
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseAntiforgery();

        // Endpoint de salud para smoke tests / readiness probes. No toca la BD
        // para no fallar durante el arranque o ante problemas transitorios.
        app.MapGet("/health", () => Results.Ok(new { status = "ok", app = "identity" }));

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(typeof(Armali.Horizon.Blazor._Imports).Assembly);

        app.Run();
    }
}