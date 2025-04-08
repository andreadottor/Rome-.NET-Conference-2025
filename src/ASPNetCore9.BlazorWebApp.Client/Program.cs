using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Security.Claims;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization(configure =>
{
    configure.DeserializationCallback = (AuthenticationStateData? authenticationStateData) =>
    {
        if (authenticationStateData is null)
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        return Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(
                new ClaimsIdentity(authenticationStateData.Claims.Select(c => new Claim(c.Type, c.Value)),
                    authenticationType: "DeserializedAuthenticationStateProvider",
                    nameType: authenticationStateData.NameClaimType,
                    roleType: authenticationStateData.RoleClaimType))));
    };
});

await builder.Build().RunAsync();
