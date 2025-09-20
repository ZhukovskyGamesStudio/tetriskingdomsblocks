// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("V9Ta1eVX1N/XV9TU1VZYk6PMw5krMwhgiPJKcvh9yw8DvJccU9swfAG8OqC91JRFGiRAcgcbK+KLJxbgoWo6+ALjLuN5Uvqhkji/0BJZxoDOCP7MW0rAV1ByznAly/c5+3q80XgmBXD0fP7D7VMzHwnocHG5w5m6fl7/qxdlDHMDp1tZJw0hC7Mq+iQ048aliyAKsiGzQHZ68iP3Z+IBDDILrAo+7TuA0/qiYdqf8Xj2RK5XPczB5klGrklG58+RimB1cMLr3Q5tIJBT04eHeXiG5579Lv6ssBEdBrFdasnwE1NSqMUutLy4rP1BMQUF5WOG/mk34LFKm4lq8y9fM79eSDXlV9T35djT3P9TnVMi2NTU1NDV1s0JTWaEPtrDoNfW1NXU");
        private static int[] order = new int[] { 1,6,2,10,11,10,9,11,8,10,13,11,12,13,14 };
        private static int key = 213;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
