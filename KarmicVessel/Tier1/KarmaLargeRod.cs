using System.Collections;
using KarmicVessel.Other;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;

namespace KarmicVessel.Tier1
{
    public class KarmaLargeRod : SpellSkillData
    {
        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if (!(spell is KarmaBase))
                return;
            
            Player.local.GetHand(caster.side).OnFistEvent += PlayerHandOnOnFistEvent;

        }
        
        
        
        public static void PlayerHandOnOnFistEvent(PlayerHand hand, bool gripping)
        {
            var karma = hand.ragdollHand.caster.spellInstance as KarmaBase;
            
            if(!gripping || karma.ability != ModOptions.SpellHands.ChakraRods)
                return;
            
            var ChakraSound= AssetStorage.KokuganChakraSFX.Spawn(Player.currentCreature.ragdoll.targetPart.transform);
            ChakraSound.Play();
            
            
            AssetStorage.GetAssetSafe<ItemData>("LargeChakraRod").SpawnAsync(item =>
            {
                var model = item.GetCustomReference("ChakraModel");
                model.transform.localScale = new Vector3(1 * ModOptions.LargeRodWidth, 0.05f, 1 * ModOptions.LargeRodWidth);
                hand.ragdollHand.Grab(item.GetMainHandle(hand.side), true);
                item.StartCoroutine(RodGrowRoutine(model, item, hand));
            });
            
        }


        static IEnumerator RodGrowRoutine(Transform item, Item mainitem, PlayerHand hand)
        {
            
            while (item.localScale.y < ModOptions.LargeRodLength)
            {
                
                if (hand.controlHand.gripPressed && hand.controlHand.castPressed)
                {
                    
                    item.localScale += new Vector3(0, 0.014f * ModOptions.GrowthSpeed, 0);
                    
                    yield return new WaitForEndOfFrame();
                    
                }
                else
                    break;
            }
            mainitem.transform.localScale = item.localScale;
            item.localScale = Vector3.one;
            if (hand.controlHand.gripPressed)
            {
                hand.ragdollHand.UnGrab( true);
                hand.ragdollHand.Grab(mainitem.GetMainHandle(hand.side), true);
            }
            else
            {
                float diff = -(mainitem.transform.localScale.y - 1f);
                mainitem.transform.Translate(Vector3.up * diff, Space.Self);
            }
            


            yield return null;
        }

        
        
        
        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);
            if (!(spell is KarmaBase karmaSpell))
                return;
            
            Player.local.GetHand(caster.side).OnFistEvent -= PlayerHandOnOnFistEvent;

            
        }
    }
}