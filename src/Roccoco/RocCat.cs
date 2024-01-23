using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using NCRMarauder;


namespace NCRRoc.RocCat
{
    public static class NCRRocCat
    {
        public class RocCWT
        {
            // Define your variables to store here!
            public bool IsRocCat;

            public RocCWT()
            {
                // Initialize your variables here! (Anything not added here will be null or false or 0 (default values))
                this.IsRocCat = false;

            }
        }

        // This part lets you access the stored stuff by simply doing "self.GetCat()" in Plugin.cs or everywhere else!
        private static readonly ConditionalWeakTable<Player, RocCWT> CWT = new();
        public static RocCWT GetRocCat(this Player player) => CWT.GetValue(player, _ => new());
    }
}