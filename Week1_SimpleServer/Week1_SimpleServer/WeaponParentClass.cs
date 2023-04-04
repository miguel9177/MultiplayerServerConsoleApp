using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Week1_SimpleServer
{
    //this class is the parent to all weapons
    internal class WeaponParentClass
    {
        public float damage { get; protected set; }
        public float maxRange { get; protected set; }
        public string? weaponName { get; protected set; }
    }
}
