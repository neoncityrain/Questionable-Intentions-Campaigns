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

                for (int i = 0; i < this.room.game.Players.Count; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        (this.room.game.Players[i].realizedCreature as Player).bodyChunks[j].HardSetPosition(this.room.MiddleOfTile(202, 87));
                    }
                        (this.room.game.Players[i].realizedCreature as Player).standing = false;
                    (this.room.game.Players[i].realizedCreature as Player).flipDirection = 1;
                    (this.room.game.Players[i].realizedCreature as Player).sleepCounter = 99;
                    (this.room.game.Players[i].realizedCreature as Player).sleepCurlUp = 1f;
                }

                Destroy();
            }
            else
            {
                Debug.Log("Player not realized");
            }
        }
    }
}
