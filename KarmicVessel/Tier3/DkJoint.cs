using System;
using ThunderRoad;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KarmicVessel.Tier3
{
    public class DkJoint : MonoBehaviour
    {
        private ConfigurableJoint joint;
        private Rigidbody anchor;
        
        public Transform target;

        public void Init(Item item, Transform target)
        {
            this.target = target;
            SetupAnchor();
            RefreshJoint(item);
        }
        
        private void OnDestroy()
        {
            if (anchor != null)
                Object.Destroy(anchor);
            if(joint != null)
                Destroy(joint);
        }
        
        public void RefreshJoint(Item item)
        {
            if (anchor != null)
            {
                if (joint == null)
                    joint = item.gameObject.AddComponent<ConfigurableJoint>();
                

                JointDrive drive = new JointDrive
                {
                    positionSpring = 100f, 
                    positionDamper = 300f,  
                    maximumForce = float.MaxValue
                };

                joint.xDrive = drive;
                joint.yDrive = drive;
                joint.zDrive = drive;

                joint.slerpDrive = new JointDrive
                {
                    positionSpring = 100f,
                    positionDamper = 300f,
                    maximumForce = float.MaxValue
                };

                joint.massScale = 1f;
                joint.connectedMassScale = 1000f;

                joint.rotationDriveMode = RotationDriveMode.Slerp;
                
                joint.xMotion = ConfigurableJointMotion.Free;
                joint.yMotion = ConfigurableJointMotion.Free;
                joint.zMotion = ConfigurableJointMotion.Free;

                joint.angularXMotion = ConfigurableJointMotion.Free;
                joint.angularYMotion = ConfigurableJointMotion.Free;
                joint.angularZMotion = ConfigurableJointMotion.Free;

                
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = item.transform.InverseTransformPoint(target.position);
                item.physicBody.rigidBody.inertiaTensor = new Vector3(1, 1, 1);
                item.physicBody.rigidBody.inertiaTensorRotation = Quaternion.identity;
                joint.connectedBody = anchor;
            }
            else
            {
                if (!(joint != null))
                    return;
                Object.Destroy(joint);
                joint = null;
            }
        }

        private void Update()
        {
            if (anchor != null)
                anchor.MovePosition(target.position);

        }

        public void SetupAnchor()
        {
            anchor = new GameObject("DKAnchor").AddComponent<Rigidbody>();
            anchor.transform.position = target.position;
            anchor.isKinematic = true;
            anchor.useGravity = false;
            anchor.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }
}