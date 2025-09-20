using Abstract;


public class InAppsManager : PreloadableSingleton<InAppsManager> {
    public IInAppsProvider InAppsProvider { get; private set; }

    protected override void OnFirstInit()
    {
        base.OnFirstInit();

#if GOOGLE_PLAY
        InAppsProvider = new GooglePlayInAppsProvider();
#else
        InAppsProvider = new InAppsProviderMock();
#endif
        
       
        InAppsProvider.Init();
    }
}
