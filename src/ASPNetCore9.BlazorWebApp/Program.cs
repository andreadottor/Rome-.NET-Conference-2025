using ASPNetCore9.BlazorWebApp.Client.Pages;
using ASPNetCore9.BlazorWebApp.Components;
using ASPNetCore9.BlazorWebApp.Components.Account;
using ASPNetCore9.BlazorWebApp.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(configure =>
    {
        configure.SerializeAllClaims = false;
        configure.SerializationCallback = (authenticationState) =>
        {
            AuthenticationStateData? data = null;

            if (authenticationState.User.Identity?.IsAuthenticated ?? false)
            {
                data = new AuthenticationStateData();

                if (authenticationState.User.Identities.FirstOrDefault() is { } identity)
                {
                    data.NameClaimType = identity.NameClaimType;
                    data.RoleClaimType = identity.RoleClaimType;
                }

                if (authenticationState.User.FindFirst(data.NameClaimType) is { } nameClaim)
                {
                    data.Claims.Add(new(nameClaim));
                }

                foreach (var roleClaim in authenticationState.User.FindAll(data.RoleClaimType))
                {
                    data.Claims.Add(new(roleClaim));
                }

                // add custom claims
                var timeZone = authenticationState.User.FindFirst("time-zone")?.Value;
                if (!string.IsNullOrEmpty(timeZone))
                {
                    data.Claims.Add(new("time-zone", timeZone));
                }

                data.Claims.Add(new("theme", "dark"));
            }

            return ValueTask.FromResult(data);
        };
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ASPNetCore9.BlazorWebApp.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
