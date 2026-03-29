using KarmicVessel.Other;
using KarmicVessel.Tier1;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;

namespace KarmicVessel.Tier2
{
    public class ShrinkageField : SpellSkillData
    {
        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if (!(spell is  KarmaBase karmaSpell))
                return;
            
            karmaSpell.OnSpellUpdateEvent += KarmaSpellOnOnSpellUpdateEvent;

        }
        
        private void KarmaSpellOnOnSpellUpdateEvent(SpellCastCharge spell)
        {
            var instance = spell as KarmaBase;
            if (instance.isCasting && instance.ability == ModOptions.SpellHands.Shrink)
            {
                foreach ( var entity in Item.InRadius(instance.spellCaster.Orb.position, 1f))
                {
                    var item = entity as Item;
                    if(item != null)
                        if (item.isFlying)
                        {
                            var flash = GameObject.Instantiate(Sukunahikuna.go);
                            flash.transform.position = item.transform.position;
                            flash.transform.localScale *= 0.1f;

                            var ChakraSound = AssetStorage.GetAssetSafe<EffectData>("KokuganShrinkSFX").Spawn(Player.currentCreature.ragdoll.targetPart.transform);
                            ChakraSound.Play();
                            dbg.Log("Auto Shrinking " + item.name);
                            item.StopFlying();
                            item.physicBody.velocity = Vector3.zero;
                            item.transform.localScale *= 0.1f;
                        }
                }
            }
        }

        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);
            
            if (!(spell is  KarmaBase karmaSpell))
                return;
            
            karmaSpell.OnSpellUpdateEvent -= KarmaSpellOnOnSpellUpdateEvent;
        }
    }
}