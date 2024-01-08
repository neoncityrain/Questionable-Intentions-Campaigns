using System;
using System.Runtime.CompilerServices;


namespace Viviated.PartonCat
{
    public static class NCRPartonCWT
    {
        public class NCRPartonCat
        {
            public bool IsNCRPartonCat;

            public NCRPartonCat(){
                this.IsNCRPartonCat = false;
            }
        }

        private static readonly ConditionalWeakTable<Player, NCRPartonCat> CWT = new();
        public static NCRPartonCat GetParCat(this Player player) => CWT.GetValue(player, _ => new());
    }
}