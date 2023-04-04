using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Week1_SimpleServer
{
    internal static class WeaponManager
    {
        //this gets the requested weapon by name
        public static WeaponParentClass? GetWeapon(string weaponName)
        {
            //if the weapon was a raycast weapon, we return a new raycast weapon class
            if (weaponName.Contains("Raycast Weapon"))
                return new RaycastWeapon();

            return null;
        }
    }
}
