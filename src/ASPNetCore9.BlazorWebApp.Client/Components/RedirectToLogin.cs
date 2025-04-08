namespace ASPNetCore9.BlazorWebApp.Client.Client.Components;

using Microsoft.AspNetCore.Components;

public class RedirectToLogin (NavigationManager navigationManager) : ComponentBase
{
    protected override void OnInitialized()
    {
        navigationManager.NavigateTo($"Account/Login?returnUrl={Uri.EscapeDataString(navigationManager.Uri)}", forceLoad: true);
    }
}
