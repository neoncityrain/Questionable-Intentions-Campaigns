using System;
using RWCustom;
using BepInEx;
using UnityEngine;
using NCREntropy.EntropyCat;
using NCRMarauder.MarauderCat;
using MoreSlugcats;
using LizardCosmetics;
using NCREntropy.SB_L01ENT;
using Viviated.PartonCat;
using Expedition;
using On.Expedition;
using DressMySlugcat;
using System.Linq;
using System.Collections.Generic;


namespace NCRcatsmod
{
    [BepInPlugin(MOD_ID, "NCRCatsMod", "0.4.0")]
    class NCREntropy : BaseUnityPlugin
    {
        private const string MOD_ID = "neoncityrain.ncrcatsmod";
        FAtlas atlas;
        public bool IsDMSActive;

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

            // ---------------------------------------------------- ENTROPY STUFF ----------------------------------------------------
            //entropy karma seizure. here for future parton usage as well
            On.KarmaFlower.BitByPlayer += KarmaFlower_BitByPlayer;

            // entropy shock collar code
            On.Creature.Grab += Creature_Grab;

            // ---------------------------------------------------- MARAUDER STUFF ----------------------------------------------------
            // marauder more mauling
            On.Player.CanMaulCreature += Player_CanMaulCreature;

            // disallow marauder from putting slugs on back
            On.Player.CanIPutDeadSlugOnBack += Player_CanIPutDeadSlugOnBack;

            // worm grass
            On.WormGrass.WormGrassPatch.InteractWithCreature += WormGrassPatch_InteractWithCreature;

            // ---------------------------------------------------- VIVIATED STUFF ----------------------------------------------------
            //gross sounds when dying
            On.Player.Die += Player_Die;
        }

        private void WormGrassPatch_InteractWithCreature(On.WormGrass.WormGrassPatch.orig_InteractWithCreature orig, WormGrass.WormGrassPatch self, WormGrass.WormGrassPatch.CreatureAndPull creatureAndPull)
        {
            orig(self, creatureAndPull);
            if (creatureAndPull.creature is Player && (creatureAndPull.creature as Player).GetMarCat().IsMarauder && !creatureAndPull.creature.dead)
            {
                creatureAndPull.bury = 0f;
                creatureAndPull.pull = 0f;
                creatureAndPull.consumeTimer = 0;
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
            string sheetID = "neoncityrain.ncrentropydms";
            for (int index = 0; index < 4; index++)
            {
                SpriteDefinitions.AddSlugcatDefault(new Customization
                {
                    Slugcat = "NCREntropy",
                    PlayerNumber = index,
                    CustomSprites = new List<CustomSprite>
            {
                new CustomSprite
                {
                    Sprite = "TAIL",
                    SpriteSheetID = sheetID
                },
                new CustomSprite
                {
                    Sprite = "LEGS",
                    SpriteSheetID = sheetID
                },
                new CustomSprite
                {
                    Sprite = "ARMS",
                    SpriteSheetID = sheetID
                },
                new CustomSprite
                {
                    Sprite = "HIPS",
                    SpriteSheetID = sheetID
                },
                new CustomSprite
                {
                    Sprite = "BODY",
                    SpriteSheetID = sheetID
                },
                new CustomSprite
                {
                    Sprite = "HEAD",
                    SpriteSheetID = sheetID
                }
            },
                    CustomTail = new CustomTail
                    {
                        Length = 4f
                    }
                });
                Debug.Log("Entropy's default sprites set!");
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
            if (self.GetMarCat().IsMarauder)
            {
                return !(crit is IPlayerEdible) && self.IsCreatureLegalToHoldWithoutStun(crit) && !crit.dead;
            }
            else
            {
                bool flag = true;
                if (ModManager.CoopAvailable)
                {
                    Player player = crit as Player;
                    if (player != null && (player.isNPC || !Custom.rainWorld.options.friendlyFire))
                    {
                        flag = false;
                    }
                }
                return !(crit is Fly) && !crit.dead && (!(crit is IPlayerEdible) || (crit is Centipede && !(crit as Centipede).Edible) ||
                    self.FoodInStomach >= self.MaxFoodInStomach) && flag && (crit.Stunned || (!(crit is Cicada) && !(crit is Player) &&
                    self.IsCreatureLegalToHoldWithoutStun(crit))) && SlugcatStats.SlugcatCanMaul(self.SlugCatClass);
            }
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
                        self.room.AddObject(new UnderwaterShock(self.room, null, (obj as Player).mainBodyChunk.pos, 14, Mathf.Lerp(ModManager.MMF ? 0f : 200f, 1200f, 2f), 0.2f + 1.9f * 2f, (obj as Player), new Color(0.7f, 0.7f, 1f)));
                    }

                    //still plays bite sfx
                    if (self is Lizard)
                    {
                        self.room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, self.mainBodyChunk);
                    }

                    //only happens once
                    (obj as Player).GetEntCat().CollarShocks = false;

                    //visual effects
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV(), Color.white, null, 4, 8));
                    (obj as Player).room.AddObject(new Spark((obj as Player).mainBodyChunk.pos, Custom.RNV(), Color.white, null, 4, 8));
                    (obj as Player).room.AddObject(new Spark((obj as Player).mainBodyChunk.pos, Custom.RNV(), Color.white, null, 4, 8));
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
                if (self.playerState.foodInStomach > 1)
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
                grasp.grabber.room.AddObject(new Spark(grasp.grabber.mainBodyChunk.pos, Custom.RNV(), Color.white, null, 4, 8));
                grasp.grabber.room.AddObject(new Spark(grasp.grabber.mainBodyChunk.pos, Custom.RNV(), Color.white, null, 4, 8));
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

                if (self.player.GetEntCat().IsEntropy)
                {
                    if (self.player.GetEntCat().IsFree == false)
                    {
                        string DMSEntropyHead = "neoncityrain.ncrentropydms";
                        for (int index = 0; index < 4; index++)
                        {
                            SpriteDefinitions.AddSlugcatDefault(new Customization
                            {
                                Slugcat = "NCREntropy",
                                PlayerNumber = index,
                                CustomSprites = new List<CustomSprite>
                                {new CustomSprite
                                {
                                    Sprite = "HEAD",
                                    SpriteSheetID = DMSEntropyHead
                                }
                                }
                            });
                        }
                    }
                    else
                    {
                        string DMSEntropyHead = "neoncityrain.ncrentropydmsfree";
                        for (int index = 0; index < 4; index++)
                        {
                            SpriteDefinitions.AddSlugcatDefault(new Customization
                            {
                                Slugcat = "NCREntropy",
                                PlayerNumber = index,
                                CustomSprites = new List<CustomSprite>
                                {new CustomSprite
                                {
                                    Sprite = "HEAD",
                                    SpriteSheetID = DMSEntropyHead
                                }
                                }
                            });
                        }
                    }
                }
                
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