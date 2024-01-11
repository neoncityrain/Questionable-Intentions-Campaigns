﻿using System;
using RWCustom;
using BepInEx;
using UnityEngine;
using NCREntropy.EntropyCat;
using NCRMarauder.MarauderCat;
using MoreSlugcats;
using NCREntropy.SB_L01ENT;
using Viviated.PartonCat;
using RegionKit;
using DressMySlugcat;
using System.Linq;
using System.Collections.Generic;
using NCRMarauder.OE_INTRO;

namespace NCRcatsmod
{
    [BepInPlugin(MOD_ID, "NCRCatsMod", "0.4.0")]
    class NCREntropy : BaseUnityPlugin
    {
        private const string MOD_ID = "neoncityrain.ncrcatsmod";
        FAtlas atlas;
        public bool IsDMSActive;
        public bool MarauderCannibalising;
        public bool MarauderKarmaCheck;
        public int MarauderStarvedForCycles;
        public int MarauderDidntCannibaliseCycles;

        public void OnEnable()
        {
            // ---------------------------------------------------- ALL CATS ----------------------------------------------------
            // initializing
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.Player.ctor += Player_ctor;

            // allow eating meat
            On.Player.CanEatMeat += Player_CanEatMeat;

            // jump codes
            On.Player.Jump += Player_Jump;

            // swim speed codes
            On.Player.UpdateAnimation += Player_UpdateAnimation;

            // draw graphics. DONT FORGET TO INITIALIZE THE SPRITES
            On.PlayerGraphics.DrawSprites += DrawSprites;
            On.RainWorld.OnModsInit += Init;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;

            // edits to gates
            On.RegionGate.customOEGateRequirements += RegionGate_customOEGateRequirements;
            On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;

            // ---------------------------------------------------- ENTROPY STUFF ----------------------------------------------------
            //entropy karma seizure. here for future parton usage as well
            On.KarmaFlower.BitByPlayer += KarmaFlower_BitByPlayer;

            // entropy shock collar code
            On.Creature.Grab += Creature_Grab;

            // ---------------------------------------------------- MARAUDER STUFF ----------------------------------------------------
            // marauder interacting with other slugcats
            On.Player.CanMaulCreature += Player_CanMaulCreature;
            On.Player.CanIPutDeadSlugOnBack += Player_CanIPutDeadSlugOnBack;
            On.Player.SlugcatGrab += Player_SlugcatGrab;

            // worm grass ignores marauder as long as marauder is alive
            On.WormGrass.WormGrassPatch.InteractWithCreature += WormGrassPatch_InteractWithCreature;

            // blue objects !!!!!!!
            On.SeedCob.ApplyPalette += SeedCob_ApplyPalette;
            On.Lantern.ApplyPalette += Lantern_ApplyPalette;
            On.Lantern.TerrainImpact += Lantern_TerrainImpact;
            On.Lantern.Update += Lantern_Update;
            On.FlyLure.ApplyPalette += FlyLure_ApplyPalette;

            // remove karma reinforcement and cannibalism buffs at the end of a cycle
            On.SaveState.SessionEnded += SaveState_SessionEnded;

            // checks if player ate a slugpup or player
            On.Player.EatMeatUpdate += Player_EatMeatUpdate;

            // cant throw spears when not starving or cannibalising
            On.Player.ThrownSpear += Player_ThrownSpear;


            // ---------------------------------------------------- VIVIATED STUFF ----------------------------------------------------
            //gross sounds when dying
            On.Player.Die += Player_Die;
        }

