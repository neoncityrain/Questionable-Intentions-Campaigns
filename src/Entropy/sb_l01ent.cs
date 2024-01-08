using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace NCREntropy.SB_L01ENT
{
    public class EntropyIntro : UpdatableAndDeletable
    {
        public EntropyIntro(Room room)
        {
            this.room = room;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room.game.AllPlayersRealized)
            {
                AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
                Player player = firstAlivePlayer.realizedCreature as Player;
                player.SuperHardSetPosition(room.MiddleOfTile(202, 87));

                player.standing = false;
                player.flipDirection = 1;
                player.sleepCounter = 99;
                player.sleepCurlUp = 1f;



                Destroy();
            }
            else
            {
                Debug.Log("Player not realized");
            }
        }
    }
}
