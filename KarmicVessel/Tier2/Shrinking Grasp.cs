using KarmicVessel.Other;
using KarmicVessel.Tier1;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;

namespace KarmicVessel.Tier2
{
    public class ShrinkingGrasp : SpellSkillData
    {
        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if (!(spell is  KarmaBase karmaSpell))
                return;
            
            karmaSpell.OnGripCastEvent += KarmaSpellOnOnGripCastEvent;

        }

        private void KarmaSpellOnOnGripCastEvent(SpellCaster caster, HandleRagdoll handle)
        {
            if(caster.spellInstance is KarmaBase karmaSpell && karmaSpell.ability != ModOptions.SpellHands.Shrink)
                return;

            RagdollHand hand = null;
            dbg.Log($"Grip Cast Event:{handle.ragdollPart.name}");
            if (handle.ragdollPart.name == "LeftForeArm")
                hand = handle.ragdollPart.ragdoll.creature.handLeft;
            if(handle.ragdollPart.name == "RightForeArm")
                hand = handle.ragdollPart.ragdoll.creature.handRight;
            
            if(hand != null)
                if(hand.grabbedHandle != null)
                    if (hand.grabbedHandle.item.transform.localScale.x > 0.1f)
                    {
                        var flash = GameObject.Instantiate(Sukunahikuna.go);
                        flash.transform.position = hand.grabbedHandle.item.transform.position;
                        flash.transform.localScale *= 0.1f;
                        
                        var ChakraSound = AssetStorage.GetAssetSafe<EffectData>("KokuganShrinkSFX").Spawn(Player.currentCreature.ragdoll.targetPart.transform);
                        ChakraSound.Play();
                        hand.grabbedHandle.item.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    }



        }

        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);
            if (!(spell is  KarmaBase karmaSpell))
                return;
            
            karmaSpell.OnGripCastEvent -= KarmaSpellOnOnGripCastEvent;
        }
    }
}