        private void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
        {
            orig(self, game, survived, newMalnourished);
            if (game.session.characterStats.name.value == "NCRMarauder")
            {
                for (int k = 0; k < game.GetStorySession.playerSessionRecords.Length; k++)
                {
                    for (int l = 0; l < game.world.GetAbstractRoom(game.Players[k].pos).creatures.Count; l++)
                    {

                        if (game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC
                            || game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].creatureTemplate.type == null)
                        {
                            if (!MarauderCannibalising)
                            {
                                self.forcePupsNextCycle = 1;
                            }
                        }
                        else
                        {
                            if (survived && newMalnourished)
                            {
                                MarauderStarvedForCycles++;
                            }
                            if (survived && !MarauderCannibalising)
                            {
                                MarauderDidntCannibaliseCycles++;
                                newMalnourished = true;
                            }
                            if (survived && !MarauderCannibalising && MarauderDidntCannibaliseCycles >= 4 &&
                                MarauderStarvedForCycles >= 2)
                            {
                                self.deathPersistentSaveData.karmaCap++;
                                MarauderStarvedForCycles = 0;
                                MarauderDidntCannibaliseCycles = 0;
                            }
                        }
                        if (MarauderCannibalising)
                        {
                            MarauderStarvedForCycles = 0;
                            MarauderDidntCannibaliseCycles = 0;
                            MarauderCannibalising = false;
                        }
                        //cannot keep reinforced karma
                        self.deathPersistentSaveData.reinforcedKarma = false;
                    }
                }
            }
        }

