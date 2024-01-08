using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using NCREntropy;


namespace NCREntropy.EntropyCat
{
    public static class EntropyCat
    {
        public class NCREntropy
        {
            // Define your variables to store here!
            public bool IsEntropy;
            public int HowManyJumps;
            public bool CollarShocks;
            public bool IsFree;

            public NCREntropy()
            {
                // Initialize your variables here! (Anything not added here will be null or false or 0 (default values))
                IsEntropy = false;
                CollarShocks = false;
                IsFree = false;
            }
        }

        // This part lets you access the stored stuff by simply doing "self.GetCat()" in Plugin.cs or everywhere else!
        private static readonly ConditionalWeakTable<Player, NCREntropy> CWT = new();
        public static NCREntropy GetEntCat(this Player player) => CWT.GetValue(player, _ => new());
    }
}