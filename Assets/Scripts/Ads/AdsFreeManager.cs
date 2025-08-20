using Abstract;

public class AdsFreeManager : PreloadableSingleton<AdsFreeManager> {
    public IAdsFreeProvider AdsFreeProvider { get; private set; }

    protected override void OnFirstInit() {
        base.OnFirstInit();
        
#if MADPIXEL
            AdsFreeProvider = new MPAdsFreeProvider();
#elif YG_PLATFORM
        AdsFreeProvider = new YGAdsFreeProvider();
#else
        AdsFreeProvider = new AdsFreeProviderMock();
#endif
       
        
        AdsFreeProvider.Init();
    }
}
