using Microsoft.AspNetCore.Components;

namespace Flipbook_App.Tests;

public class TestNavigationManager : NavigationManager
{
    public string? LastUri { get; private set; }
    public bool? LastForceLoad { get; private set; }
    public bool? LastReplace { get; private set; }

    public TestNavigationManager()
    {
        Initialize("http://localhost/", "http://localhost/");
    }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        LastUri = uri;
        LastForceLoad = forceLoad;
        LastReplace = false;
    }

    public void NavigateTo(string uri, bool forceLoad, bool replace)
    {
        LastUri = uri;
        LastForceLoad = forceLoad;
        LastReplace = replace;
    }
}
