using System;
using System.Threading;
using MoreSlugcats;
using RWCustom;
using NCRcatsmod;
using UnityEngine;
using IL.Menu.Remix;

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
                    for (int i = 0; i < this.room.game.Players.Count; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            (this.room.game.Players[i].realizedCreature as Player).bodyChunks[j].HardSetPosition(this.room.MiddleOfTile(15, 68));
                        }
                        (this.room.game.Players[i].realizedCreature as Player).standing = false;
                        (this.room.game.Players[i].realizedCreature as Player).SetMalnourished(true);
                        (this.room.game.Players[i].realizedCreature as Player).flipDirection = 1;
                        (this.room.game.Players[i].realizedCreature as Player).sleepCounter = 99;
                        (this.room.game.Players[i].realizedCreature as Player).sleepCurlUp = 1f;
                    }
                    Awaketimer = 0;

                    CreaturesMade = true;
                    
                }
                if (!player.Sleeping)
                {
                    this.Awaketimer++;
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
