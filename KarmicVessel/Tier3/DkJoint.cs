using System;
using KarmicVessel.Other;
using ThunderRoad;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KarmicVessel.Tier3
{
    public class DkJoint : MonoBehaviour
    {
        private Item item;
        public Transform target;
        SpellCaster caster;

        public float strength = 40f;
        public float damping = 10f;
        public float maxForce = 50f;
        public float rotationSpeed = 20f;

        private LineRenderer lr;

        public void Init(Item item, Transform target, SpellCaster caster) 
        {
            this.target = target;
            this.item = item;
            this.caster = caster;
            lr = dbg.DrawLine(transform.position, target.position, 0.01f, Color.red, despawnTime: 20f);
            item.physicBody.rigidBody.useGravity = false;

        }

        private void OnDestroy()
        {
            Destroy(lr);
            item.physicBody.rigidBody.useGravity = true;

        }

        private void FixedUpdate()
        {
            if (!item || !target) return;

            Vector3 toTarget = target.position - transform.position;
            float distance = toTarget.magnitude;

            if (distance < 0.001f) return;

            Vector3 dir = toTarget / distance;

            Vector3 velocity = item.physicBody.velocity;
            Vector3 force = (toTarget * strength) - (velocity * damping);

            force = Vector3.ClampMagnitude(force, maxForce);

            item.AddForce(force, ForceMode.Acceleration);

            Quaternion targetRot = Quaternion.LookRotation(caster.ragdollHand.PalmDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
            if (lr != null)
            {
                lr.SetPosition(0, transform.position);
                lr.SetPosition(1, target.position);
            }
        }
    }
}