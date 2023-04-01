using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Week1_SimpleServer
{
    internal class RaycastWeapon : WeaponParentClass
    {
        public RaycastWeapon()
        {
            damage = 25;
            maxRange = 50f;
            weaponName = "Raycast Weapon";
        }
    }
}
