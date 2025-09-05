// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("taMWg2hGwvmBm9rIC+wH6iF9iSYFrKG8OP6pyqErpFKdx1hYz4GQGPcayNhAHLKjvmvZ/rSOFEafxDvqyXI8aLwtfHNMOfE+EcA/aEbtZhRhSalXgTtNaMzTdkEfBSupZkkre9aTEPdXpbkxYAwXbaae+fWU1LN2y6ftNsNNawkyJo/m0822os+HBrdTmZMnCK/Glj2EvE7bgHeWD+80i07OzDKSnkbGVt6qEm3llNfRUw+f+kjL6PrHzMPgTIJMPcfLy8vPyslIy8XK+kjLwMhIy8vKBGpA32z/hNS186SRWTa4rgGtw15UW8zdNCSiaF3rvRE0WaE5EAXRR0e6X0N7VclQ2rbX0p2MduGTABzZucgYpmoxyRYdNpP1zGkhW8jJy8rL");
        private static int[] order = new int[] { 12,11,7,10,9,7,13,9,10,12,11,12,13,13,14 };
        private static int key = 202;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
