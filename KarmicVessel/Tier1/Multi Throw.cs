using KarmicVessel.Other;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;

namespace KarmicVessel.Tier1
{
    public class RodScatter : SpellSkillData
    {
        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if (!(spell is  KarmaBase karmaSpell))
                return;
            
            KarmaBase.LaunchSpeed = 20f;

            karmaSpell.OnSpellUpdateEvent += KarmaSpellOnOnSpellUpdateEvent;
        }

        private void KarmaSpellOnOnSpellUpdateEvent(SpellCastCharge spell)
        {
            {
                if(spell.spellCaster?.side == Side.Right && ModOptions.RightHandAbilities != ModOptions.SpellHands.ChakraRods)
                    return;
                if(spell.spellCaster?.side == Side.Left && ModOptions.LeftHandAbilities != ModOptions.SpellHands.ChakraRods)
                    return;
                
                if(KarmaBase.lastCaster == null || KarmaBase.lastItem == null)
                    return;

                Item[] KarmicItems = new Item[5]; 
                KarmicItems[0] = KarmaBase.lastItem;
                
                dbg.Log("Rod Scatter Update");
                if (KarmaBase.lastCaster.ragdollHand.playerHand.controlHand.castPressed && !KarmaBase.lastItem.isPenetrating )
                {
                    dbg.Log("Rod Scatter Fired");
                    var lastitem = KarmaBase.lastItem;
                    Vector3[] positions =
                    {
                        lastitem.transform.position + (0.2f * lastitem.transform.right),
                        lastitem.transform.position + (-0.2f * lastitem.transform.right),
                        lastitem.transform.position + (0.2f * lastitem.transform.up),
                        lastitem.transform.position + (-0.2f * lastitem.transform.up)
                    };
                    
                    for (int i = 0; i < 4; i++)
                    {
                        
                        var centerpos = positions[i];

                        var v = i;
                        AssetStorage.GetAssetSafe<ItemData>("SmallChakraRod").SpawnAsync(item =>
                        {
                            item.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
                            item.Despawn(13f);
                            item.renderers.ForEach(r => r.enabled = false);
                            item.transform.rotation = lastitem.transform.rotation;
                            /*
                            item.GetOrAddComponent<DefaultModule>();
                            if (StunningRods)
                                item.GetOrAddComponent<ItemStunBehaviour>();
                            if (HomingRods)
                                item.GetOrAddComponent<ItemHomingBehavior>();
                            */
                            item.transform.position = centerpos;
                            item.Throw();
                            item.renderers.ForEach(r => r.enabled = true);
                            item.physicBody.velocity = lastitem.Velocity;
                            foreach (var kItem in KarmicItems)
                            {
                                item.IgnoreItemCollision(kItem);
                            }
                            KarmicItems[v+1] = item;
                        });

                    }
                    KarmaBase.lastItem = null;
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