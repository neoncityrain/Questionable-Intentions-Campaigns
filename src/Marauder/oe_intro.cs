using System;
using System.Threading;
using MoreSlugcats;
using RWCustom;
using NCRcatsmod;
using UnityEngine;

namespace NCRMarauder.OE_INTRO
{
    public class MarauderIntro : UpdatableAndDeletable
    {
        bool CreaturesMade;
        int Awaketimer;
        
        public MarauderIntro(Room room)
        {
            this.room = room;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room.game.AllPlayersRealized)
            {
                if (this.room.game.cameras[0].hud != null && this.room.game.cameras[0].hud.textPrompt != null && this.room.game.cameras[0].hud.textPrompt.subregionTracker != null)
                {
                    this.room.game.cameras[0].hud.textPrompt.subregionTracker.showCycleNumber = false;
                    this.room.game.cameras[0].hud.textPrompt.subregionTracker.lastRegion = 1;
                    this.room.game.cameras[0].hud.textPrompt.subregionTracker.lastShownRegion = 1;
                }

                AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
                Player player = firstAlivePlayer.realizedCreature as Player;
                if (CreaturesMade == false)
                {
                    player.SuperHardSetPosition(room.MiddleOfTile(15, 68));
                    player.standing = false;
                    player.SetMalnourished(true);
                    player.flipDirection = 1;
                    player.sleepCounter = 99;
                    player.sleepCurlUp = 1f;
                    player.Hypothermia = 0f;
                    Awaketimer = 0;
                    this.room.world.rainCycle.timer = this.room.world.rainCycle.cycleLength - 200;

                    CreaturesMade = true;
                    
                }
                if (!player.Sleeping)
                {
                    this.Awaketimer++;
                    player.Hypothermia = 0f;
                }

                if (this.Awaketimer == 150)
                {
                    this.room.game.cameras[0].hud.textPrompt.AddMessage(this.room.game.rainWorld.inGameTranslator.Translate("You are starving. You will be faster and stronger than normal, but take care not to starve to death."), 20, 500, true, true);
                    Destroy();
                }


                
            }
            else
            {
                Debug.Log("Error in Marauder Intro!");
            }
        }

    }
}
