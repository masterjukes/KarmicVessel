using System;
using UnityEngine;
using Object = System.Object;

namespace KarmicVessel.ItemModules
{
    public class CustomLoop : MonoBehaviour
    {
        public Action code;

        private void Update()
        {
            code?.Invoke();
        }
    }
}