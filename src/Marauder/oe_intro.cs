using System;
using System.Threading;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace NCRMarauder.OE_INTRO
{
    public class MarauderIntro : UpdatableAndDeletable
    {
        bool CreaturesMade;
        bool TimerSet;
        int Timer;
        
        public MarauderIntro(Room room)
        {
            this.room = room;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room.game.AllPlayersRealized)
            {
                if (TimerSet == false)
                {
                    Timer = 0;
                    TimerSet = true;
                }

                AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
                Player player = firstAlivePlayer.realizedCreature as Player;
                if (CreaturesMade == false)
                {
                    player.SuperHardSetPosition(room.MiddleOfTile(7, 38));

                    AbstractCreature abstractCreature = new AbstractCreature(this.room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, this.room.ToWorldCoordinate(new Vector2(135f, 773f)), this.room.game.GetNewID());
                    if (!this.room.world.game.rainWorld.setup.forcePup)
                    {
                        (abstractCreature.state as PlayerState).forceFullGrown = true;
                    }
                    this.room.abstractRoom.AddEntity(abstractCreature);
                    abstractCreature.Die();
                    abstractCreature.RealizeInRoom();

                    player.standing = true;
                    player.SlugcatGrab(abstractCreature.realizedCreature, 0);
                    CreaturesMade = true;
                }

                if (Timer < 220)
                {
                    Timer += 1;

                }
                if (Timer >= 220)
                {
                    Destroy();
                }

            }
            else
            {
                Debug.Log("Player not realized");
            }
        }

    }
}