        private void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
        {
            orig(self);
            if (self.room.game.session.characterStats.name.value == ("NCRMarauder"))
            {
                if (self.room.abstractRoom.name == "GATE_SB_OE")
                {
                    int num;
                    if (int.TryParse(self.karmaRequirements[0].value, out num))
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.OneKarma;
                    }
                    int num2;
                    if (int.TryParse(self.karmaRequirements[1].value, out num2))
                    {
                        self.karmaRequirements[1] = RegionGate.GateRequirement.ThreeKarma;
                    }
                }
            }
            if (self.room.game.session.characterStats.name.value == ("NCREntropy"))
            {
                System.Random rd = new System.Random();
                int rand_num = rd.Next(1, 5);
                System.Random rd2 = new System.Random();
                int rand_num2 = rd2.Next(1, 5);
                int num;
                if (int.TryParse(self.karmaRequirements[0].value, out num))
                {
                    if (rand_num == 1)
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.OneKarma;
                    }
                    else if (rand_num == 2)
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.TwoKarma;
                    }
                    else if (rand_num == 3)
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.ThreeKarma;
                    }
                    else if (rand_num == 4)
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.FourKarma;
                    }
                    else
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.FiveKarma;
                    }
                }
                int num2;
                if (int.TryParse(self.karmaRequirements[1].value, out num2))
                {
                    if (rand_num2 == 1)
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.OneKarma;
                    }
                    else if (rand_num2 == 2)
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.TwoKarma;
                    }
                    else if (rand_num2 == 3)
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.ThreeKarma;
                    }
                    else if (rand_num2 == 4)
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.FourKarma;
                    }
                    else
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.FiveKarma;
                    }
                }
                Debug.Log("Entropy's gate requirements randomized!");
            }
        }

        private void Player_EatMeatUpdate(On.Player.orig_EatMeatUpdate orig, Player self, int graspIndex) 
        { 
            orig(self, graspIndex);
            if (self.grasps[graspIndex].grabbed is Player && self.GetMarCat().IsMarauder && self.eatMeat > 20) 
            { 
                MarauderKarmaCheck = true;
                if (MarauderKarmaCheck && !MarauderCannibalising) 
                { 
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma++; 
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap--;

                    if ((self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap < 0) 
                    { 
                        (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 0; 
                    } 
                    if ((self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap > 9) 
                    { 
                        (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = 9;
                    } 
                    
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma = true; 
                    for (int i = 0; i < self.room.game.cameras.Length; i++) 
                    { 
                        self.room.game.cameras[i].hud.karmaMeter.reinforceAnimation = 0; 
                    } 

                    Debug.Log("MARAUDER FUCKED UP AND EVIL MOMENTS!!!!!!!!");
                    
                    MarauderKarmaCheck = false; 
                    MarauderCannibalising = true; 
                } 
            } 
        }

        private bool RegionGate_customOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
        {
            orig(self);
            bool flag = self.room.game.rainWorld.progression.miscProgressionData.beaten_Gourmand ||
                self.room.game.rainWorld.progression.miscProgressionData.beaten_Gourmand_Full;
            if (!(self.room.game.session is StoryGameSession) && self.room.game.session.characterStats.name.value != "NCRMarauder" 
                && self.room.game.session.characterStats.name.value != ("NCREntropy"))
            {
                return false;
            }
            if ((self.room.game.session as StoryGameSession).saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                return (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark || flag;
            }
            return (((self.room.game.session as StoryGameSession).saveStateNumber == SlugcatStats.Name.White ||
                (self.room.game.session as StoryGameSession).saveStateNumber == SlugcatStats.Name.Yellow) && flag) ||
                self.room.game.session.characterStats.name.value == ("NCRMarauder") ||
                self.room.game.session.characterStats.name.value == ("NCREntropy");
        }

        private void FlyLure_ApplyPalette(On.FlyLure.orig_ApplyPalette orig, FlyLure self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.room.game.session.characterStats.name.value == "NCRMarauder")
            {
                self.color = UnityEngine.Color.Lerp(new UnityEngine.Color(0.6f, 0.8f, 1f), palette.fogColor, 0.3f);
                self.UpdateColor(sLeaser, false);
            }
        }

        private void Player_ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
        {
            orig(self, spear);
            if (self.GetMarCat().IsMarauder)
            {
                if (MarauderCannibalising)
                {
                    BodyChunk firstChunk = spear.firstChunk;
                    firstChunk.vel.x = firstChunk.vel.x * 1.3f;
                    spear.spearDamageBonus = 2.5f;
                    spear.room.AddObject(new Spark(spear.thrownPos, firstChunk.vel, UnityEngine.Color.Lerp(new UnityEngine.Color(1f, 0.2f, 0f), new UnityEngine.Color(1f, 1f, 1f), UnityEngine.Random.value * 0.5f), null, 19, 47));
                    spear.room.AddObject(new Spark(spear.thrownPos, firstChunk.vel, UnityEngine.Color.Lerp(new UnityEngine.Color(1f, 0.2f, 0f), new UnityEngine.Color(1f, 1f, 1f), UnityEngine.Random.value * 0.5f), null, 19, 47));
                    spear.room.AddObject(new Spark(spear.thrownPos, firstChunk.vel, UnityEngine.Color.Lerp(new UnityEngine.Color(1f, 0.2f, 0f), new UnityEngine.Color(1f, 1f, 1f), UnityEngine.Random.value * 0.5f), null, 19, 47));
                    Debug.Log("Marauder spear thrown after cannibalising");
                }
                else if (self.Malnourished)
                {
                    spear.spearDamageBonus = 2.5f;
                    Debug.Log("Marauder spear thrown while malnourished");
                }
                else
                {
                    //im weeping. it looks so stupid. its beautiful
                    spear.throwModeFrames = 2;
                    spear.spearDamageBonus = 0.2f;
                    Debug.Log("Marauder spear thrown while NOT malnourished");
                }
            }
        }

        private void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
        {
            orig(self, obj, graspUsed);
            if (self.GetMarCat().IsMarauder && obj is Player)
            {
                (obj as Player).dangerGrasp = self.grasps[graspUsed];
                (obj as Player).dangerGraspTime = 0;
            }
        }

        private void Lantern_Update(On.Lantern.orig_Update orig, Lantern self, bool eu)
        {
            orig(self, eu);
            if (self.room.game.session.characterStats.name.value == "NCRMarauder" && self.lightSource == null)
            {
                self.lightSource = new LightSource(self.firstChunk.pos, false, new UnityEngine.Color(0.5f, 0.8f, 0.9f), self);
                self.room.AddObject(self.lightSource);
            }

        }

        private void Lantern_TerrainImpact(On.Lantern.orig_TerrainImpact orig, Lantern self, int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            orig(self, chunk, direction, speed, firstContact);
            // if the world belongs to marauder, lanterns will have blue sparks when hitting things. small change but important to me
            // unsure how to remove the red ones, though...
            if (speed > 5f && firstContact && self.room.game.session.characterStats.name.value == "NCRMarauder")
            {
                Vector2 pos = self.bodyChunks[chunk].pos + direction.ToVector2() * self.bodyChunks[chunk].rad * 0.9f;
                int num = 0;
                while ((float)num < Mathf.Round(Custom.LerpMap(speed, 5f, 15f, 2f, 8f)))
                {
                    self.room.AddObject(new Spark(pos, direction.ToVector2() * Custom.LerpMap(speed, 5f, 15f, -2f, -8f) + Custom.RNV() * UnityEngine.Random.value * Custom.LerpMap(speed, 5f, 15f, 2f, 4f), UnityEngine.Color.Lerp(new UnityEngine.Color(0f, 0.8f, 0.9f), new UnityEngine.Color(1f, 1f, 1f), UnityEngine.Random.value * 0.5f), null, 19, 47));
                    num++;
                }
            }
        }

        private void Lantern_ApplyPalette(On.Lantern.orig_ApplyPalette orig, Lantern self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.room.game.session.characterStats.name.value == "NCRMarauder")
            {
                sLeaser.sprites[0].color = new UnityEngine.Color(0.5f, 0.8f, 0.9f);
                sLeaser.sprites[1].color = new UnityEngine.Color(1f, 1f, 1f);
                sLeaser.sprites[2].color = UnityEngine.Color.Lerp(new UnityEngine.Color(0.5f, 0.8f, 0.9f), new UnityEngine.Color(1f, 1f, 1f), 0.3f);
                sLeaser.sprites[3].color = new UnityEngine.Color(0.6f, 0.9f, 0.9f);
                if (self.stick != null)
                {
                    sLeaser.sprites[4].color = palette.blackColor;
                }
            }
        }

        private void SeedCob_ApplyPalette(On.SeedCob.orig_ApplyPalette orig, SeedCob self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.room.game.session.characterStats.name.value == "NCRMarauder")
            {
                sLeaser.sprites[self.StalkSprite(0)].color = palette.blackColor;
                self.StoredBlackColor = palette.blackColor;
                UnityEngine.Color pixel = palette.texture.GetPixel(0, 5);
                self.StoredPlantColor = pixel;
                for (int i = 0; i < (sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors.Length; i++)
                {
                    float num = (float)i / (float)((sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors.Length - 1);
                    (sLeaser.sprites[self.StalkSprite(1)] as TriangleMesh).verticeColors[i] = UnityEngine.Color.Lerp(palette.blackColor, pixel, 0.4f + Mathf.Pow(1f - num, 0.5f) * 0.4f);
                }
                self.yellowColor = UnityEngine.Color.Lerp(new UnityEngine.Color(0.5f, 0.83f, 0.9f), palette.blackColor, self.AbstractCob.dead ? (0.95f + 0.5f * rCam.PaletteDarkness()) : (0.18f + 0.7f * rCam.PaletteDarkness()));
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < (sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors.Length; k++)
                    {
                        float num2 = 1f - (float)k / (float)((sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors.Length - 1);
                        (sLeaser.sprites[self.ShellSprite(j)] as TriangleMesh).verticeColors[k] = UnityEngine.Color.Lerp(palette.blackColor, new UnityEngine.Color(0f, 0.6f, 1f), Mathf.Pow(num2, 2.5f) * 0.4f);
                    }
                }
                sLeaser.sprites[self.CobSprite].color = self.yellowColor;
                UnityEngine.Color color = self.yellowColor + new UnityEngine.Color(0.2f, 0.3f, 0.3f) * Mathf.Lerp(1f, 0.15f, rCam.PaletteDarkness());
                if (self.AbstractCob.dead)
                {
                    color = UnityEngine.Color.Lerp(self.yellowColor, pixel, 0.75f);
                }
                for (int l = 0; l < self.seedPositions.Length; l++)
                {
                    sLeaser.sprites[self.SeedSprite(l, 0)].color = self.yellowColor;
                    sLeaser.sprites[self.SeedSprite(l, 1)].color = color;
                    sLeaser.sprites[self.SeedSprite(l, 2)].color = UnityEngine.Color.Lerp(new UnityEngine.Color(0f, 0f, 1f), palette.blackColor, self.AbstractCob.dead ? 0.6f : 0.3f);
                }
                for (int m = 0; m < self.leaves.GetLength(0); m++)
                {
                    sLeaser.sprites[self.LeafSprite(m)].color = palette.blackColor;
                }
            }
        }

        private void WormGrassPatch_InteractWithCreature(On.WormGrass.WormGrassPatch.orig_InteractWithCreature orig, WormGrass.WormGrassPatch self, WormGrass.WormGrassPatch.CreatureAndPull creatureAndPull)
        {
            orig(self, creatureAndPull);
            if (creatureAndPull.creature is Player && (creatureAndPull.creature as Player).GetMarCat().IsMarauder && !creatureAndPull.creature.dead)
            {
                // worm grass should never be able to fully consume marauder as long as theyre alive
                self.LoseGrip(creatureAndPull);
                creatureAndPull.consumeTimer = 1;
                // doesnt track marauder and instantly removes them from the list of tracked creatures
                self.trackedCreatures.Remove(creatureAndPull);
                return;
            }
        }

        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                bool flag = ModManager.ActiveMods.Any((ModManager.Mod mod) => mod.id == "dressmyslugcat");
                if (flag)
                {
                    Debug.Log("Neoncats DMS loaded!");
                    IsDMSActive = true;
                    this.SetupDMSSprites();
                }
            }
            catch (Exception ex)
            {
               Debug.LogException(ex);
                Debug.Log("Something went wrong with NCR's QI mod! You will run into errors.");
            }
            finally
            {
                orig.Invoke(self);
            }
        }

        public void SetupDMSSprites()
        {
            for (int index = 0; index < 4; index++)
            {
                string EntropySheet = "neoncityrain.ncrentropydms";
                SpriteDefinitions.AddSlugcatDefault(new Customization
                {
                    Slugcat = "NCREntropy",
                    PlayerNumber = index,
                    CustomSprites = new List<CustomSprite>{
                        new CustomSprite{Sprite = "TAIL",SpriteSheetID = EntropySheet},
                        new CustomSprite{Sprite = "LEGS",SpriteSheetID = EntropySheet},
                        new CustomSprite{Sprite = "ARMS",SpriteSheetID = EntropySheet},
                        new CustomSprite{Sprite = "HIPS",SpriteSheetID = EntropySheet},
                        new CustomSprite{Sprite = "BODY",SpriteSheetID = EntropySheet},
                        new CustomSprite{Sprite = "HEAD",SpriteSheetID = EntropySheet},
                        new CustomSprite{Sprite = "EYES",SpriteSheetID = EntropySheet}
                    },
                    CustomTail = new CustomTail { Length = 4f }
                });
                string MarauderSheet = "neoncityrain.ncrmarauderdms";
                SpriteDefinitions.AddSlugcatDefault(new Customization
                {
                    Slugcat = "NCRMarauder",
                    PlayerNumber = index,
                    CustomSprites = new List<CustomSprite>{
                        new CustomSprite{Sprite = "TAIL",SpriteSheetID = MarauderSheet, ColorHex = "#ffffff"},
                        new CustomSprite{Sprite = "LEGS",SpriteSheetID = MarauderSheet, ColorHex = "#ffffff"},
                        new CustomSprite{Sprite = "ARMS",SpriteSheetID = MarauderSheet, ColorHex = "#ffffff"},
                        new CustomSprite{Sprite = "HIPS",SpriteSheetID = MarauderSheet, ColorHex = "#ffffff"},
                        new CustomSprite{Sprite = "BODY",SpriteSheetID = MarauderSheet, ColorHex = "#ffffff"},
                        new CustomSprite{Sprite = "FACE",SpriteSheetID = MarauderSheet,ColorHex = "#ffffff"},
                        new CustomSprite{Sprite = "HEAD",SpriteSheetID = MarauderSheet,ColorHex = "#ffffff"}
                    },
                    CustomTail = new CustomTail { Wideness = 1.5f, AsymTail = true, Roundness = 0.1f }
                });
                Debug.Log("Questionable Intention's default DMS sprites set!");
            }
        }

        private void Player_Die(On.Player.orig_Die orig, Player self)
        {
            orig(self);
            if (self.GetParCat().IsNCRPartonCat && self.dead)
            {
                // the below gets the main body colour, possibly to be used for corruption bubble graphics if i can get those to work
                // var color = self.ShortCutColor();

                self.room.PlaySound(SoundID.Daddy_Digestion_Init, self.mainBodyChunk.pos);
                self.room.InGameNoise(new Noise.InGameNoise(self.mainBodyChunk.pos, 9999f, self, 1f));
            }
        }

        private bool Player_CanIPutDeadSlugOnBack(On.Player.orig_CanIPutDeadSlugOnBack orig, Player self, Player pickUpCandidate)
        {
            orig(self, pickUpCandidate);
            if (self.GetMarCat().IsMarauder)
            {
                return false;
            }
            else return ModManager.CoopAvailable && pickUpCandidate != null && (!ModManager.MSC ||
                    !(pickUpCandidate.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup));
        }

        private bool Player_CanMaulCreature(On.Player.orig_CanMaulCreature orig, Player self, Creature crit)
        {
            orig(self, crit);
            bool flag = true;
            if (ModManager.CoopAvailable)
                {
                    Player player = crit as Player;
                    if (player != null && (player.isNPC || !Custom.rainWorld.options.friendlyFire || !self.GetMarCat().IsMarauder))
                    {
                        flag = false;
                    }
                }
            return !(crit is Fly) && !crit.dead && (!(crit is IPlayerEdible) || (crit is Centipede && !(crit as Centipede).Edible) ||
                    self.FoodInStomach >= self.MaxFoodInStomach) && flag && (crit.Stunned || (!(crit is Cicada) && (!(crit is Player) ||
                    self.GetMarCat().IsMarauder) && self.IsCreatureLegalToHoldWithoutStun(crit))) && SlugcatStats.SlugcatCanMaul(self.SlugCatClass);
        }

        private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            // ---------------------------------------------------- ENTROPY STUFF ----------------------------------------------------
            if (self.slugcatStats.name.value == "NCREntropy")
            {
                self.GetEntCat().IsEntropy = true;
                if (self.GetEntCat().IsFree == false)
                {
                    self.GetEntCat().CollarShocks = true;
                }

                if (self.KarmaCap < 8)
                {
                    self.playerState.isPup = true;
                }
                else
                {
                    self.playerState.isPup = false;
                };

                if (self.room.game.session is StoryGameSession)
                {
                    string name = self.room.abstractRoom.name;
                    if (name == "SB_L01")
                    {
                        self.room.AddObject(new EntropyIntro(self.room));
                    }
                }
            }
            // ---------------------------------------------------- MARAUDER STUFF ----------------------------------------------------
            if (self.slugcatStats.name.value == "NCRMarauder")
            {
                self.GetMarCat().IsMarauder = true;
                // freezes to death a little slower
                self.HypothermiaExposure -= 0.05f;

                if (self.room.game.session is StoryGameSession)
                {
                    string name = self.room.abstractRoom.name;
                    if (name == "OE_RUINCourtYard")
                    {
                        self.room.AddObject(new MarauderIntro(self.room));
                    }

                }
                if (self.dead)
                {
                    MarauderCannibalising = false;
                    MarauderKarmaCheck = false;
                }
            }
            // ---------------------------------------------------- VIVIATED STUFF ----------------------------------------------------
            if (self.slugcatStats.name.value == "NCRParton")
            {
                self.GetParCat().IsNCRPartonCat = true;
            }
        }

        private bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
        {
            orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
            if (self is Creature && (obj is Player player && player.GetEntCat().IsEntropy && !player.GetEntCat().IsFree && player.GetEntCat().CollarShocks == true)
                && !(self is Player) && !(self is Leech) && !(self is Centipede) && !(self is Spider) && !(self is Cicada) && !(self is JetFish))
            {
                try
                {
                    //please stop teleporting lizards. should make both parties drop everything
                    self.grasps[graspUsed] = new Creature.Grasp(self, obj, graspUsed, chunkGrabbed, shareability, dominance, pacifying);
                    self.ReleaseGrasp(graspUsed);
                    (obj as Player).LoseAllGrasps();
                    (obj as Player).dangerGrasp = null;

                    //seizure shock code
                    self.room.AddObject(new CreatureSpasmer(self, true, 100));
                    self.room.AddObject(new CreatureSpasmer((obj as Player), true, 80));
                    self.Stun(100);
                    (obj as Player).Stun(80);
                    if ((obj as Player).Submersion > 0f)
                    {
                        self.room.AddObject(new UnderwaterShock(self.room, null, (obj as Player).mainBodyChunk.pos, 14, Mathf.Lerp(ModManager.MMF ? 0f : 200f, 1200f, 2f), 0.2f + 1.9f * 2f, (obj as Player), new UnityEngine.Color(0.7f, 0.7f, 1f)));
                    }

                    //still plays bite sfx
                    if (self is Lizard)
                    {
                        self.room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, self.mainBodyChunk);
                    }

                    //only happens once
                    (obj as Player).GetEntCat().CollarShocks = false;

                    //visual effects
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV(), UnityEngine.Color.white, null, 4, 8));
                    (obj as Player).room.AddObject(new Spark((obj as Player).mainBodyChunk.pos, Custom.RNV(), UnityEngine.Color.white, null, 4, 8));
                    (obj as Player).room.AddObject(new Spark((obj as Player).mainBodyChunk.pos, Custom.RNV(), UnityEngine.Color.white, null, 4, 8));
                    (obj as Player).room.PlaySound(SoundID.Centipede_Shock, (obj as Player).mainBodyChunk.pos);
                    return false;


                }
                catch (Exception ex)
                {
                    string str = "Shock Collar Error! ";
                    Debug.Log(str + ((ex != null) ? ex.ToString() : null));
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);
            //marader has a larger jump boost when hungry
            if (self.GetMarCat().IsMarauder)
            {
                if (MarauderCannibalising || self.Malnourished)
                {
                    self.jumpBoost += 4f;
                }
                else if (self.playerState.foodInStomach > 1 && !self.Malnourished)
                { self.jumpBoost += 1.5f; }
                else
                { self.jumpBoost += 3f; }
            }
            //viviated just jumps a lil higher
            if (self.GetParCat().IsNCRPartonCat)
            {
                self.jumpBoost += 1f;
            }
            //entropy has a jump height that is semi-randomly determined
            if (self.GetEntCat().IsEntropy)
            {
                System.Random rd = new System.Random();
                int rand_num = rd.Next(1, 7);

                if (self.GetEntCat().HowManyJumps < 5)
                {
                    self.jumpBoost += 2f;
                    self.GetEntCat().HowManyJumps += rand_num;
                }
                else if (self.GetEntCat().HowManyJumps <= 8 && self.GetEntCat().HowManyJumps > 5)
                {
                    self.jumpBoost += 4f;
                    self.GetEntCat().HowManyJumps = 0;
                }
                else if (self.GetEntCat().HowManyJumps > 8 || self.GetEntCat().HowManyJumps == 5)
                {
                    self.jumpBoost -= 1f;
                    self.GetEntCat().HowManyJumps = 0;
                }
                else
                {
                    self.jumpBoost += 99f;
                    Debug.Log("Entropy jump error! Entropy launched into stratosphere. Please report this. Jump number: " + self.GetEntCat().HowManyJumps);
                    self.GetEntCat().HowManyJumps = 0;
                }
            }
        }

        private bool Player_CanEatMeat(On.Player.orig_CanEatMeat orig, Player self, Creature crit)
        {
            orig(self, crit);
            // entropy can eat meat if an adult
            if (self.GetEntCat().IsEntropy && self.KarmaCap > 7 && (!ModManager.CoopAvailable || !(crit is Player)))
            {
                return true;
            }
            // marauder can eat players
            if (self.GetMarCat().IsMarauder)
            {
                return true;
            }
            if (ModManager.MSC && (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint || self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear))
            {
                return false;
            }
            if (self.EatMeatOmnivoreGreenList(crit) && crit.dead)
            {
                return !ModManager.MSC || self.pyroJumpCooldown <= 60f;
            }
            return !(crit is IPlayerEdible) && crit.dead && (self.slugcatStats.name == SlugcatStats.Name.Red || (ModManager.MSC &&
                (self.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer || 
                self.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand || self.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel))) 
                && (!ModManager.CoopAvailable || !(crit is Player)) && (!ModManager.MSC || self.pyroJumpCooldown <= 60f);
        }

        private void KarmaFlower_BitByPlayer(On.KarmaFlower.orig_BitByPlayer orig, KarmaFlower self, Creature.Grasp grasp, bool eu)
        {
            orig(self, grasp, eu);
            // entropys flower oddity
            if ((grasp.grabber as Player).GetEntCat().IsEntropy && self.bites < 1)
            {
                grasp.grabber.LoseAllGrasps();

                if ((grasp.grabber as Player).GetEntCat().CollarShocks == true)
                {
                    (grasp.grabber as Player).GetEntCat().CollarShocks = false;
                    self.room.AddObject(new CreatureSpasmer(grasp.grabber, true, 90));
                    grasp.grabber.Stun(110);
                }
                else
                {
                    (grasp.grabber as Player).GetEntCat().CollarShocks = true;
                    self.room.AddObject(new CreatureSpasmer(grasp.grabber, true, 120));
                    grasp.grabber.Stun(150);
                    (grasp.grabber as Player).airInLungs *= 0.1f;
                    (grasp.grabber as Player).exhausted = true;

                }

                //no reinforced karma we die like men
                ((grasp.grabber as Player).room.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma = false;

                //visual effects
                grasp.grabber.room.AddObject(new Spark(grasp.grabber.mainBodyChunk.pos, Custom.RNV(), UnityEngine.Color.white, null, 4, 8));
                grasp.grabber.room.AddObject(new Spark(grasp.grabber.mainBodyChunk.pos, Custom.RNV(), UnityEngine.Color.white, null, 4, 8));
                grasp.grabber.room.PlaySound(SoundID.Centipede_Shock, grasp.grabber.mainBodyChunk.pos);
            }
        }

        private void Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self)
        {
            orig(self);
            if (self.GetEntCat().IsEntropy)
            {
                self.waterFriction = 0.97f;
            }
            if (self.GetParCat().IsNCRPartonCat)
            {
                self.waterFriction = 0.95f;
            }
            if (self.GetMarCat().IsMarauder)
            {
                if (self.playerState.foodInStomach > 1)
                {
                    self.waterFriction = 0.965f;
                }
                else
                {
                    self.waterFriction = 0.97f;
                }
            }
        }

        private void DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (IsDMSActive == false)
            {
                
                //0-body, 1-hips, 2-tail, 3-head, 4-legs, 5-left arm, 6-right arm, 7-left hand, 8-right hand, 9-face, 10-glow, 11-pixel/mark
                if (self.player.GetEntCat().IsEntropy && self.player.GetEntCat().IsFree == false)
                {
                    string name = sLeaser.sprites[3]?.element?.name; //head
                                                                     // uses different sprites for baby vs adult
                    if (self.player.KarmaCap < 8 && name != null && name.StartsWith("HeadC") && atlas._elementsByName.TryGetValue("ent" + name, out var babyhead))
                    {
                        sLeaser.sprites[3].element = babyhead;
                    }
                    if (self.player.KarmaCap >= 8 && name != null && name.StartsWith("HeadA") && atlas._elementsByName.TryGetValue("adultent" + name, out var adulthead))
                    {
                        sLeaser.sprites[3].element = adulthead;
                    }
                }

            }
            else
            {

                
                
            }
        }

        private void Init(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            atlas ??= Futile.atlasManager.LoadAtlas("atlases/enthead");
            atlas ??= Futile.atlasManager.LoadAtlas("atlases/adultenthead");
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
    }
}