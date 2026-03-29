using System;
using ThunderRoad;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KarmicVessel.ItemModules
{
    public class ItemHomingBehavior : MonoBehaviour
    {
        public Item item;
        public Creature target;
        public bool RandomTarget;
        public RagdollPart.Type part = RagdollPart.Type.Tail;
        public void Start()
        {
            item = GetComponent<Item>();
            item.mainCollisionHandler.OnCollidingEvent += colliding =>
            {
                Destroy(item.GetComponent<ItemHomingBehavior>());
            };
        }

        public void Update()
        {
            if (item == null)
                return;
            
            var position1 = item.transform.position;
            if(target == null)
                target = ThunderEntity.AimAssist(position1, item.Velocity.normalized, 20f, 30f, Filter.EnemyOf(Player.currentCreature)) as Creature;
            if (target != null)
            {
                var position2 = target.ragdoll.GetPart(RagdollPart.Type.Head).transform.position;
                if (RandomTarget || part == RagdollPart.Type.Tail)
                {
                    var creature = target;
                    var randomPart = Random.Range(0, 12);
                    position2 = creature.ragdoll.GetPart((RagdollPart.Type) Math.Pow(2, randomPart)).transform.position;
                }
                else
                {
                    position2 = target.ragdoll.GetPart(part).transform.position;
                }

                item.physicBody.velocity = (position2 - position1).normalized * item.Velocity.magnitude * 1.2f;
            }
        }
    }
}