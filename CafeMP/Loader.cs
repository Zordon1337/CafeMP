using System;
using UnityEngine;

namespace CafeMP
{
    public class Loader
    {
        public static void Init()
        {
            Loader.MPDLL = new UnityEngine.GameObject();
            Loader.MPDLL.AddComponent<MP>();
            UnityEngine.Object.DontDestroyOnLoad(Loader.MPDLL);
        }
        public static void Unload()
        {

        }
        private static void _Unload()
        {
            
        }

        private static GameObject MPDLL;
    }
}
