using System;
using System.Collections;
using System.Linq;
using IngameDebugConsole;
using KarmicVessel.ItemModules;
using KarmicVessel.Other;
using KarmicVessel.Tier1;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;
using Object = System.Object;

namespace KarmicVessel.Tier3

{
    public class PortalCannon : SpellSkillData
    {
        
        public float cooldown = 1.2f; 
        private Vector3 lastPos;
        private float lastTriggerTime;
        public float velocityThreshold = 1.5f; 
        public float angleThreshold = 30f; 

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);
            Action<GestureUtils.Direction> OGesture = OnGesture;
            DebugLogConsole.AddCommand("PortalShoot", "does a portal thing", OGesture, "direction");

        }


        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if(!(spell is KarmaBase karmaSpell))
                return;
            
            if(!KarmaBase.SpellAbilityCycle.Contains(ModOptions.SpellHands.Daikokuten))
                KarmaBase.SpellAbilityCycle.Add(ModOptions.SpellHands.Daikokuten);
            
            
            
            karmaSpell.OnSpellUpdateEvent += KarmaSpellOnOnSpellUpdateEvent;

            
        }
        
        private void KarmaSpellOnOnSpellUpdateEvent(SpellCastCharge _spell)
        {
            var spell = _spell as KarmaBase;
            if(spell.ability != ModOptions.SpellHands.Daikokuten) return;

            if (spell.DaikokutenItem == null && Time.time - lastTriggerTime > cooldown) 
            {
                var hand = spell.spellCaster.ragdollHand; 
                var velocity = hand.Velocity();
                var speed = velocity.magnitude;
                
                
                var cameraTransform = Player.local.head.cam.transform;
                GestureUtils.Direction dir = GestureUtils.Direction.None;


                var direction = cameraTransform.up * -1;
                var angle = Vector3.Angle(direction, velocity.normalized);
                if((speed > velocityThreshold && angle < angleThreshold && Vector3.Dot(hand.PalmDir.normalized, direction) > 0.5f))
                    dir = GestureUtils.Direction.Down;
                
                direction = cameraTransform.right;
                angle = Vector3.Angle(direction, velocity.normalized);
                if((speed > velocityThreshold && angle < angleThreshold && Vector3.Dot(hand.PalmDir.normalized, direction) > 0.5f))
                    dir = GestureUtils.Direction.Right;
                
                
                direction = cameraTransform.right * -1;
                angle = Vector3.Angle(direction, velocity.normalized);
                if((speed > velocityThreshold && angle < angleThreshold && Vector3.Dot(hand.PalmDir.normalized, direction) > 0.5f))
                    dir = GestureUtils.Direction.Left;
                
                direction = cameraTransform.forward * -1;
                angle = Vector3.Angle(direction, velocity.normalized);
                if((speed > velocityThreshold && angle < angleThreshold && Vector3.Dot(hand.PalmDir.normalized, direction * -1) > 0.5f))
                    dir = GestureUtils.Direction.Backward;


                var allowedDirections = new[] {GestureUtils.Direction.Backward, GestureUtils.Direction.Left, GestureUtils.Direction.Right, GestureUtils.Direction.Down};
                if (allowedDirections.Contains(dir))
                {
                    OnGesture(dir);
                    lastTriggerTime = Time.time;
                }
            }
            
        }
        
        [ConsoleMethod( "PortalShoot", "does a portal thing", "direction")]
        void OnGesture(GestureUtils.Direction dir)
        {
            var te = Creature.AimAssist(Player.currentCreature.transform.position, Player.local.head.cam.transform.forward, 50, 90,
                entity => ((entity is Creature creat) && !creat.isKilled),ignoredEntity: Player.currentCreature );
            if(!(te is Creature creature)) return;
            Vector3 creaturePortalOffset = Vector3.zero;
            
            
            switch (dir)
            {
                case GestureUtils.Direction.Forward:
                    return;

                case GestureUtils.Direction.Backward:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.forward * -1f;
                    break;

                case GestureUtils.Direction.Left:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.up * -1f;
                    break;

                case GestureUtils.Direction.Right:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.up * 1f;

                    break;
                
                case GestureUtils.Direction.Down:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.right * -1f;
                    break;
                
                case GestureUtils.Direction.Up:
                    return;
                    
                    
            }
            
            var portalPos = creature.ragdoll.targetPart.transform.position + creaturePortalOffset;

            Player.local.StartCoroutine(SpawnPortalCoroutine(portalPos, creature.ragdoll.targetPart.transform, true,
                1f, true, creature));

        }
        
        private static GameObject SpawnPortal(Vector3 pos, Transform target)
        {
            var portal = GameObject.Instantiate(AssetStorage.AssetKarmaPortal);
            portal.transform.position = pos;
            portal.transform.LookAt(target);
            portal.transform.Rotate(0f, -91.7f,-106.97f);
            if (ModOptions.ShowDebugRays)
            {
                var lr = portal.GetOrAddComponent<LineRenderer>();
                lr.startWidth = 0.01f;
                lr.endWidth = 0.01f;
                lr.SetPosition(0, portal.transform.position);
                lr.SetPosition(1, target.position);
            }


            return portal;
        }



        public static IEnumerator SpawnPortalCoroutine(Vector3 pos, Transform target, bool fade, float fadeAfter, bool afterSpawn, Creature targetCreature)
        {
            var portal = SpawnPortal(pos, target);
            portal.transform.localScale = Vector3.zero;
            while (portal.transform.localScale.x < 1)
            {
                portal.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
                yield return null;
            }
            portal.transform.localScale = Vector3.one;

            if (afterSpawn)
            {
                Debug.Log("running afterSpawn");
                var spawn = portal.transform.position;
                ItemData item = null;
                if(Daikokuten._instance.data.items.Count > 0) 
                    item = Daikokuten._instance.data.items.Pop();
                if (item != null)
                    item.SpawnAsync(_i =>
                    {
                        Debug.Log("Spawned portal item");
                        _i.transform.position = spawn;
                        _i.transform.LookAt(target);
                        var comp = _i.GetOrAddComponent<ItemHomingBehavior>();
                        comp.target = targetCreature;
                        comp.part = RagdollPart.Type.Torso;
                        _i.Throw();
                        _i.AddForce(_i.transform.forward * 10 * item.mass, ForceMode.Impulse);
                    });
                else
                    Debug.Log("No portal item");
            }

            if (fade)
                yield return DestroyPortalFade(portal, fadeAfter);
        }

        public static IEnumerator DestroyPortalFade(GameObject portal, float seconds)
        {
            yield return Yielders.ForSeconds(seconds);
            portal.transform.localScale = Vector3.one;
            while (portal.transform.localScale.x > 0)
            {
                portal.transform.localScale -= new Vector3(0.05f, 0.05f, 0.05f);
                yield return null;
            }

            portal.transform.localScale = Vector3.zero;

            GameObject.Destroy(portal);
        }
    }
}