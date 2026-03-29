using System.Collections;
using System.Linq;
using KarmicVessel.ItemModules;
using KarmicVessel.Other;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;

namespace KarmicVessel.Tier1
{
    public class ChakraSpike : SpellSkillData
    {
        //ignore these stuff
        public float velocityThreshold = 1.5f; 
        public float angleThreshold = 30f; 
        private float lastGestureTime = 0f;
        private int SpikeAmount = 12;
        
        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if (!(spell is  KarmaBase karmaSpell )) //instead of KarmaBase use the class of ur own spell
                return;
            
            karmaSpell.OnSpellUpdateEvent += KarmaSpellOnOnSpellUpdateEvent;
            //this will run at the same time as UpdateCaster

        }

        private void KarmaSpellOnOnSpellUpdateEvent(SpellCastCharge spell)
        {

            if (spell.spellCaster.side == Side.Right &&
                ModOptions.RightHandAbilities != ModOptions.SpellHands.ChakraRods)
            {
                lastGestureTime = 0f;
                return;
            }
            
            if(spell.currentCharge == 0)
                return;
            
            if (Time.time - lastGestureTime < ModOptions.BloomCooldownTime)
                return;
            
            
            var hand = spell.spellCaster.ragdollHand; 
            var velocity = hand.Velocity();
            var speed = velocity.magnitude;
            var angle = Vector3.Angle(Vector3.up * 2.5f, velocity.normalized);
                
            var canGesture = (speed > velocityThreshold && angle < angleThreshold && Vector3.Angle(hand.PalmDir, -Vector3.up) > 120.0);


            if (canGesture)
            {
                HandleGesture();
                dbg.Log("Attempted Gesture");
            }
        }

        void HandleGesture()
        {
            Vector3 centerPos;
            if (Physics.SphereCast(Player.local.head.cam.transform.position + Player.local.head.cam.transform.forward * 0.35f, 0.3f,
                    Player.local.head.cam.transform.forward, out var hit, 20f))
            {
                dbg.DrawLine(Player.local.head.cam.transform.position, hit.point, 0.3f, Color.red);
            }
            else
                return;
            
            var nearby = Creature.InRadius(hit.point, 0.4f);
            var target = nearby.FirstOrDefault(c => !c.isPlayer);

            if (target == null)
                return;
                
            
            lastGestureTime = Time.time;
            dbg.Log("gesture - SpehereCast hit creature ");
            
            dbg.DrawLine(target.ragdoll.GetPart(RagdollPart.Type.Head).transform.position, target.ragdoll.GetPart(RagdollPart.Type.LeftFoot).transform.position, 0.05f, Color.magenta); 
            dbg.Log(target.name); 
            target.ragdoll.SetState(Ragdoll.State.Destabilized); 
            centerPos = target.ragdoll.targetPart.transform.position; 
            dbg.Log("Chakra Spike Gesture Activated"); 
            
            for (var i = 0; i < SpikeAmount; i++) 
                AssetStorage.GetAssetSafe<ItemData>("LargeChakraRod").SpawnAsync(item =>
                {
                    var pos = centerPos - item.flyDirRef.forward * 1.4f;
                    GameManager.local.StartCoroutine(MakeInvisible(item));
                    item.Despawn(10);
                    item.data.damagers.ForEach(damager =>
                    {
                        damager.damagerData.penetrationDamper *= 0.1f;
                    } );
                    item.transform.localScale = new Vector3(Random.Range(0.7f, 1.4f), Random.Range(0.85f, 1.15f ), Random.Range(0.7f, 1.4f));
                
                    item.transform.position = pos + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                    item.Throw();

                    var c = item.GetOrAddComponent<ItemHomingBehavior>();
                    c.target = target;
                    

                }); 
            
        }

        IEnumerator MakeInvisible(Item item)
        {
            item.renderers.ForEach(r => r.enabled = false);
            yield return Yielders.ForSeconds(0.4f);
            if(item.isPenetrating)
                item.renderers.ForEach(r => r.enabled = true);
            else
            {
                item.Despawn();
            }

        }


        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);
            if (!(spell is KarmaBase karmaSpell))
                return;
            
            karmaSpell.OnSpellUpdateEvent -= KarmaSpellOnOnSpellUpdateEvent;
            
        }
    }
}