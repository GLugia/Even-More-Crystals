using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Loot
{
	/// <summary>
	/// This class is used for generic methods that deal with damage dealt <see cref="ModifierPlayer.ApplyDamageToNPC(NPC, int, float, int, bool, bool)"/>
	/// I have them here for two reasons:
	/// 1) there is no need for it to be in ModifierPlayer
	/// 2) these are very large methods i'd rather not look at for more than a few seconds tbh
	/// 
	/// I understand the current methods used here are direct copies of <see cref="Terraria.NPC.StrikeNPC(int, float, int, bool, bool, bool)"/>
	/// but I need the extra bool var to check for dots since we use them as secondary damage in this mod
	/// More dots might be added in the future. More specifically dots that deal certain damage types that enemies might be more resistant to
	/// I have many many plans for this mod besides just random modifiers and "cubes"
	/// </summary>
	public static class ModifierNpc
	{
		// unneeded atm but might be needed in the future
		private static ModifierPlayer thePlayer => ModifierPlayer.Player(Main.LocalPlayer);

		public static double StrikeNPC(NPC npc, int damage, float kb, int hitDirection, bool crit = false, bool noEffect = false, bool fromNet = false, bool dot = false)
		{
			bool flag = Main.netMode == 0;

			if (!npc.active || npc.life <= 0)
			{
				return 0.0;
			}

			double num = damage;
			int num2 = npc.defense;

			if (npc.ichor)
			{
				num2 -= 20;
			}

			if (npc.betsysCurse)
			{
				num2 -= 40;
			}

			if (num2 < 0)
			{
				num2 = 0;
			}

			if (NPCLoader.StrikeNPC(npc, ref num, num2, ref kb, hitDirection, ref crit))
			{
				num = Main.CalculateDamage((int)num, num2);

				if (crit)
				{
					num *= 2.0;
				}

				if (npc.takenDamageMultiplier > 1f)
				{
					num *= (double)npc.takenDamageMultiplier;
				}
			}

			if ((npc.takenDamageMultiplier > 1f || damage != 9999) && npc.lifeMax > 1)
			{
				if (npc.friendly)
				{
					Color color = crit ? CombatText.DamagedFriendlyCrit : CombatText.DamagedFriendly;

					CombatText.NewText(new Rectangle((int)npc.position.X, (int)npc.position.Y - npc.height / 2, npc.width, npc.height), color, (int)num, crit, dot);
				}
				else
				{
					Color color2 = crit ? CombatText.DamagedHostileCrit : CombatText.DamagedHostile;

					if (fromNet)
					{
						color2 = (crit ? CombatText.OthersDamagedHostileCrit : CombatText.OthersDamagedHostile);
					}

					CombatText.NewText(new Rectangle((int)npc.position.X, (int)npc.position.Y - npc.height / 2, npc.width, npc.height), color2, (int)num, crit, dot);
				}

				// @TODO not sure if this is needed here
				//
				//CombatText cbt = Main.combatText[cbtid];
				//NetMessage.SendData(MessageID.CombatTextString, -1, -1, null, (int)cbt.color.PackedValue, cbt.position.X, cbt.position.Y, (int)num);
			}

			if (num >= 1.0)
			{
				if (flag)
				{
					npc.PlayerInteraction(Main.myPlayer);
				}

				npc.justHit = true;

				if (npc.townNPC)
				{
					if (npc.aiStyle == 7 && (npc.ai[0] == 3f || npc.ai[0] == 4f || npc.ai[0] == 16f || npc.ai[0] == 17f))
					{
						NPC nPC = Main.npc[(int)npc.ai[2]];
						if (nPC.active)
						{
							nPC.ai[0] = 1f;
							nPC.ai[1] = 300 + Main.rand.Next(300);
							nPC.ai[2] = 0f;
							nPC.localAI[3] = 0f;
							nPC.direction = hitDirection;
							nPC.netUpdate = true;
						}
					}
					npc.ai[0] = 1f;
					npc.ai[1] = 300 + Main.rand.Next(300);
					npc.ai[2] = 0f;
					npc.localAI[3] = 0f;
					npc.direction = hitDirection;
					npc.netUpdate = true;
				}

				if (npc.aiStyle == 8 && Main.netMode != 1)
				{
					if (npc.type == NPCID.RuneWizard)
					{
						npc.ai[0] = 450f;
					}
					else if (npc.type == NPCID.Necromancer || npc.type == NPCID.NecromancerArmored)
					{
						if (Main.rand.Next(2) == 0)
						{
							npc.ai[0] = 390f;
							npc.netUpdate = true;
						}
					}
					else if (npc.type == NPCID.DesertDjinn)
					{
						if (Main.rand.Next(3) != 0)
						{
							npc.ai[0] = 181f;
							npc.netUpdate = true;
						}
					}
					else
					{
						npc.ai[0] = 400f;
					}

					npc.TargetClosest();
				}

				if (npc.aiStyle == 97 && Main.netMode != 1)
				{
					npc.localAI[1] = 1f;
					npc.TargetClosest();
				}

				if (npc.type == NPCID.DetonatingBubble)
				{
					num = 0.0;
					npc.ai[0] = 1f;
					npc.ai[1] = 4f;
					npc.dontTakeDamage = true;
				}

				if (npc.type == NPCID.SantaNK1 && (double)npc.life >= (double)npc.lifeMax * 0.5 && (double)npc.life - num < (double)npc.lifeMax * 0.5)
				{
					Gore.NewGore(npc.position, npc.velocity, 517);
				}

				if (npc.type == NPCID.SpikedIceSlime)
				{
					npc.localAI[0] = 60f;
				}

				if (npc.type == NPCID.SlimeSpiked)
				{
					npc.localAI[0] = 60f;
				}

				if (npc.type == NPCID.SnowFlinx)
				{
					npc.localAI[0] = 1f;
				}

				if (!npc.immortal && npc.type != NPCID.TargetDummy)
				{
					if (npc.realLife >= 0)
					{
						Main.npc[npc.realLife].life -= (int)num;
						npc.life = Main.npc[npc.realLife].life;
						npc.lifeMax = Main.npc[npc.realLife].lifeMax;
					}
					else
					{
						npc.life -= (int)num;
					}
				}

				if (kb > 0f && npc.knockBackResist > 0f)
				{
					float num3 = kb * npc.knockBackResist;

					if (num3 > 8f)
					{
						float num4 = num3 - 8f;
						num4 *= 0.9f;
						num3 = 8f + num4;
					}

					if (num3 > 10f)
					{
						float num5 = num3 - 10f;
						num5 *= 0.8f;
						num3 = 10f + num5;
					}

					if (num3 > 12f)
					{
						float num6 = num3 - 12f;
						num6 *= 0.7f;
						num3 = 12f + num6;
					}

					if (num3 > 14f)
					{
						float num7 = num3 - 14f;
						num7 *= 0.6f;
						num3 = 14f + num7;
					}

					if (num3 > 16f)
					{
						num3 = 16f;
					}

					if (crit)
					{
						num3 *= 1.4f;
					}

					int num8 = (int)num * 10;

					if (Main.expertMode)
					{
						num8 = (int)num * 15;
					}

					if (num8 > npc.lifeMax)
					{
						if (hitDirection < 0 && npc.velocity.X > 0f - num3)
						{
							if (npc.velocity.X > 0f)
							{
								npc.velocity.X -= num3;
							}

							npc.velocity.X -= num3;

							if (npc.velocity.X < 0f - num3)
							{
								npc.velocity.X = 0f - num3;
							}
						}
						else if (hitDirection > 0 && npc.velocity.X < num3)
						{
							if (npc.velocity.X < 0f)
							{
								npc.velocity.X += num3;
							}

							npc.velocity.X += num3;

							if (npc.velocity.X > num3)
							{
								npc.velocity.X = num3;
							}
						}

						if (npc.type == NPCID.SnowFlinx)
						{
							num3 *= 1.5f;
						}

						num3 = (npc.noGravity ? (num3 * -0.5f) : (num3 * -0.75f));

						if (npc.velocity.Y > num3)
						{
							npc.velocity.Y += num3;

							if (npc.velocity.Y < num3)
							{
								npc.velocity.Y = num3;
							}
						}
					}
					else
					{
						if (!npc.noGravity)
						{
							npc.velocity.Y = (0f - num3) * 0.75f * npc.knockBackResist;
						}
						else
						{
							npc.velocity.Y = (0f - num3) * 0.5f * npc.knockBackResist;
						}

						npc.velocity.X = num3 * (float)hitDirection * npc.knockBackResist;
					}
				}

				if ((npc.type == NPCID.WallofFlesh || npc.type == NPCID.WallofFleshEye) && npc.life <= 0)
				{
					for (int i = 0; i < 200; i++)
					{
						if (Main.npc[i].active && (Main.npc[i].type == NPCID.WallofFlesh || Main.npc[i].type == NPCID.WallofFleshEye))
						{
							Main.npc[i].HitEffect(hitDirection, num);
						}
					}
				}
				else
				{
					npc.HitEffect(hitDirection, num);
				}

				if (npc.HitSound != null && !dot)
				{
					Main.PlaySound(npc.HitSound, npc.position);
				}

				if (npc.realLife >= 0)
				{
					Main.npc[npc.realLife].checkDead();
				}
				else
				{
					npc.checkDead();
				}

				return num;
			}

			return 0.0;
		}

		public static double StrikePlayer(Player target, PlayerDeathReason damageSource, int Damage, int hitDirection, bool pvp = false, bool quiet = false, bool Crit = false, int cooldownCounter = -1, bool dot = false)
		{
			bool flag = !target.immune;
			bool flag2 = false;
			int hitContext = cooldownCounter;

			if (cooldownCounter == 0)
			{
				flag = (target.hurtCooldowns[cooldownCounter] <= 0);
			}

			if (cooldownCounter == 1)
			{
				flag = (target.hurtCooldowns[cooldownCounter] <= 0);
			}

			if (cooldownCounter == 2)
			{
				flag2 = true;
				cooldownCounter = -1;
			}

			if (!flag)
			{
				return 0.0;
			}

			if (!dot && target.whoAmI == Main.myPlayer && target.blackBelt && Main.rand.Next(10) == 0)
			{
				target.NinjaDodge();
				return 0.0;
			}

			if (!dot && target.whoAmI == Main.myPlayer && target.shadowDodge)
			{
				target.ShadowDodge();
				return 0.0;
			}

			bool customDamage = false;
			bool playSound = !dot;
			bool genGore = !dot;

			if (!PlayerHooks.PreHurt(target, pvp, quiet, ref Damage, ref hitDirection, ref Crit, ref customDamage, ref playSound, ref genGore, ref damageSource))
			{
				return 0.0;
			}

			if (target.whoAmI == Main.myPlayer && target.panic)
			{
				target.AddBuff(BuffID.Panic, 300);
			}

			if (target.whoAmI == Main.myPlayer && target.setSquireT2)
			{
				target.AddBuff(BuffID.BallistaPanic, 300);
			}

			target.stealth = 1f;

			if (Main.netMode == 1)
			{
				NetMessage.SendData(MessageID.PlayerStealth, -1, -1, null, target.whoAmI);
			}

			int num = Damage;
			double num2 = customDamage ? ((double)num) : Main.CalculatePlayerDamage(num, target.statDefense);

			if (Crit)
			{
				num *= 2;
			}

			if (num2 >= 1.0)
			{
				if (target.invis)
				{
					for (int i = 0; i < Player.MaxBuffs; i++)
					{
						if (target.buffType[i] == BuffID.Invisibility)
						{
							target.DelBuff(i);
						}
					}
				}

				num2 = (int)((double)(1f - target.endurance) * num2);

				if (num2 < 1.0)
				{
					num2 = 1.0;
				}

				if (target.ConsumeSolarFlare())
				{
					float num3 = 0.3f;
					num2 = (int)((double)(1f - num3) * num2);

					if (num2 < 1.0)
					{
						num2 = 1.0;
					}

					if (target.whoAmI == Main.myPlayer)
					{
						int num4 = Projectile.NewProjectile(target.Center.X, target.Center.Y, 0f, 0f, 608, 150, 15f, Main.myPlayer);

						Main.projectile[num4].Kill();
					}
				}

				if (target.beetleDefense && target.beetleOrbs > 0 && !dot)
				{
					float num5 = 0.15f * (float)target.beetleOrbs;
					num2 = (int)((double)(1f - num5) * num2);
					target.beetleOrbs--;

					for (int j = 0; j < Player.MaxBuffs; j++)
					{
						if (target.buffType[j] >= BuffID.BeetleEndurance1 && target.buffType[j] <= BuffID.BeetleEndurance3)
						{
							target.DelBuff(j);
						}
					}

					if (target.beetleOrbs > 0)
					{
						target.AddBuff(BuffID.BeetleEndurance1 + target.beetleOrbs - 1, 5, quiet: false);
					}

					target.beetleCounter = 0f;

					if (num2 < 1.0)
					{
						num2 = 1.0;
					}
				}

				if (target.magicCuffs)
				{
					int num6 = num;
					target.statMana += num6;

					if (target.statMana > target.statManaMax2)
					{
						target.statMana = target.statManaMax2;
					}

					target.ManaEffect(num6);
				}

				if (target.defendedByPaladin)
				{
					if (target.whoAmI != Main.myPlayer)
					{
						if (Main.player[Main.myPlayer].hasPaladinShield)
						{
							Player player = Main.player[Main.myPlayer];

							if (player.team == target.team && target.team != 0)
							{
								float num7 = player.Distance(target.Center);
								bool flag3 = num7 < 800f;

								if (flag3)
								{
									for (int k = 0; k < 255; k++)
									{
										if (k != Main.myPlayer && Main.player[k].active && !Main.player[k].dead && !Main.player[k].immune && Main.player[k].hasPaladinShield && Main.player[k].team == target.team && (float)Main.player[k].statLife > (float)Main.player[k].statLifeMax2 * 0.25f)
										{
											float num8 = Main.player[k].Distance(target.Center);

											if (num7 > num8 || (num7 == num8 && k < Main.myPlayer))
											{
												flag3 = false;
												break;
											}
										}
									}
								}

								if (flag3)
								{
									int damage = (int)(num2 * 0.25);
									num2 = (int)(num2 * 0.75);

									player.Hurt(PlayerDeathReason.LegacyEmpty(), damage, 0);
								}
							}
						}
					}
					else
					{
						bool flag4 = false;

						for (int l = 0; l < 255; l++)
						{
							if (l != Main.myPlayer && Main.player[l].active && !Main.player[l].dead && !Main.player[l].immune && Main.player[l].hasPaladinShield && Main.player[l].team == target.team && (float)Main.player[l].statLife > (float)Main.player[l].statLifeMax2 * 0.25f)
							{
								flag4 = true;
								break;
							}
						}

						if (flag4)
						{
							num2 = (int)(num2 * 0.75);
						}
					}
				}

				if (target.brainOfConfusion && Main.myPlayer == target.whoAmI)
				{
					for (int m = 0; m < 200; m++)
					{
						if (!Main.npc[m].active || Main.npc[m].friendly)
						{
							continue;
						}

						int num9 = 300;
						num9 += (int)num2 * 2;

						if (Main.rand.Next(500) < num9)
						{
							float num10 = (Main.npc[m].Center - target.Center).Length();
							float num11 = Main.rand.Next(200 + (int)num2 / 2, 301 + (int)num2 * 2);

							if (num11 > 500f)
							{
								num11 = 500f + (num11 - 500f) * 0.75f;
							}

							if (num11 > 700f)
							{
								num11 = 700f + (num11 - 700f) * 0.5f;
							}

							if (num11 > 900f)
							{
								num11 = 900f + (num11 - 900f) * 0.25f;
							}

							if (num10 < num11)
							{
								float num12 = Main.rand.Next(90 + (int)num2 / 3, 300 + (int)num2 / 2);

								Main.npc[m].AddBuff(BuffID.Confused, (int)num12);
							}
						}
					}

					Projectile.NewProjectile(target.Center.X + (float)Main.rand.Next(-40, 40), target.Center.Y - (float)Main.rand.Next(20, 60), target.velocity.X * 0.3f, target.velocity.Y * 0.3f, 565, 0, 0f, target.whoAmI);
				}

				PlayerHooks.Hurt(target, pvp, quiet, num2, hitDirection, Crit);

				if (Main.netMode == 1 && target.whoAmI == Main.myPlayer && !quiet)
				{
					NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, target.whoAmI);
					NetMessage.SendData(MessageID.PlayerHealth, -1, -1, null, target.whoAmI);
					NetMessage.SendPlayerHurt(target.whoAmI, damageSource, Damage, hitDirection, Crit, pvp, hitContext);
				}

				Color color = Crit ? CombatText.DamagedFriendlyCrit : CombatText.DamagedFriendly;
				CombatText.NewText(new Rectangle((int)target.position.X, (int)target.position.Y - target.height / 2, target.width, target.height), color, (int)num2, Crit, dot);

				target.statLife -= (int)num2;

				if (!dot)
				{
					switch (cooldownCounter)
					{
						case -1:
							{
								target.immune = true;

								if (num2 == 1.0)
								{
									target.immuneTime = 20;

									if (target.longInvince)
									{
										target.immuneTime += 20;
									}
								}
								else
								{
									target.immuneTime = 40;

									if (target.longInvince)
									{
										target.immuneTime += 40;
									}
								}

								if (pvp)
								{
									target.immuneTime = 8;
								}

								break;
							}
						case 0:
							{
								if (num2 == 1.0)
								{
									target.hurtCooldowns[cooldownCounter] = (target.longInvince ? 40 : 20);
								}
								else
								{
									target.hurtCooldowns[cooldownCounter] = (target.longInvince ? 80 : 40);
								}

								break;
							}
						case 1:
							{
								if (num2 == 1.0)
								{
									target.hurtCooldowns[cooldownCounter] = (target.longInvince ? 40 : 20);
								}
								else
								{
									target.hurtCooldowns[cooldownCounter] = (target.longInvince ? 80 : 40);
								}

								break;
							}
					}
				}

				target.lifeRegenTime = 0;

				if (target.whoAmI == Main.myPlayer)
				{
					if (target.starCloak && !dot)
					{
						for (int n = 0; n < 3; n++)
						{
							float x = target.position.X + (float)Main.rand.Next(-400, 400);
							float y = target.position.Y - (float)Main.rand.Next(500, 800);
							Vector2 vector = new Vector2(x, y);
							float num13 = target.position.X + (float)(target.width / 2) - vector.X;
							float num14 = target.position.Y + (float)(target.height / 2) - vector.Y;
							num13 += (float)Main.rand.Next(-100, 101);
							float num15 = (float)Math.Sqrt(num13 * num13 + num14 * num14);
							num15 = 23f / num15;
							num13 *= num15;
							num14 *= num15;

							int num16 = Projectile.NewProjectile(x, y, num13, num14, 92, 30, 5f, target.whoAmI);
							Main.projectile[num16].ai[1] = target.position.Y;
						}
					}

					if (target.bee && !dot)
					{
						int num17 = 1;

						if (Main.rand.Next(3) == 0)
						{
							num17++;
						}

						if (Main.rand.Next(3) == 0)
						{
							num17++;
						}

						if (target.strongBees && Main.rand.Next(3) == 0)
						{
							num17++;
						}

						for (int num18 = 0; num18 < num17; num18++)
						{
							float speedX = (float)Main.rand.Next(-35, 36) * 0.02f;
							float speedY = (float)Main.rand.Next(-35, 36) * 0.02f;

							Projectile.NewProjectile(target.position.X, target.position.Y, speedX, speedY, target.beeType(), target.beeDamage(7), target.beeKB(0f), Main.myPlayer);
						}
					}
				}

				if (flag2 && hitDirection != 0)
				{
					if (!target.mount.Active || !target.mount.Cart)
					{
						float num19 = 10.5f;
						float y2 = -7.5f;

						if (target.noKnockback)
						{
							num19 = 2.5f;
							y2 = -1.5f;
						}

						target.velocity.X = num19 * (float)hitDirection;
						target.velocity.Y = y2;
					}
				}
				else if (!target.noKnockback && hitDirection != 0 && (!target.mount.Active || !target.mount.Cart))
				{
					target.velocity.X = 4.5f * (float)hitDirection;
					target.velocity.Y = -3.5f;
				}

				if (playSound)
				{
					if (target.stoned)
					{
						Main.PlaySound(0, (int)target.position.X, (int)target.position.Y);
					}
					else if (target.frostArmor)
					{
						Main.PlaySound(SoundID.Item27, target.position);
					}
					else if ((target.wereWolf || target.forceWerewolf) && !target.hideWolf)
					{
						Main.PlaySound(3, (int)target.position.X, (int)target.position.Y, 6);
					}
					else if (target.boneArmor)
					{
						Main.PlaySound(3, (int)target.position.X, (int)target.position.Y, 2);
					}
					else if (!target.Male)
					{
						Main.PlaySound(20, (int)target.position.X, (int)target.position.Y);
					}
					else
					{
						Main.PlaySound(1, (int)target.position.X, (int)target.position.Y);
					}
				}

				if (target.statLife > 0)
				{
					if (genGore)
					{
						double num20 = num2 / (double)target.statLifeMax2 * 100.0;
						float num21 = 2 * hitDirection;
						float num22 = 0f;

						if (flag2)
						{
							num20 *= 12.0;
							num22 = 6f;
						}

						for (int num23 = 0; (double)num23 < num20; num23++)
						{
							if (target.stoned)
							{
								Dust.NewDust(target.position, target.width, target.height, 1, num21 + (float)hitDirection * num22 * Main.rand.NextFloat(), -2f);
							}
							else if (target.frostArmor)
							{
								int num24 = Dust.NewDust(target.position, target.width, target.height, 135, num21 + (float)hitDirection * num22 * Main.rand.NextFloat(), -2f);
								Main.dust[num24].shader = GameShaders.Armor.GetSecondaryShader(target.ArmorSetDye(), target);
							}
							else if (target.boneArmor)
							{
								int num25 = Dust.NewDust(target.position, target.width, target.height, 26, num21 + (float)hitDirection * num22 * Main.rand.NextFloat(), -2f);
								Main.dust[num25].shader = GameShaders.Armor.GetSecondaryShader(target.ArmorSetDye(), target);
							}
							else
							{
								Dust.NewDust(target.position, target.width, target.height, 5, num21 + (float)hitDirection * num22 * Main.rand.NextFloat(), -2f);
							}
						}
					}

					PlayerHooks.PostHurt(target, pvp, quiet, num2, hitDirection, Crit);
				}
				else
				{
					target.statLife = 0;

					if (target.whoAmI == Main.myPlayer)
					{
						target.KillMe(damageSource, num2, hitDirection, pvp);
					}
				}
			}

			if (pvp)
			{
				num2 = Main.CalculateDamage(num, target.statDefense);
			}

			return num2;
		}

		// @TODO isn't needed yet but leave it here just in case
		private static void CalculatePlayerDamage(ref int Damage, int Defense, float Endurance)
		{
			double mod = 0.5;

			if (Main.expertMode)
			{
				mod = 0.75;
			}

			Damage -= (int)Math.Ceiling(Defense * mod * (1.0 - Endurance));

			if (Damage < 1)
			{
				Damage = 1;
			}
		}
	}
}
