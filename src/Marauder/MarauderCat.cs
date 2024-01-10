using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using NCRMarauder;


namespace NCRMarauder.MarauderCat
{
    public static class NCRMarauderCat
    {
        public class MarauderCWT
        {
            // Define your variables to store here!
            public bool IsMarauder;
            public List<Creature> CreaturesYouPickedUp;

            public MarauderCWT()
            {
                // Initialize your variables here! (Anything not added here will be null or false or 0 (default values))
                this.IsMarauder = false;

            }
        }

        // This part lets you access the stored stuff by simply doing "self.GetCat()" in Plugin.cs or everywhere else!
        private static readonly ConditionalWeakTable<Player,MarauderCWT> CWT = new();
        public static MarauderCWT GetMarCat(this Player player) => CWT.GetValue(player, _ => new());
    }
}