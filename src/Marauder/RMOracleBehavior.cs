using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;

namespace NCRMarauder.RMOracleBehavior
{
    internal class RMOracleBehavior
    {
        public static void Hook()
        {
            On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;
        }

        private static void PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, SSOracleBehavior.PebblesConversation self)
        {
            if (self.owner.oracle.room.game.StoryCharacter == null || self.owner.oracle.room.game.StoryCharacter.value != "NCRMarauder")
            {
                orig.Invoke(self);
            }
            else
            {
                
            }
        }
    }
}
