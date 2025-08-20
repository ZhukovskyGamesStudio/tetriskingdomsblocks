using ZhukovskyGamesPlugin;

namespace Abstract {
    public class PreloadableSingleton<T> : SingletonBase<T>, IPreloadable where T : CustomMonoBehaviour {
        
        public void Init() {
            CreateSingleton();
        }
    }
}