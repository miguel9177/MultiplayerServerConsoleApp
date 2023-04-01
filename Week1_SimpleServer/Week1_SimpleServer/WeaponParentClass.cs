using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Week1_SimpleServer
{
    internal class WeaponParentClass
    {
        public float damage { get; protected set; }
        public float maxRange { get; protected set; }
        public string? weaponName { get; protected set; }
    }
}
