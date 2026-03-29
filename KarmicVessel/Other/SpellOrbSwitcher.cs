using System.Collections;
using System.Collections.Generic;
using System.Web;
using KarmicVessel.ItemModules;
using KarmicVessel.Tier1;
using KarmicVessel.Tier2;
using ThunderRoad;
using UnityEngine;
using QualityLevel = ThunderRoad.QualityLevel;

namespace KarmicVessel.Other
{
    public class SpellOrbSwitcher : ThunderScript 
    {
    
        public static GameObject OrbLeft;
        public static GameObject OrbRight;


        public static IEnumerator OrbJustEnabled(SpellCaster caster)
        {
            var karmaSpell = caster?.spellInstance as KarmaBase;
            if (caster == null || karmaSpell == null) yield break;

            var orb = GetOrb(caster);
            if (orb == null) yield break;

            const float target = 0.1f;
            const float speed = 0.5f;

            orb.SetActive(true);

            float scale = orb.transform.localScale.x;

            while (scale < target)
            {
                scale += speed * Time.deltaTime;
                orb.transform.localScale = Vector3.one * scale;

                yield return null;
            }
            orb.transform.localScale = Vector3.one * target;
        }
        public static IEnumerator OrbJustDisabled(SpellCaster caster)
        {
            if (caster == null) yield break;

            var orb = GetOrb(caster);
            if (orb == null) yield break;

            const float speed = 0.5f;

            float scale = orb.transform.localScale.x;

            while (scale > 0f)
            {
                
                scale -= speed * Time.deltaTime;
                orb.transform.localScale = Vector3.one * scale;

                yield return null;
                
            }
            
            orb.SetActive(false);
            
        }

        public static GameObject GetOrb(SpellCaster caster)
        {
            if(caster.side == Side.Left)
                return OrbLeft;
            return OrbRight;
        }
        
        public static void UpdateOrb(SpellCaster caster, ModOptions.SpellHands type)
        {
            if(caster?.spellInstance.id != "KarmicVesselSpell")
                return;
            
            ref GameObject orb = ref OrbRight;
            if(caster.side == Side.Left)
                orb = ref OrbLeft;

            
            // Destroy Orb 
            if (orb != null)
            {
                
                var qn = orb.GetComponentInChildren<Item>(); 
                if(qn != null) 
                    qn.Despawn(); 
                GameObject.Destroy(orb);
                 
                
            }
            
            //Spawn Orb 
            switch (type)
            {
                
                    
                case ModOptions.SpellHands.ChakraRods:
                    GenerateOrbChakraRods(caster, out orb);
                    break;
                case ModOptions.SpellHands.Shrink:
                    GenerateOrbShrink(caster, out orb);
                    break;
                case ModOptions.SpellHands.Daikokuten:
                    GenerateOrbDaiko(caster, out orb);
                    break;
            }


            var karma = caster.spellInstance as KarmaBase;
            if (!karma.isCasting)
            {
                orb.SetActive(false);
            }
            
            
        }
        
        static void GenerateOrbChakraRods(SpellCaster caster, out GameObject orb)
        {
            orb = GameObject.Instantiate(AssetStorage.GetAssetSafe<GameObject>("AssetKarmaChakra"));
            orb.name = "KarmaSpellOrb" + caster.side;
            orb.transform.position = caster.Orb.transform.position;
            orb.transform.SetParent(caster.Orb.transform);
            dbg.Log("Generated Orb Chakra Rods");
        }

        static void GenerateOrbShrink(SpellCaster caster, out GameObject orb)
        {

            var _orb = GameObject.Instantiate(AssetStorage.GetAssetSafe<GameObject>("AssetKarmaShrink"));
            _orb.transform.position = caster.Orb.transform.position;
            
            
            
            if (Sukunahikuna.currentTarget != null)
            {
                Sukunahikuna.currentTarget.item.data.SpawnAsync(q =>
                {
                    q.ScaleToGlobalSize(0.13f);
                    q.SetColliders(false);
                    q.RefreshCollision();
                    q.name = "KarmaSpellOrb" + caster.side;
                    Vector3 worldCenter = q.transform.transform.TransformPoint(q.GetLocalCenter());
                    q.transform.position += caster.Orb.transform.position - worldCenter;
                    q.GetOrAddComponent<CustomLoop>().code = () =>
                    {
                        q.transform.Rotate(0, 0, 100f * Time.deltaTime);
                    };
                    q.handles.Clear();
                    q.physicBody.useGravity = false;
                    q.physicBody.isKinematic = true;
                    q.transform.SetParent(_orb.transform);
                    dbg.Log("Generated Orb Shrink");
                });
            }
            orb = _orb;
            orb.transform.SetParent(caster.Orb.transform);

            
        }
        
        

        
        static void GenerateOrbDaiko(SpellCaster caster, out GameObject orb)
        {

            orb = new GameObject();
            
            orb.name = "KarmaSpellOrb" + caster.side;
            orb.transform.position = caster.Orb.transform.position;
            orb.transform.SetParent(caster.Orb.transform);
            dbg.Log("Generated Orb Daikokuten");
        }
        
        
    }
}