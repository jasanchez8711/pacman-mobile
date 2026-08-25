namespace PacmanMobile;

public sealed class App : Application
{
    public App()
    {
        UserAppTheme = AppTheme.Dark;
        MainPage = new MainPage();
    }
}
