using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Week1_SimpleServer
{
    internal static class WeaponManager
    {
        public static WeaponParentClass? GetWeapon(string weaponName)
        {
            if (weaponName.Contains("Raycast Weapon"))
                return new RaycastWeapon();

            return null;
        }
    }
}
