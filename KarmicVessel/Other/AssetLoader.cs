using System;
using System.Collections;
using KarmicVessel.Tier2;
using ThunderRoad;
using UnityEngine;

namespace KarmicVessel.Other
{

    
    public class AssetStorage
    {
        public static GameObject AssetKarmaChakra;
        public static GameObject AssetKarmaShrink;
        public static Shader Assetlitmoss;
        public static ItemData SmallChakraRod;
        public static ItemData LargeChakraRod;
        public static  EffectData KokuganChakraSFX;
        public static EffectData KokuganShrinkSFX;

        public static void LoadAll()
        {
            dbg.Log("Loading Assets");
            LoadCatalog();
            LoadAddressables();
        }

        public static void UnloadAll()
        {
            Catalog.ReleaseAsset("AssetChakraOrb");
        }


        

        public static void LoadAddressables()
        {
            dbg.Log("Loading Addressables");
            try
            {
                Catalog.LoadAssetAsync<GameObject>("KarmaChakraOrb", q =>
                {
                    dbg.Log("Loaded Chakra Orb");
                    AssetKarmaChakra = q;
                }, "AssetChakraOrb");
                
                Catalog.LoadAssetAsync<GameObject>("KarmaShrinkOrb", q =>
                {
                    dbg.Log("Loaded Shrink Orb");
                    AssetKarmaShrink = q;
                }, "AssetShrinkOrb");

                //Catalog.LoadAssetAsync<Shader>("Shader.litmoss", q => { Assetlitmoss = q; }, "AssetLitmoss");
            }
            catch (System.Exception e)
            {
                
                dbg.Log("Failed to load assets: " + e + "");
            }
            
            
        }

        public static void LoadCatalog()
        {
            dbg.Log("Loading Catalog");
            try
            {
                dbg.Log("Getting Assets For Karmic Vessel");
                SmallChakraRod = Catalog.GetData<ItemData>("Karma_SmallChakraRod");
                LargeChakraRod = Catalog.GetData<ItemData>("Karma_LargeChakraRod");
                KokuganChakraSFX = Catalog.GetData<EffectData>("KokuganChakraSFX");
                KokuganShrinkSFX = Catalog.GetData<EffectData>("KokuganShrinkSFX");
            }
            catch (Exception e)
            {
                dbg.Log("Failed to load Catalog Assets: " + e + "");
            }
        }
        

        public static T GetAssetSafe<T>(string address) where T : class
        {
            var fi = typeof(AssetStorage).GetField(
                address,
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            var value = fi?.GetValue(null) as T;
            if (value != null) return value;
            
            dbg.Log($"Failed to get asset {address}, null or wrong type");
            return null;

        }
    }
    
}