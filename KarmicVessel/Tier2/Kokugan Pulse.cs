using KarmicVessel.ItemModules;
using KarmicVessel.Other;
using KarmicVessel.Tier1;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;

namespace KarmicVessel.Tier2
{
    public class KokuganPulse : SpellSkillData
    {
        public float LastTime = 0f;
        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if (!(spell is  KarmaBase karmaSpell))
                return;

            var hand = Player.local.GetHand(karmaSpell.spellCaster.side);
            hand.OnFistEvent += HandOnOnFistEvent;

        }

        private void HandOnOnFistEvent(PlayerHand hand, bool gripping)
        {
            var spell = Player.currentCreature.mana.GetCaster(hand.side).spellInstance as KarmaBase;
            if(spell.ability != ModOptions.SpellHands.Shrink || !gripping)
                return;

            if (Time.time - LastTime > ModOptions.PulseCooldwonSpeed)
            {
                var ChakraSound = AssetStorage.GetAssetSafe<EffectData>("KokuganShrinkSFX").Spawn(Player.currentCreature.ragdoll.targetPart.transform);
                ChakraSound.Play();
                foreach (var entity in Item.InRadius(spell.spellCaster.Orb.position, 5f))
                {
                    var item = entity as Item;
                    if (item != null)
                    {
                        var flash = GameObject.Instantiate(Sukunahikuna.go);
                        flash.transform.position = item.transform.position;
                        flash.transform.localScale *= 0.1f;
                        
                        item.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        var c = item.GetOrAddComponent<ItemHomingBehavior>();
                        var f = Creature.InRadius(Player.currentCreature.transform.position, 5f);
                        f.Remove(Player.currentCreature);
                        c.target = f.RandomChoice();
                    }
                }
                LastTime = Time.time; 
            }
        }

        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);
            if (!(spell is  KarmaBase karmaSpell))
                return;

            var hand = Player.local.GetHand(karmaSpell.spellCaster.side);
            hand.OnFistEvent -= HandOnOnFistEvent;
        }
    }
}