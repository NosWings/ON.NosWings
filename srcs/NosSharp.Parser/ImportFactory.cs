/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;

namespace NosSharp.Parser
{
    public class ImportFactory
    {
        #region Instantiation

        public ImportFactory(string folder) => _folder = folder;

        #endregion

        #region Members

        private readonly string _folder;
        private readonly List<string[]> _packetList = new List<string[]>();
        private List<MapDTO> _maps;

        #endregion

        #region Methods

        public static void ImportAccounts()
        {
            if (DaoFactory.AccountDao.ContainsAccounts())
            {
                // If there are accounts in the database, there is no need to re add these 2 accounts.
                // We definately don't want people to access the admin account.
                return;
            }

            var acc1 = new AccountDTO
            {
                AccountId = 1,
                Authority = AuthorityType.GameMaster,
                Name = "admin",
                Password = EncryptionBase.Sha512("test")
            };
            DaoFactory.AccountDao.InsertOrUpdate(ref acc1);

            var acc2 = new AccountDTO
            {
                AccountId = 2,
                Authority = AuthorityType.User,
                Name = "test",
                Password = EncryptionBase.Sha512("test")
            };
            DaoFactory.AccountDao.InsertOrUpdate(ref acc2);
        }

        public void ImportQuests()
        {
            string fileQuestDat = $"{_folder}\\quest.dat";
            string fileRewardsDat = $"{_folder}\\qstprize.dat";
            int qstCounter = 0;

            Dictionary<long, QuestRewardDTO> dictionaryRewards = new Dictionary<long, QuestRewardDTO>();
            var reward = new QuestRewardDTO();
            string line;

            using (var questRewardStream = new StreamReader(fileRewardsDat, Encoding.GetEncoding(1252)))
            {
                while ((line = questRewardStream.ReadLine()) != null)
                {
                    string[] currentLine = line.Split('\t');
                    if (currentLine.Length <= 1 && currentLine[0] != "END")
                    {
                        continue;
                    }

                    switch (currentLine[0])
                    {
                        case "VNUM":
                            reward = new QuestRewardDTO
                            {
                                QuestRewardId = long.Parse(currentLine[1]),
                                RewardType = byte.Parse(currentLine[2])
                            };
                            break;

                        case "DATA":
                            if (currentLine.Length < 3)
                            {
                                return;
                            }

                            switch ((QuestRewardType)reward.RewardType)
                            {
                                case QuestRewardType.Exp:
                                case QuestRewardType.SecondExp:
                                case QuestRewardType.JobExp:
                                case QuestRewardType.SecondJobExp:
                                    reward.Data = int.Parse(currentLine[2]) == -1 ? 0 : int.Parse(currentLine[2]);
                                    reward.Amount = int.Parse(currentLine[1]);
                                    break;

                                case QuestRewardType.WearItem:
                                    reward.Data = int.Parse(currentLine[1]);
                                    reward.Amount = 1;
                                    break;

                                case QuestRewardType.EtcMainItem:
                                    reward.Data = int.Parse(currentLine[1]);
                                    reward.Amount = int.Parse(currentLine[5]) == -1 ? 1 : int.Parse(currentLine[5]);
                                    break;

                                case QuestRewardType.Gold:
                                case QuestRewardType.SecondGold:
                                case QuestRewardType.ThirdGold:
                                case QuestRewardType.FourthGold:
                                case QuestRewardType.Reput:
                                    reward.Data = 0;
                                    reward.Amount = int.Parse(currentLine[1]);
                                    break;

                                default:
                                    reward.Data = int.Parse(currentLine[1]);
                                    reward.Amount = int.Parse(currentLine[2]);
                                    break;
                            }

                            break;

                        case "END":
                            dictionaryRewards[reward.QuestRewardId] = reward;
                            break;
                    }
                }

                questRewardStream.Close();
            }


            // Final List
            List<QuestDTO> quests = new List<QuestDTO>();
            List<QuestRewardDTO> rewards = new List<QuestRewardDTO>();
            List<QuestObjectiveDTO> questObjectives = new List<QuestObjectiveDTO>();

            // Current
            var quest = new QuestDTO();
            List<QuestRewardDTO> currentRewards = new List<QuestRewardDTO>();
            List<QuestObjectiveDTO> currentObjectives = new List<QuestObjectiveDTO>();

            byte objectiveIndex = 0;
            using (var questStream = new StreamReader(fileQuestDat, Encoding.GetEncoding(1252)))
            {
                while ((line = questStream.ReadLine()) != null)
                {
                    string[] currentLine = line.Split('\t');
                    if (currentLine.Length > 1 || currentLine[0] == "END")
                    {
                        switch (currentLine[0])
                        {
                            case "VNUM":
                                quest = new QuestDTO
                                {
                                    QuestId = long.Parse(currentLine[1]),
                                    QuestType = int.Parse(currentLine[2]),
                                    InfoId = int.Parse(currentLine[1])
                                };
                                switch (quest.QuestId)
                                {
                                    //TODO: Legendary Hunter quests will be markes ad daily, but should be in the "secondary" slot and be daily nevertheless
                                    case 6057: // John the adventurer
                                    case 7519: // Legendary Hunter 1 time Kertos
                                    case 7520: // Legendary Hunter 1 time Valakus
                                    case 7521: // Legendary Hunter Grenigas
                                    case 7522: // Legendary Hunter Draco
                                    case 7523: // Legendary Hunter Glacerus
                                    case 7524: // Legendary Hunter Laurena
                                    case 5514: // Sherazade ice flower (n_run 65)
                                    case 5919: // Akamur's military engineer (n_run 68)
                                    case 5908: // John (n_run 67)
                                    case 5914: // Alchemist (n_run 66)
                                        quest.IsDaily = true;
                                        break;
                                }

                                objectiveIndex = 0;
                                currentRewards.Clear();
                                currentObjectives.Clear();
                                break;

                            case "LINK":
                                if (int.Parse(currentLine[1]) != -1) // Base Quest Order (ex: SpQuest)
                                {
                                    quest.NextQuestId = int.Parse(currentLine[1]);
                                    continue;
                                }

                                // Main Quest Order
                                switch (quest.QuestId)
                                {
                                    case 1997:
                                        quest.NextQuestId = 1500;
                                        break;
                                    case 1523:
                                    case 1532:
                                    case 1580:
                                    case 1610:
                                    case 1618:
                                    case 1636:
                                    case 1647:
                                    case 1664:
                                    case 3075:
                                        quest.NextQuestId = quest.QuestId + 2;
                                        break;
                                    case 1527:
                                    case 1553:
                                        quest.NextQuestId = quest.QuestId + 3;
                                        break;
                                    case 1690:
                                        quest.NextQuestId = 1694;
                                        break;
                                    case 1751:
                                        quest.NextQuestId = 3000;
                                        break;
                                    case 3101:
                                        quest.NextQuestId = 3200;
                                        break;
                                    case 3331:
                                        quest.NextQuestId = 3340;
                                        break;

                                    default:
                                        if (quest.QuestId < 1500 || quest.QuestId >= 1751 && quest.QuestId < 3000 || quest.QuestId >= 3374)
                                        {
                                            continue;
                                        }

                                        quest.NextQuestId = quest.QuestId + 1;
                                        break;
                                }

                                break;

                            case "LEVEL":
                                quest.LevelMin = byte.Parse(currentLine[1]);
                                quest.LevelMax = byte.Parse(currentLine[2]);
                                break;

                            case "TALK":
                                if (int.Parse(currentLine[1]) > 0)
                                {
                                    quest.StartDialogId = int.Parse(currentLine[1]);
                                }

                                if (int.Parse(currentLine[2]) > 0)
                                {
                                    quest.EndDialogId = int.Parse(currentLine[2]);
                                }

                                break;

                            case "TARGET":
                                if (int.Parse(currentLine[3]) > 0)
                                {
                                    quest.TargetMap = short.Parse(currentLine[3]);
                                    quest.TargetX = short.Parse(currentLine[1]);
                                    quest.TargetY = short.Parse(currentLine[2]);
                                }

                                break;

                            case "DATA":
                                if (currentLine.Length < 3)
                                {
                                    return;
                                }

                                objectiveIndex++;
                                int? data = null, objective = null, specialData = null, secondSpecialData = null;
                                switch ((QuestType)quest.QuestType)
                                {
                                    case QuestType.Hunt:
                                    case QuestType.Capture1:
                                    case QuestType.Capture2:
                                    case QuestType.Collect1:
                                    case QuestType.Product:
                                        data = int.Parse(currentLine[1]);
                                        objective = int.Parse(currentLine[2]);
                                        break;

                                    case QuestType.Brings: // npcVNum - ItemCount - ItemVNum //
                                    case QuestType.Collect3: // ItemVNum - Objective - TsId //
                                    case QuestType.Needed: // ItemVNum - Objective - npcVNum //
                                    case QuestType.Collect5: // ItemVNum - Objective - npcVNum //
                                        data = int.Parse(currentLine[2]);
                                        objective = int.Parse(currentLine[3]);
                                        specialData = int.Parse(currentLine[1]);
                                        break;

                                    case QuestType.Collect4: // ItemVNum - Objective - MonsterVNum - DropRate // 
                                    case QuestType.Collect2: // ItemVNum - Objective - MonsterVNum - DropRate // 
                                        data = int.Parse(currentLine[2]);
                                        objective = int.Parse(currentLine[3]);
                                        specialData = int.Parse(currentLine[1]);
                                        secondSpecialData = int.Parse(currentLine[4]);
                                        break;

                                    case QuestType.TimesSpace: // TS Lvl - Objective - TS Id //
                                    case QuestType.TsPoint:
                                        data = int.Parse(currentLine[4]);
                                        objective = int.Parse(currentLine[2]);
                                        specialData = int.Parse(currentLine[1]);
                                        break;

                                    case QuestType.Wear: // Item VNum - * - NpcVNum //
                                        data = int.Parse(currentLine[2]);
                                        specialData = int.Parse(currentLine[1]);
                                        break;

                                    case QuestType.TransmitGold: // NpcVNum - Gold x10K - * //
                                        data = int.Parse(currentLine[1]);
                                        objective = int.Parse(currentLine[2]) * 10000;
                                        break;

                                    case QuestType.GoTo: // Map - PosX - PosY //
                                        data = int.Parse(currentLine[1]);
                                        objective = int.Parse(currentLine[2]);
                                        specialData = int.Parse(currentLine[3]);
                                        break;

                                    case QuestType.WinRaid: // Design - Objective - ? //
                                        data = int.Parse(currentLine[1]);
                                        objective = int.Parse(currentLine[2]);
                                        specialData = int.Parse(currentLine[3]);
                                        break;

                                    case QuestType.Use: // Item to use - * - mateVnum //
                                        data = int.Parse(currentLine[1]);
                                        specialData = int.Parse(currentLine[2]);
                                        break;

                                    case QuestType.Dialog1: // npcVNum - * - * //
                                    case QuestType.Dialog2: // npcVNum - * - * //
                                        data = int.Parse(currentLine[1]);
                                        break;

                                    case QuestType.FlowerQuest:
                                        objective = 10;
                                        break;

                                    case QuestType.Inspect: // NpcVNum - Objective - ItemVNum //
                                    case QuestType.Required: // npcVNum - Objective - ItemVNum //
                                        data = int.Parse(currentLine[1]);
                                        objective = int.Parse(currentLine[3]);
                                        specialData = int.Parse(currentLine[2]);
                                        break;

                                    default:
                                        data = int.Parse(currentLine[1]);
                                        objective = int.Parse(currentLine[2]);
                                        specialData = int.Parse(currentLine[3]);
                                        break;
                                }

                                currentObjectives.Add(new QuestObjectiveDTO
                                {
                                    Data = data,
                                    Objective = objective ?? 1,
                                    SpecialData = specialData < 0 ? null : specialData,
                                    DropRate = secondSpecialData < 0 ? null : specialData,
                                    ObjectiveIndex = objectiveIndex,
                                    QuestId = (int)quest.QuestId
                                });
                                break;

                            case "PRIZE":
                                for (int a = 1; a < 5; a++)
                                {
                                    if (!dictionaryRewards.ContainsKey(long.Parse(currentLine[a])))
                                    {
                                        continue;
                                    }

                                    QuestRewardDTO currentReward = dictionaryRewards[long.Parse(currentLine[a])];
                                    currentRewards.Add(new QuestRewardDTO
                                    {
                                        RewardType = currentReward.RewardType,
                                        Data = currentReward.Data,
                                        Amount = currentReward.Amount,
                                        QuestId = quest.QuestId
                                    });
                                }

                                break;

                            case "END":
                                if (DaoFactory.QuestDao.LoadById(quest.QuestId) == null)
                                {
                                    questObjectives.AddRange(currentObjectives);
                                    rewards.AddRange(currentRewards);
                                    qstCounter++;
                                }

                                quests.Add(quest);
                                break;
                        }
                    }
                }

                DaoFactory.QuestDao.InsertOrUpdate(quests);
                DaoFactory.QuestRewardDao.Insert(rewards);
                DaoFactory.QuestObjectiveDao.Insert(questObjectives);
                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("QUEST_PARSED"), qstCounter));
                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("QUEST_REWARD_PARSED"), rewards.Count));
                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("QUEST_OBJECTIVE_PARSED"), questObjectives.Count));

                questStream.Close();
            }
        }

        public void ImportCards()
        {
            string fileCardDat = $"{_folder}\\Card.dat";
            string fileCardLang = $"{_folder}\\_code_{ConfigurationManager.AppSettings["Language"]}_Card.txt";
            List<CardDTO> cards = new List<CardDTO>();
            Dictionary<string, string> dictionaryIdLang = new Dictionary<string, string>();
            var card = new CardDTO();
            List<BCardDTO> bcards = new List<BCardDTO>();
            DaoFactory.BCardDao.Clean();
            string line;
            int counter = 0;
            bool itemAreaBegin = false;

            using (var npcIdLangStream = new StreamReader(fileCardLang, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length > 1 && !dictionaryIdLang.ContainsKey(linesave[0]))
                    {
                        dictionaryIdLang.Add(linesave[0], linesave[1]);
                    }
                }

                npcIdLangStream.Close();
            }

            using (var npcIdStream = new StreamReader(fileCardDat, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdStream.ReadLine()) != null)
                {
                    string[] currentLine = line.Split('\t');

                    if (currentLine.Length > 2 && currentLine[1] == "VNUM")
                    {
                        card = new CardDTO
                        {
                            CardId = Convert.ToInt16(currentLine[2])
                        };
                        itemAreaBegin = true;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "NAME")
                    {
                        card.Name = dictionaryIdLang.ContainsKey(currentLine[2]) ? dictionaryIdLang[currentLine[2]] : string.Empty;
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "GROUP")
                    {
                        if (!itemAreaBegin)
                        {
                            continue;
                        }

                        card.Level = Convert.ToByte(currentLine[3]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "EFFECT")
                    {
                        card.EffectId = Convert.ToInt32(currentLine[2]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "STYLE")
                    {
                        card.BuffType = (BuffType)Convert.ToByte(currentLine[3]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "TIME")
                    {
                        card.Duration = Convert.ToInt32(currentLine[2]);
                        card.Delay = Convert.ToInt32(currentLine[3]);
                    }
                    else
                    {
                        BCardDTO bcard;
                        if (currentLine.Length > 3 && currentLine[1] == "1ST")
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (currentLine[2 + i * 6] == "-1" || currentLine[2 + i * 6] == "0")
                                {
                                    continue;
                                }

                                int first = int.Parse(currentLine[i * 6 + 6]);
                                bcard = new BCardDTO
                                {
                                    CardId = card.CardId,
                                    Type = byte.Parse(currentLine[2 + i * 6]),
                                    SubType = (byte)((Convert.ToByte(currentLine[3 + i * 6]) + 1) * 10 + 1 + (first < 0 ? 1 : 0)),
                                    FirstData = (first > 0 ? first : -first) / 4,
                                    SecondData = int.Parse(currentLine[7 + i * 6]) / 4,
                                    ThirdData = int.Parse(currentLine[5 + i * 6]),
                                    IsLevelScaled = Convert.ToBoolean(first % 4),
                                    IsLevelDivided = Math.Abs(first % 4) == 2
                                };
                                bcards.Add(bcard);
                            }
                        }
                        else if (currentLine.Length > 3 && currentLine[1] == "2ST")
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                if (currentLine[2 + i * 6] == "-1" || currentLine[2 + i * 6] == "0")
                                {
                                    continue;
                                }

                                int first = int.Parse(currentLine[i * 6 + 6]);
                                bcard = new BCardDTO
                                {
                                    CardId = card.CardId,
                                    Type = byte.Parse(currentLine[2 + i * 6]),
                                    SubType = (byte)((Convert.ToByte(currentLine[3 + i * 6]) + 1) * 10 + 1 + (first < 0 ? 1 : 0)),
                                    FirstData = (first > 0 ? first : -first) / 4,
                                    SecondData = int.Parse(currentLine[7 + i * 6]) / 4,
                                    ThirdData = int.Parse(currentLine[5 + i * 6]),
                                    IsLevelScaled = Convert.ToBoolean(first % 4),
                                    IsLevelDivided = (first % 4) == 2
                                };
                                bcards.Add(bcard);
                            }
                        }
                        else if (currentLine.Length > 3 && currentLine[1] == "LAST")
                        {
                            card.TimeoutBuff = short.Parse(currentLine[2]);
                            card.TimeoutBuffChance = byte.Parse(currentLine[3]);

                            // investigate
                            if (DaoFactory.CardDao.LoadById(card.CardId) == null)
                            {
                                cards.Add(card);
                                counter++;
                            }

                            itemAreaBegin = false;
                        }
                    }
                }

                DaoFactory.CardDao.Insert(cards);
                DaoFactory.BCardDao.Insert(bcards);

                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("CARDS_PARSED"), counter));
                npcIdStream.Close();
            }
        }


        public void ImportMapNpcs()
        {
            int npcCounter = 0;
            short map = 0;
            List<MapNpcDTO> npcs = new List<MapNpcDTO>();
            List<int> npcMvPacketsList = new List<int>();
            Dictionary<int, short> effPacketsDictionary = new Dictionary<int, short>();
            npcs.Add(new MapNpcDTO // Add Broken Red Plate as Npc for quests
            {
                MapX = 102,
                MapY = 154,
                MapId = 5,
                NpcVNum = 860,
                Position = 2,
                IsMoving = false,
                EffectDelay = 4750,
                Dialog = 999 // unused dialog
            });

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("mv") && o[1].Equals("2")))
            {
                if (long.Parse(currentPacket[2]) >= 20000)
                {
                    continue;
                }

                if (!npcMvPacketsList.Contains(Convert.ToInt32(currentPacket[2])))
                {
                    npcMvPacketsList.Add(Convert.ToInt32(currentPacket[2]));
                }
            }

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("eff") && o[1].Equals("2")))
            {
                if (long.Parse(currentPacket[2]) >= 20000)
                {
                    continue;
                }

                if (!effPacketsDictionary.ContainsKey(Convert.ToInt32(currentPacket[2])))
                {
                    effPacketsDictionary.Add(Convert.ToInt32(currentPacket[2]), Convert.ToInt16(currentPacket[3]));
                }
            }

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("in") || o[0].Equals("at")))
            {
                if (currentPacket.Length > 5 && currentPacket[0] == "at")
                {
                    map = short.Parse(currentPacket[2]);
                    continue;
                }

                if (currentPacket.Length <= 7 || currentPacket[0] != "in" || currentPacket[1] != "2")
                {
                    continue;
                }

                var npctest = new MapNpcDTO
                {
                    MapX = short.Parse(currentPacket[4]),
                    MapY = short.Parse(currentPacket[5]),
                    MapId = map,
                    NpcVNum = short.Parse(currentPacket[2])
                };
                if (long.Parse(currentPacket[3]) > 20000)
                {
                    continue;
                }

                npctest.MapNpcId = short.Parse(currentPacket[3]);
                if (effPacketsDictionary.ContainsKey(npctest.MapNpcId))
                {
                    npctest.Effect = (short)(npctest.NpcVNum == 453 /*Lod*/ ? 855 : effPacketsDictionary[npctest.MapNpcId]);
                }

                npctest.EffectDelay = 4750;
                npctest.IsMoving = npcMvPacketsList.Contains(npctest.MapNpcId);
                npctest.Position = byte.Parse(currentPacket[6]);
                npctest.Dialog = short.Parse(currentPacket[9]);
                npctest.IsSitting = currentPacket[13] != "1";
                npctest.IsDisabled = false;

                if (DaoFactory.NpcMonsterDao.LoadByVNum(npctest.NpcVNum) == null || DaoFactory.MapNpcDao.LoadById(npctest.MapNpcId) != null || npcs.Count(i => i.MapNpcId == npctest.MapNpcId) != 0)
                {
                    continue;
                }

                npcs.Add(npctest);
                npcCounter++;
            }

            DaoFactory.MapNpcDao.Insert(npcs);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("NPCS_PARSED"), npcCounter));
        }

        public void ImportMaps()
        {
            string fileMapIdDat = $"{_folder}\\MapIDData.dat";
            string fileMapIdLang = $"{_folder}\\_code_{ConfigurationManager.AppSettings["Language"]}_MapIDData.txt";
            string folderMap = $"{_folder}\\map";
            List<MapDTO> maps = new List<MapDTO>();
            Dictionary<int, string> dictionaryId = new Dictionary<int, string>();
            Dictionary<string, string> dictionaryIdLang = new Dictionary<string, string>();
            Dictionary<int, int> dictionaryMusic = new Dictionary<int, int>();

            string line;
            int i = 0;
            using (var mapIdStream = new StreamReader(fileMapIdDat, Encoding.GetEncoding(1252)))
            {
                while ((line = mapIdStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split(' ');
                    if (linesave.Length <= 1)
                    {
                        continue;
                    }

                    if (!int.TryParse(linesave[0], out int mapid))
                    {
                        continue;
                    }

                    if (!dictionaryId.ContainsKey(mapid))
                    {
                        dictionaryId.Add(mapid, linesave[4]);
                    }
                }

                mapIdStream.Close();
            }

            using (var mapIdLangStream = new StreamReader(fileMapIdLang, Encoding.GetEncoding(1252)))
            {
                while ((line = mapIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length <= 1 || dictionaryIdLang.ContainsKey(linesave[0]))
                    {
                        continue;
                    }

                    dictionaryIdLang.Add(linesave[0], linesave[1]);
                }

                mapIdLangStream.Close();
            }

            foreach (string[] linesave in _packetList.Where(o => o[0].Equals("at")))
            {
                if (linesave.Length <= 7 || linesave[0] != "at")
                {
                    continue;
                }

                if (dictionaryMusic.ContainsKey(int.Parse(linesave[2])))
                {
                    continue;
                }

                dictionaryMusic.Add(int.Parse(linesave[2]), int.Parse(linesave[7]));
            }

            foreach (FileInfo file in new DirectoryInfo(folderMap).GetFiles())
            {
                string name = string.Empty;
                int music = 0;

                if (dictionaryId.ContainsKey(int.Parse(file.Name)) && dictionaryIdLang.ContainsKey(dictionaryId[int.Parse(file.Name)]))
                {
                    name = dictionaryIdLang[dictionaryId[int.Parse(file.Name)]];
                }

                if (dictionaryMusic.ContainsKey(int.Parse(file.Name)))
                {
                    music = dictionaryMusic[int.Parse(file.Name)];
                }

                var map = new MapDTO
                {
                    Name = name,
                    Music = music,
                    MapId = short.Parse(file.Name),
                    Data = File.ReadAllBytes(file.FullName),
                    ShopAllowed = short.Parse(file.Name) == 147
                };
                if (DaoFactory.MapDao.LoadById(map.MapId) != null)
                {
                    continue; // Map already exists in list
                }

                maps.Add(map);
                i++;
            }

            DaoFactory.MapDao.Insert(maps);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MAPS_PARSED"), i));
        }

        public void ImportMapType()
        {
            List<MapTypeDTO> list = DaoFactory.MapTypeDao.LoadAll().ToList();
            var mt1 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act1,
                MapTypeName = "Act1",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt1.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt1);
            }

            var mt2 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act2,
                MapTypeName = "Act2",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt2.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt2);
            }

            var mt3 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act3,
                MapTypeName = "Act3",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt3.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt3);
            }

            var mt4 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act4,
                MapTypeName = "Act4",
                PotionDelay = 5000
            };
            if (list.All(s => s.MapTypeId != mt4.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt4);
            }

            var mt5 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act51,
                MapTypeName = "Act5.1",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct5,
                ReturnMapTypeId = (long)RespawnType.ReturnAct5
            };
            if (list.All(s => s.MapTypeId != mt5.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt5);
            }

            var mt6 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act52,
                MapTypeName = "Act5.2",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct5,
                ReturnMapTypeId = (long)RespawnType.ReturnAct5
            };
            if (list.All(s => s.MapTypeId != mt6.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt6);
            }

            var mt7 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act61,
                MapTypeName = "Act6.1",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct6,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt7.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt7);
            }

            var mt8 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act62,
                MapTypeName = "Act6.2",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct6,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt8.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt8);
            }

            var mt9 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act61A,
                MapTypeName = "Act6.1a", // angel camp
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct6,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt9.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt9);
            }

            var mt10 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act61D,
                MapTypeName = "Act6.1d", // demon camp
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct6,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt10.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt10);
            }

            var mt11 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.CometPlain,
                MapTypeName = "CometPlain",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt11.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt11);
            }

            var mt12 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Mine1,
                MapTypeName = "Mine1",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt12.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt12);
            }

            var mt13 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Mine2,
                MapTypeName = "Mine2",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt13.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt13);
            }

            var mt14 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.MeadowOfMine,
                MapTypeName = "MeadownOfPlain",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt14.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt14);
            }

            var mt15 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.SunnyPlain,
                MapTypeName = "SunnyPlain",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt15.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt15);
            }

            var mt16 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Fernon,
                MapTypeName = "Fernon",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt16.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt16);
            }

            var mt17 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.FernonF,
                MapTypeName = "FernonF",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt17.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt17);
            }

            var mt18 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Cliff,
                MapTypeName = "Cliff",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                ReturnMapTypeId = (long)RespawnType.ReturnAct1
            };
            if (list.All(s => s.MapTypeId != mt18.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt18);
            }

            var mt19 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.LandOfTheDead,
                MapTypeName = "LandOfTheDead",
                PotionDelay = 300
            };
            if (list.All(s => s.MapTypeId != mt19.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt19);
            }

            var mt20 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act32,
                MapTypeName = "Act 3.2",
                PotionDelay = 300
            };
            if (list.All(s => s.MapTypeId != mt20.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt20);
            }

            var mt21 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.CleftOfDarkness,
                MapTypeName = "Cleft of Darkness",
                PotionDelay = 300
            };
            if (list.All(s => s.MapTypeId != mt21.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt21);
            }

            var mt23 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.CitadelAngel,
                MapTypeName = "AngelCitadel",
                PotionDelay = 300
            };
            if (list.All(s => s.MapTypeId != mt23.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt23);
            }

            var mt24 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.CitadelDemon,
                MapTypeName = "DemonCitadel",
                PotionDelay = 300
            };
            if (list.All(s => s.MapTypeId != mt24.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt24);
            }

            var mt25 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Oasis,
                MapTypeName = "Oasis",
                PotionDelay = 300,
                RespawnMapTypeId = (long)RespawnType.DefaultOasis,
                ReturnMapTypeId = (long)RespawnType.DefaultOasis
            };
            if (list.All(s => s.MapTypeId != mt25.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt25);
            }

            var mt26 = new MapTypeDTO
            {
                MapTypeId = (short)MapTypeEnum.Act42,
                MapTypeName = "Act42",
                PotionDelay = 5000
            };
            if (list.All(s => s.MapTypeId != mt26.MapTypeId))
            {
                DaoFactory.MapTypeDao.Insert(ref mt26);
            }

            Logger.Log.Info(Language.Instance.GetMessageFromKey("MAPTYPES_PARSED"));
        }

        public void ImportMapTypeMap()
        {
            List<MapTypeMapDTO> maptypemaps = new List<MapTypeMapDTO>();
            short mapTypeId = 1;
            for (int i = 1; i < 300; i++)
            {
                bool objectset = false;
                if (i < 3 || i > 48 && i < 53 || i > 67 && i < 76 || i == 102 || i > 103 && i < 105 || i > 144 && i < 149)
                {
                    // "act1"
                    mapTypeId = (short)MapTypeEnum.Act1;
                    objectset = true;
                }
                else if (i > 19 && i < 34 || i > 52 && i < 68 || i > 84 && i < 101)
                {
                    // "act2"
                    mapTypeId = (short)MapTypeEnum.Act2;
                    objectset = true;
                }
                else if (i > 40 && i < 45 || i > 45 && i < 48 || i > 99 && i < 102 || i > 104 && i < 128)
                {
                    // "act3"
                    mapTypeId = (short)MapTypeEnum.Act3;
                    objectset = true;
                }
                else if (i == 260)
                {
                    // "act3.2"
                    mapTypeId = (short)MapTypeEnum.Act32;
                    objectset = true;
                }
                else if (i > 129 && i <= 134 || i == 135 || i == 137 || i == 139 || i == 141 || i > 150 && i < 153)
                {
                    // "act4"
                    mapTypeId = (short)MapTypeEnum.Act4;
                    objectset = true;
                }
                else if (i == 153)
                {
                    // "act4.2"
                    mapTypeId = (short)MapTypeEnum.Act42;
                    objectset = true;
                }
                else if (i > 169 && i < 205)
                {
                    // "act5.1"
                    mapTypeId = (short)MapTypeEnum.Act51;
                    objectset = true;
                }
                else if (i > 204 && i < 221)
                {
                    // "act5.2"
                    mapTypeId = (short)MapTypeEnum.Act52;
                    objectset = true;
                }
                else if (i > 228 && i < 233)
                {
                    // "act6.1a"
                    mapTypeId = (short)MapTypeEnum.Act61;
                    objectset = true;
                }
                else if (i > 232 && i < 238)
                {
                    // "act6.1d"
                    mapTypeId = (short)MapTypeEnum.Act61;
                    objectset = true;
                }
                else if (i > 239 && i < 251 || i == 299)
                {
                    // "act6.2"
                    mapTypeId = (short)MapTypeEnum.Act62;
                    objectset = true;
                }
                else if (i > 260 && i < 264 || i > 2614 && i < 2621)
                {
                    // "Oasis"
                    mapTypeId = (short)MapTypeEnum.Oasis;
                    objectset = true;
                }
                else if (i == 103)
                {
                    // "Comet plain"
                    mapTypeId = (short)MapTypeEnum.CometPlain;
                    objectset = true;
                }
                else if (i == 6)
                {
                    // "Mine1"
                    mapTypeId = (short)MapTypeEnum.Mine1;
                    objectset = true;
                }
                else if (i > 6 && i < 9)
                {
                    // "Mine2"
                    mapTypeId = (short)MapTypeEnum.Mine2;
                    objectset = true;
                }
                else if (i == 3)
                {
                    // "Meadown of mine"
                    mapTypeId = (short)MapTypeEnum.MeadowOfMine;
                    objectset = true;
                }
                else if (i == 4)
                {
                    // "Sunny plain"
                    mapTypeId = (short)MapTypeEnum.SunnyPlain;
                    objectset = true;
                }
                else if (i == 5)
                {
                    // "Fernon"
                    mapTypeId = (short)MapTypeEnum.Fernon;
                    objectset = true;
                }
                else if (i > 9 && i < 19 || i > 79 && i < 85)
                {
                    // "FernonF"
                    mapTypeId = (short)MapTypeEnum.FernonF;
                    objectset = true;
                }
                else if (i > 75 && i < 79)
                {
                    // "Cliff"
                    mapTypeId = (short)MapTypeEnum.Cliff;
                    objectset = true;
                }
                else if (i == 150)
                {
                    // "Land of the dead"
                    mapTypeId = (short)MapTypeEnum.LandOfTheDead;
                    objectset = true;
                }
                else if (i == 138)
                {
                    // "Cleft of Darkness"
                    mapTypeId = (short)MapTypeEnum.CleftOfDarkness;
                    objectset = true;
                }
                else if (i == 130)
                {
                    // "Citadel"
                    mapTypeId = (short)MapTypeEnum.CitadelAngel;
                    objectset = true;
                }
                else if (i == 131)
                {
                    mapTypeId = (short)MapTypeEnum.CitadelDemon;
                    objectset = true;
                }

                // add "act6.1a" and "act6.1d" when ids found
                if (objectset && DaoFactory.MapDao.LoadById((short)i) != null && DaoFactory.MapTypeMapDao.LoadByMapAndMapType((short)i, mapTypeId) == null)
                {
                    maptypemaps.Add(new MapTypeMapDTO { MapId = (short)i, MapTypeId = mapTypeId });
                }
            }

            DaoFactory.MapTypeMapDao.Insert(maptypemaps);
        }

        public void ImportMonsters()
        {
            int monsterCounter = 0;
            short map = 0;
            List<int> mobMvPacketsList = new List<int>();
            List<MapMonsterDTO> monsters = new List<MapMonsterDTO>();

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("mv") && o[1].Equals("3")))
            {
                if (!mobMvPacketsList.Contains(Convert.ToInt32(currentPacket[2])))
                {
                    mobMvPacketsList.Add(Convert.ToInt32(currentPacket[2]));
                }
            }

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("in") || o[0].Equals("at")))
            {
                if (currentPacket.Length > 5 && currentPacket[0] == "at")
                {
                    map = short.Parse(currentPacket[2]);
                    continue;
                }

                if (currentPacket.Length <= 7 || currentPacket[0] != "in" || currentPacket[1] != "3")
                {
                    continue;
                }

                var monster = new MapMonsterDTO
                {
                    MapId = map,
                    MonsterVNum = short.Parse(currentPacket[2]),
                    MapMonsterId = int.Parse(currentPacket[3]),
                    MapX = short.Parse(currentPacket[4]),
                    MapY = short.Parse(currentPacket[5]),
                    Position = (byte)(currentPacket[6] == string.Empty ? 0 : byte.Parse(currentPacket[6])),
                    IsDisabled = false
                };
                monster.IsMoving = mobMvPacketsList.Contains(monster.MapMonsterId);

                if (DaoFactory.NpcMonsterDao.LoadByVNum(monster.MonsterVNum) == null || DaoFactory.MapMonsterDao.LoadById(monster.MapMonsterId) != null ||
                    monsters.Count(i => i.MapMonsterId == monster.MapMonsterId) != 0 || monster.MonsterVNum == 860 /* remove Broken Red Plate for quests*/)
                {
                    continue;
                }

                monsters.Add(monster);
                monsterCounter++;
            }

            DaoFactory.MapMonsterDao.Insert(monsters);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MONSTERS_PARSED"), monsterCounter));
        }

        public void ImportNpcMonsterData()
        {
            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("e_info") && o[1].Equals("10")))
            {
                if (currentPacket.Length <= 25)
                {
                    continue;
                }

                NpcMonsterDTO npcMonster = DaoFactory.NpcMonsterDao.LoadByVNum(short.Parse(currentPacket[2]));
                if (npcMonster == null)
                {
                    continue;
                }

                npcMonster.AttackClass = byte.Parse(currentPacket[5]);
                npcMonster.AttackUpgrade = byte.Parse(currentPacket[7]);
                npcMonster.DamageMinimum = short.Parse(currentPacket[8]);
                npcMonster.DamageMaximum = short.Parse(currentPacket[9]);
                npcMonster.Concentrate = short.Parse(currentPacket[10]);
                npcMonster.CriticalChance = byte.Parse(currentPacket[11]);
                npcMonster.CriticalRate = short.Parse(currentPacket[12]);
                npcMonster.DefenceUpgrade = byte.Parse(currentPacket[13]);
                npcMonster.CloseDefence = short.Parse(currentPacket[14]);
                npcMonster.DefenceDodge = short.Parse(currentPacket[15]);
                npcMonster.DistanceDefence = short.Parse(currentPacket[16]);
                npcMonster.DistanceDefenceDodge = short.Parse(currentPacket[17]);
                npcMonster.MagicDefence = short.Parse(currentPacket[18]);
                npcMonster.FireResistance = sbyte.Parse(currentPacket[19]);
                npcMonster.WaterResistance = sbyte.Parse(currentPacket[20]);
                npcMonster.LightResistance = sbyte.Parse(currentPacket[21]);
                npcMonster.DarkResistance = sbyte.Parse(currentPacket[22]);

                DaoFactory.NpcMonsterDao.InsertOrUpdate(ref npcMonster);
            }
        }

        public void ImportNpcMonsters()
        {
            int[] basicHp = new int[100];
            int[] basicPrimaryMp = new int[100];
            int[] basicSecondaryMp = new int[100];
            int[] basicXp = new int[100];
            int[] basicJXp = new int[100];

            // basicHpLoad
            int baseHp = 138;
            int HPbasup = 18;
            for (int i = 0; i < 100; i++)
            {
                basicHp[i] = baseHp;
                HPbasup++;
                baseHp += HPbasup;

                if (i == 37)
                {
                    baseHp = 1765;
                    HPbasup = 65;
                }

                if (i < 41)
                {
                    continue;
                }

                if (((99 - i) % 8) == 0)
                {
                    HPbasup++;
                }
            }

            //Race == 0
            basicPrimaryMp[0] = 10;
            basicPrimaryMp[1] = 10;
            basicPrimaryMp[2] = 15;

            int primaryBasup = 5;
            byte count = 0;
            bool isStable = true;
            bool isDouble = false;

            for (int i = 3; i < 100; i++)
            {
                if ((i % 10) == 1)
                {
                    basicPrimaryMp[i] += basicPrimaryMp[i - 1] + primaryBasup * 2;
                    continue;
                }

                if (!isStable)
                {
                    primaryBasup++;
                    count++;

                    if (count == 2)
                    {
                        if (isDouble)
                        {
                            isDouble = false;
                        }
                        else
                        {
                            isStable = true;
                            isDouble = true;
                            count = 0;
                        }
                    }

                    if (count == 4)
                    {
                        isStable = true;
                        count = 0;
                    }
                }
                else
                {
                    count++;
                    if (count == 2)
                    {
                        isStable = false;
                        count = 0;
                    }
                }

                basicPrimaryMp[i] = basicPrimaryMp[i - ((i % 10) == 2 ? 2 : 1)] + primaryBasup;
            }

            // Race == 2
            basicSecondaryMp[0] = 60;
            basicSecondaryMp[1] = 60;
            basicSecondaryMp[2] = 78;

            int secondaryBasup = 18;
            bool boostup = false;

            for (int i = 3; i < 100; i++)
            {
                if ((i % 10) == 1)
                {
                    basicSecondaryMp[i] += basicSecondaryMp[i - 1] + i + 10;
                    continue;
                }

                if (boostup)
                {
                    secondaryBasup += 3;
                    boostup = false;
                }
                else
                {
                    secondaryBasup++;
                    boostup = true;
                }

                basicSecondaryMp[i] = basicSecondaryMp[i - ((i % 10) == 2 ? 2 : 1)] + secondaryBasup;
            }

            // basicXPLoad
            for (int i = 0; i < 100; i++)
            {
                basicXp[i] = i * 180;
            }

            // basicJXpLoad
            for (int i = 0; i < 100; i++)
            {
                basicJXp[i] = 360;
            }

            string fileNpcId = $"{_folder}\\monster.dat";
            string fileNpcLang = $"{_folder}\\_code_{ConfigurationManager.AppSettings["Language"]}_monster.txt";
            List<NpcMonsterDTO> npcs = new List<NpcMonsterDTO>();

            // Store like this: (vnum, (name, level))
            Dictionary<string, string> dictionaryIdLang = new Dictionary<string, string>();
            var npc = new NpcMonsterDTO();
            List<DropDTO> drops = new List<DropDTO>();
            List<BCardDTO> monstercards = new List<BCardDTO>();
            List<NpcMonsterSkillDTO> skills = new List<NpcMonsterSkillDTO>();
            string line;
            bool itemAreaBegin = false;
            int counter = 0;
            long unknownData = 0;
            using (var npcIdLangStream = new StreamReader(fileNpcLang, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length > 1 && !dictionaryIdLang.ContainsKey(linesave[0]))
                    {
                        dictionaryIdLang.Add(linesave[0], linesave[1]);
                    }
                }

                npcIdLangStream.Close();
            }

            using (var npcIdStream = new StreamReader(fileNpcId, Encoding.GetEncoding(1252)))
            {
                while ((line = npcIdStream.ReadLine()) != null)
                {
                    string[] currentLine = line.Split('\t');

                    if (currentLine.Length > 2 && currentLine[1] == "VNUM")
                    {
                        npc = new NpcMonsterDTO
                        {
                            NpcMonsterVNum = Convert.ToInt16(currentLine[2])
                        };
                        itemAreaBegin = true;
                        unknownData = 0;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "NAME")
                    {
                        npc.Name = dictionaryIdLang.ContainsKey(currentLine[2]) ? dictionaryIdLang[currentLine[2]] : string.Empty;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "LEVEL")
                    {
                        if (!itemAreaBegin)
                        {
                            continue;
                        }

                        npc.Level = Convert.ToByte(currentLine[2]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "RACE")
                    {
                        npc.Race = Convert.ToByte(currentLine[2]);
                        npc.RaceType = Convert.ToByte(currentLine[3]);
                    }
                    else if (currentLine.Length > 7 && currentLine[1] == "ATTRIB")
                    {
                        npc.Element = Convert.ToByte(currentLine[2]);
                        npc.ElementRate = Convert.ToInt16(currentLine[3]);
                        npc.FireResistance = Convert.ToSByte(currentLine[4]);
                        npc.WaterResistance = Convert.ToSByte(currentLine[5]);
                        npc.LightResistance = Convert.ToSByte(currentLine[6]);
                        npc.DarkResistance = Convert.ToSByte(currentLine[7]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "HP/MP")
                    {
                        npc.MaxHP = Convert.ToInt32(currentLine[2]) + basicHp[npc.Level];
                        npc.MaxMP = (Convert.ToInt32(currentLine[3]) + npc.Race) == 0 ? basicPrimaryMp[npc.Level] : basicSecondaryMp[npc.Level];
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "EXP")
                    {
                        npc.XP = Math.Abs(Convert.ToInt32(currentLine[2]) + basicXp[npc.Level]);
                        npc.JobXP = Convert.ToInt32(currentLine[3]) + basicJXp[npc.Level];
                        switch (npc.NpcMonsterVNum)
                        {
                            case 2500:
                                npc.HeroXp = 879;
                                break;

                            case 2501:
                                npc.HeroXp = 881;
                                break;

                            case 2502:
                                npc.HeroXp = 884;
                                break;

                            case 2503:
                                npc.HeroXp = 1013;
                                break;

                            case 2505:
                                npc.HeroXp = 871;
                                break;

                            case 2506:
                                npc.HeroXp = 765;
                                break;

                            case 2507:
                                npc.HeroXp = 803;
                                break;

                            case 2508:
                                npc.HeroXp = 825;
                                break;

                            case 2509:
                                npc.HeroXp = 789;
                                break;

                            case 2510:
                                npc.HeroXp = 881;
                                break;

                            case 2511:
                                npc.HeroXp = 879;
                                break;

                            case 2512:
                                npc.HeroXp = 884;
                                break;

                            case 2513:
                                npc.HeroXp = 1075;
                                break;

                            case 2515:
                                npc.HeroXp = 3803;
                                break;

                            case 2516:
                                npc.HeroXp = 836;
                                break;

                            case 2517:
                                npc.HeroXp = 450;
                                break;

                            case 2518:
                                npc.HeroXp = 911;
                                break;

                            case 2519:
                                npc.HeroXp = 845;
                                break;

                            case 2520:
                                npc.HeroXp = 3682;
                                break;

                            case 2521:
                                npc.HeroXp = 401;
                                break;

                            case 2522:
                                npc.HeroXp = 471;
                                break;

                            case 2523:
                                npc.HeroXp = 328;
                                break;

                            case 2524:
                                npc.HeroXp = 12718;
                                break;

                            case 2525:
                                npc.HeroXp = 412;
                                break;

                            case 2526:
                                npc.HeroXp = 11157;
                                break;

                            case 2527:
                                npc.HeroXp = 18057;
                                break;

                            case 2530:
                                npc.HeroXp = 28756;
                                break;

                            case 2559:
                                npc.HeroXp = 1308;
                                break;

                            case 2560:
                                npc.HeroXp = 1234;
                                break;

                            case 2561:
                                npc.HeroXp = 1168;
                                break;

                            case 2562:
                                npc.HeroXp = 959;
                                break;

                            case 2563:
                                npc.HeroXp = 947;
                                break;

                            case 2564:
                                npc.HeroXp = 952;
                                break;

                            case 2566:
                                npc.HeroXp = 1097;
                                break;

                            case 2567:
                                npc.HeroXp = 1096;
                                break;

                            case 2568:
                                npc.HeroXp = 4340;
                                break;

                            case 2569:
                                npc.HeroXp = 3534;
                                break;

                            case 2570:
                                npc.HeroXp = 4343;
                                break;

                            case 2571:
                                npc.HeroXp = 2205;
                                break;

                            case 2572:
                                npc.HeroXp = 5632;
                                break;

                            case 2573:
                                npc.HeroXp = 3756;
                                break;

                            /*
                             * percent damage monsters
                             */
                            case 2309: // Foxy
                                npc.IsPercent = true;
                                npc.TakeDamages = 193;
                                npc.GiveDamagePercentage = 50;
                                break;

                            case 2314: // renard enragé
                                npc.IsPercent = true;
                                npc.TakeDamages = 3666;
                                npc.GiveDamagePercentage = 10;
                                break;

                            case 2315: // renard dusi enragé
                                npc.IsPercent = true;
                                npc.TakeDamages = 3948;
                                npc.GiveDamagePercentage = 10;
                                break;

                            case 1381: // Jack o lantern
                                npc.IsPercent = true;
                                npc.TakeDamages = 600;
                                npc.GiveDamagePercentage = 20;
                                break;

                            case 2316: // Maru
                                npc.IsPercent = true;
                                npc.TakeDamages = 193;
                                npc.GiveDamagePercentage = 50;
                                break;

                            case 1500: // Pete o peng
                                npc.IsPercent = true;
                                npc.TakeDamages = 338;
                                npc.GiveDamagePercentage = 20;
                                break;

                            case 774: // Reine poule
                                npc.IsPercent = true;
                                npc.TakeDamages = 338;
                                npc.GiveDamagePercentage = 20;
                                break;

                            case 2331: // Hongbi
                                npc.IsPercent = true;
                                npc.TakeDamages = 676;
                                npc.GiveDamagePercentage = 30;
                                break;

                            case 2332: // Cheongbi
                                npc.IsPercent = true;
                                npc.TakeDamages = 507;
                                npc.GiveDamagePercentage = 30;
                                break;

                            case 2357: // Lola longoreil
                                npc.IsPercent = true;
                                npc.TakeDamages = 193;
                                npc.GiveDamagePercentage = 50;
                                break;

                            case 1922: // Oeuf valak
                                npc.IsPercent = true;
                                npc.TakeDamages = 9678;
                                npc.MaxHP = 193560;
                                npc.GiveDamagePercentage = 0;
                                break;

                            case 532: // Tete de bonhomme de neige geant
                                npc.IsPercent = true;
                                npc.TakeDamages = 193;
                                npc.GiveDamagePercentage = 50;
                                break;

                            case 531: // Bonhomme de neige
                                npc.IsPercent = true;
                                npc.TakeDamages = 392;
                                npc.GiveDamagePercentage = 10;
                                break;

                            case 796: // Roi poulet
                                npc.IsPercent = true;
                                npc.TakeDamages = 200;
                                npc.GiveDamagePercentage = 20;
                                break;

                            case 2639: // Yertirand
                                npc.IsPercent = true;
                                npc.TakeDamages = 666;
                                npc.GiveDamagePercentage = 0;
                                break;

                            default:
                                npc.HeroXp = 0;
                                break;
                        }
                    }
                    else if (currentLine.Length > 6 && currentLine[1] == "PREATT")
                    {
                        npc.IsHostile = currentLine[2] != "0";
                        npc.NoticeRange = Convert.ToByte(currentLine[4]);
                        npc.Speed = Convert.ToByte(currentLine[5]);
                        npc.RespawnTime = Convert.ToInt32(currentLine[6]);
                    }
                    else if (currentLine.Length > 6 && currentLine[1] == "WEAPON")
                    {
                        switch (currentLine[3])
                        {
                            case "1":
                                npc.DamageMinimum = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 4 + 32 + Convert.ToInt16(currentLine[4]) +
                                    Math.Round(Convert.ToDecimal((npc.Level - 1) / 5)));
                                npc.DamageMaximum = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 6 + 40 + Convert.ToInt16(currentLine[5]) -
                                    Math.Round(Convert.ToDecimal((npc.Level - 1) / 5)));
                                npc.Concentrate = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 5 + 27 + Convert.ToInt16(currentLine[6]));
                                npc.CriticalChance = Convert.ToByte(4 + Convert.ToInt16(currentLine[7]));
                                npc.CriticalRate = Convert.ToInt16(70 + Convert.ToInt16(currentLine[8]));
                                break;
                            case "2":
                                npc.DamageMinimum = Convert.ToInt16(Convert.ToInt16(currentLine[2]) * 6.5f + 23 + Convert.ToInt16(currentLine[4]));
                                npc.DamageMaximum = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 8 + 38 + Convert.ToInt16(currentLine[5]));
                                npc.Concentrate = Convert.ToInt16(70 + Convert.ToInt16(currentLine[6]));
                                break;
                        }
                    }
                    else if (currentLine.Length > 6 && currentLine[1] == "ARMOR")
                    {
                        npc.CloseDefence = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 2 + 18);
                        npc.DistanceDefence = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 3 + 17);
                        npc.MagicDefence = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 2 + 13);
                        npc.DefenceDodge = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 5 + 31);
                        npc.DistanceDefenceDodge = Convert.ToInt16((Convert.ToInt16(currentLine[2]) - 1) * 5 + 31);
                    }
                    else if (currentLine.Length > 7 && currentLine[1] == "ETC")
                    {
                        unknownData = Convert.ToInt64(currentLine[2]);
                        switch (unknownData)
                        {
                            case -2147481593:
                                npc.MonsterType = MonsterType.Special;
                                break;
                            case -2147483616:
                            case -2147483647:
                            case -2147483646:
                                if (npc.Race == 8 && npc.RaceType == 0)
                                {
                                    npc.NoAggresiveIcon = true;
                                }
                                else
                                {
                                    npc.NoAggresiveIcon = false;
                                }

                                break;
                        }

                        if (npc.NpcMonsterVNum >= 588 && npc.NpcMonsterVNum <= 607)
                        {
                            npc.MonsterType = MonsterType.Elite;
                        }
                    }
                    else if (currentLine.Length > 6 && currentLine[1] == "SETTING")
                    {
                        if (currentLine[4] == "0")
                        {
                            continue;
                        }

                        npc.VNumRequired = Convert.ToInt16(currentLine[4]);
                        npc.AmountRequired = 1;
                    }
                    else if (currentLine.Length > 4 && currentLine[1] == "PETINFO")
                    {
                        if (npc.VNumRequired != 0 || unknownData != -2147481593 && unknownData != -2147481599 && unknownData != -1610610681)
                        {
                            continue;
                        }

                        npc.VNumRequired = Convert.ToInt16(currentLine[2]);
                        npc.AmountRequired = Convert.ToByte(currentLine[3]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "EFF")
                    {
                        npc.BasicSkill = Convert.ToInt16(currentLine[2]);
                    }
                    else if (currentLine.Length > 8 && currentLine[1] == "ZSKILL")
                    {
                        npc.AttackClass = Convert.ToByte(currentLine[2]);
                        npc.BasicRange = Convert.ToByte(currentLine[3]);
                        npc.BasicArea = Convert.ToByte(currentLine[5]);
                        npc.BasicCooldown = Convert.ToInt16(currentLine[6]);
                    }
                    else if (currentLine.Length > 4 && currentLine[1] == "WINFO")
                    {
                        npc.AttackUpgrade = Convert.ToByte(unknownData == 1 ? currentLine[2] : currentLine[4]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "AINFO")
                    {
                        npc.DefenceUpgrade = Convert.ToByte(unknownData == 1 ? currentLine[2] : currentLine[3]);
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "SKILL")
                    {
                        for (int i = 2; i < currentLine.Length - 3; i += 3)
                        {
                            short vnum = short.Parse(currentLine[i]);
                            if (vnum == -1 || vnum == 0)
                            {
                                break;
                            }

                            if (DaoFactory.SkillDao.LoadById(vnum) == null || DaoFactory.NpcMonsterSkillDao.LoadByNpcMonster(npc.NpcMonsterVNum).Count(s => s.SkillVNum == vnum) != 0)
                            {
                                continue;
                            }

                            skills.Add(new NpcMonsterSkillDTO
                            {
                                SkillVNum = vnum,
                                Rate = Convert.ToInt16(currentLine[i + 1]),
                                NpcMonsterVNum = npc.NpcMonsterVNum
                            });
                        }
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "CARD")
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            byte type = (byte)int.Parse(currentLine[5 * i + 2]);
                            if (type == 0 || type == 255)
                            {
                                continue;
                            }

                            int first = int.Parse(currentLine[5 * i + 3]);
                            var itemCard = new BCardDTO
                            {
                                NpcMonsterVNum = npc.NpcMonsterVNum,
                                Type = type,
                                SubType = (byte)(int.Parse(currentLine[5 * i + 5]) + 1 * 10 + 1 + (first > 0 ? 0 : 1)),
                                IsLevelScaled = Convert.ToBoolean(first % 4),
                                IsLevelDivided = (first % 4) == 2,
                                FirstData = (short)((first > 0 ? first : -first) / 4),
                                SecondData = (short)(int.Parse(currentLine[5 * i + 4]) / 4),
                                ThirdData = (short)(int.Parse(currentLine[5 * i + 6]) / 4)
                            };
                            monstercards.Add(itemCard);
                        }
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "BASIC")
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            byte type = (byte)int.Parse(currentLine[5 * i + 2]);
                            if (type == 0)
                            {
                                continue;
                            }

                            int first = int.Parse(currentLine[5 * i + 5]);
                            var itemCard = new BCardDTO
                            {
                                NpcMonsterVNum = npc.NpcMonsterVNum,
                                Type = type,
                                SubType = (byte)((int.Parse(currentLine[5 * i + 6]) + 1) * 10 + 1 + (first > 0 ? 0 : 1)),
                                FirstData = (short)((first > 0 ? first : -first) / 4),
                                SecondData = (short)(int.Parse(currentLine[5 * i + 4]) / 4),
                                ThirdData = (short)(int.Parse(currentLine[5 * i + 3]) / 4),
                                CastType = 1,
                                IsLevelScaled = false,
                                IsLevelDivided = false
                            };
                            monstercards.Add(itemCard);
                        }
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "ITEM")
                    {
                        if (DaoFactory.NpcMonsterDao.LoadByVNum(npc.NpcMonsterVNum) == null)
                        {
                            npcs.Add(npc);
                            counter++;
                        }

                        for (int i = 2; i < currentLine.Length - 3; i += 3)
                        {
                            short vnum = Convert.ToInt16(currentLine[i]);
                            if (vnum == -1)
                            {
                                break;
                            }

                            if (DaoFactory.DropDao.LoadByMonster(npc.NpcMonsterVNum).Count(s => s.ItemVNum == vnum) != 0)
                            {
                                continue;
                            }

                            drops.Add(new DropDTO
                            {
                                ItemVNum = vnum,
                                Amount = Convert.ToInt32(currentLine[i + 2]),
                                MonsterVNum = npc.NpcMonsterVNum,
                                DropChance = Convert.ToInt32(currentLine[i + 1])
                            });
                        }

                        itemAreaBegin = false;
                    }
                }

                DaoFactory.NpcMonsterDao.Insert(npcs);
                DaoFactory.NpcMonsterSkillDao.Insert(skills);
                DaoFactory.BCardDao.Insert(monstercards);
                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("NPCMONSTERS_PARSED"), counter));
                npcIdStream.Close();
            }

            // Act 1
            drops.Add(new DropDTO { ItemVNum = 1002, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act1 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 12000, MapTypeId = (short)MapTypeEnum.Act1 });
            drops.Add(new DropDTO { ItemVNum = 2015, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act1 });
            drops.Add(new DropDTO { ItemVNum = 2016, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act1 });
            drops.Add(new DropDTO { ItemVNum = 2023, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act1 });
            drops.Add(new DropDTO { ItemVNum = 2024, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act1 });
            drops.Add(new DropDTO { ItemVNum = 2028, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act1 });

            // Act2
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 7000, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1028, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1237, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1239, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 900, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 900, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 900, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 900, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 900, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 250, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act2 });
            drops.Add(new DropDTO { ItemVNum = 5853, Amount = 1, MonsterVNum = null, DropChance = 80, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 5854, Amount = 1, MonsterVNum = null, DropChance = 80, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 5855, Amount = 1, MonsterVNum = null, DropChance = 80, MapTypeId = (short)MapTypeEnum.Oasis });

            // Act3
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 8000, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1235, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1237, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1238, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1239, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1240, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 4000, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 350, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2285, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 5853, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 5854, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });
            drops.Add(new DropDTO { ItemVNum = 5855, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act3 });

            // Act3.2 (Midgard)
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 6000, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 250, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1235, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1237, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1238, Amount = 1, MonsterVNum = null, DropChance = 20, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1239, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1240, Amount = 1, MonsterVNum = null, DropChance = 20, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 60, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 40, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 60, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 40, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 3500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2285, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2600, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 2605, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 5857, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 5853, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 5854, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act32 });
            drops.Add(new DropDTO { ItemVNum = 5855, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act32 });


            // Act 3.4 Oasis 
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 7000, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1235, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1237, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1238, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1239, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1240, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 3000, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2285, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 5853, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 5854, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 5855, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });
            drops.Add(new DropDTO { ItemVNum = 5999, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Oasis });

            // Act4
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1010, Amount = 3, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 2, MonsterVNum = null, DropChance = 3000, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 3, MonsterVNum = null, DropChance = 3000, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 3, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1246, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1247, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1248, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 1429, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 2307, Amount = 1, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act4 });
            drops.Add(new DropDTO { ItemVNum = 2308, Amount = 1, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act4 });

            //Act4.2
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 1010, Amount = 3, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 2, MonsterVNum = null, DropChance = 3000, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 3, MonsterVNum = null, DropChance = 3000, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 3, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 1246, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 1247, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 1248, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 1429, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 2307, Amount = 1, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 2308, Amount = 1, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 2445, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 2448, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 2449, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 2450, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 2451, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act42 });
            drops.Add(new DropDTO { ItemVNum = 5986, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act42 });


            // Act5
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 6000, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1872, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1873, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 1874, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 2500, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2285, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2351, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 2379, Amount = 1, MonsterVNum = null, DropChance = 1000, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 5853, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 5854, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act51 });
            drops.Add(new DropDTO { ItemVNum = 5855, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act51 });

            // Act5.2
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 5000, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 1500, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2379, Amount = 1, MonsterVNum = null, DropChance = 3000, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 2380, Amount = 1, MonsterVNum = null, DropChance = 6000, MapTypeId = (short)MapTypeEnum.Act52 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act52 });

            // Act6.1 Angel
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 1010, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 5000, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 1028, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 2000, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2285, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2446, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2806, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2807, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2813, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2815, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2816, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2818, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 2819, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 5853, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 5854, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 5855, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act61A });
            drops.Add(new DropDTO { ItemVNum = 5880, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act61A });

            // Act6.1 Demon
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 1010, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 5000, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 1028, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2282, Amount = 1, MonsterVNum = null, DropChance = 2000, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2283, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2284, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2285, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2446, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2806, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2807, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2813, Amount = 1, MonsterVNum = null, DropChance = 150, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2815, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2816, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2818, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 2819, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 5853, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 5854, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 5855, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act61D });
            drops.Add(new DropDTO { ItemVNum = 5881, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act61D });

            // Act6.2
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1010, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act61 });
            drops.Add(new DropDTO { ItemVNum = 1010, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 6000, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1028, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1086, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1191, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1192, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1193, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 1194, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2129, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2452, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2453, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2454, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2455, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 2456, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 5853, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 5854, Amount = 1, MonsterVNum = null, DropChance = 50, MapTypeId = (short)MapTypeEnum.Act62 });
            drops.Add(new DropDTO { ItemVNum = 5855, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Act62 });

            // Comet plain
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 7000, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.CometPlain });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.CometPlain });

            // Mine1
            drops.Add(new DropDTO { ItemVNum = 1002, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Mine1 });
            drops.Add(new DropDTO { ItemVNum = 1005, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Mine1 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 11000, MapTypeId = (short)MapTypeEnum.Mine1 });

            // Mine2
            drops.Add(new DropDTO { ItemVNum = 1002, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 1005, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 11000, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 1241, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.Mine2 });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Mine2 });

            // MeadownOfMine
            drops.Add(new DropDTO { ItemVNum = 1002, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 1005, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 10000, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 2016, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 2023, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 2024, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 2028, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.MeadowOfMine });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.MeadowOfMine });

            // SunnyPlain
            drops.Add(new DropDTO { ItemVNum = 1003, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1006, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 8000, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2118, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.SunnyPlain });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.SunnyPlain });

            // Fernon
            drops.Add(new DropDTO { ItemVNum = 1003, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1006, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 9000, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Fernon });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Fernon });

            // FernonF
            drops.Add(new DropDTO { ItemVNum = 1004, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 9000, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1092, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1093, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 1094, Amount = 1, MonsterVNum = null, DropChance = 500, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 200, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2114, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2115, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2116, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2117, Amount = 1, MonsterVNum = null, DropChance = 700, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2205, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2206, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2207, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2208, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.FernonF });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.FernonF });

            // Cliff
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 8000, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2098, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2099, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2100, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2101, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2102, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 2296, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.Cliff });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 30, MapTypeId = (short)MapTypeEnum.Cliff });

            // LandOfTheDead
            drops.Add(new DropDTO { ItemVNum = 1007, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1010, Amount = 1, MonsterVNum = null, DropChance = 800, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1012, Amount = 1, MonsterVNum = null, DropChance = 8000, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1015, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1016, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1078, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1114, Amount = 1, MonsterVNum = null, DropChance = 400, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1019, Amount = 1, MonsterVNum = null, DropChance = 2000, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1020, Amount = 1, MonsterVNum = null, DropChance = 1200, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1021, Amount = 1, MonsterVNum = null, DropChance = 600, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1022, Amount = 1, MonsterVNum = null, DropChance = 300, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 1211, Amount = 1, MonsterVNum = null, DropChance = 250, MapTypeId = (short)MapTypeEnum.LandOfTheDead });
            drops.Add(new DropDTO { ItemVNum = 5119, Amount = 1, MonsterVNum = null, DropChance = 100, MapTypeId = (short)MapTypeEnum.LandOfTheDead });

            DaoFactory.DropDao.Insert(drops);
        }

        public void ImportPackets()
        {
            string filePacket = $"{_folder}\\packet.txt";
            using (var packetTxtStream = new StreamReader(filePacket, Encoding.GetEncoding(1252)))
            {
                string line;
                while ((line = packetTxtStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split(' ');
                    _packetList.Add(linesave);
                }
            }
        }

        public void ImportPortals()
        {
            List<PortalDTO> listPortals1 = new List<PortalDTO>();
            List<PortalDTO> listPortals2 = new List<PortalDTO>();
            short map = 0;

            var lodPortal = new PortalDTO
            {
                SourceMapId = 150,
                SourceX = 172,
                SourceY = 171,
                DestinationMapId = 98,
                Type = -1,
                DestinationX = 6,
                DestinationY = 36,
                IsDisabled = false
            };
            DaoFactory.PortalDao.Insert(lodPortal);

            var minilandPortal = new PortalDTO
            {
                SourceMapId = 20001,
                SourceX = 3,
                SourceY = 8,
                DestinationMapId = 1,
                Type = -1,
                DestinationX = 48,
                DestinationY = 132,
                IsDisabled = false
            };
            DaoFactory.PortalDao.Insert(minilandPortal);

            var weddingPortal = new PortalDTO
            {
                SourceMapId = 2586,
                SourceX = 34,
                SourceY = 54,
                DestinationMapId = 145,
                Type = -1,
                DestinationX = 61,
                DestinationY = 165,
                IsDisabled = false
            };
            DaoFactory.PortalDao.Insert(weddingPortal);

            var glacerusCavernPortal = new PortalDTO
            {
                SourceMapId = 2587,
                SourceX = 42,
                SourceY = 3,
                DestinationMapId = 189,
                Type = -1,
                DestinationX = 48,
                DestinationY = 156,
                IsDisabled = false
            };
            DaoFactory.PortalDao.Insert(glacerusCavernPortal);

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("at") || o[0].Equals("gp")))
            {
                if (currentPacket.Length > 5 && currentPacket[0] == "at")
                {
                    map = short.Parse(currentPacket[2]);
                    continue;
                }

                if (currentPacket.Length > 4 && currentPacket[0] == "gp")
                {
                    var portal = new PortalDTO
                    {
                        SourceMapId = map,
                        SourceX = short.Parse(currentPacket[1]),
                        SourceY = short.Parse(currentPacket[2]),
                        DestinationMapId = short.Parse(currentPacket[3]),
                        Type = sbyte.Parse(currentPacket[4]),
                        DestinationX = -1,
                        DestinationY = -1,
                        IsDisabled = false
                    };

                    if (listPortals1.Any(s => s.SourceMapId == map && s.SourceX == portal.SourceX && s.SourceY == portal.SourceY && s.DestinationMapId == portal.DestinationMapId) ||
                        _maps.All(s => s.MapId != portal.SourceMapId) || _maps.All(s => s.MapId != portal.DestinationMapId))
                    {
                        // Portal already in list
                        continue;
                    }

                    listPortals1.Add(portal);
                }
            }

            listPortals1 = listPortals1.OrderBy(s => s.SourceMapId).ThenBy(s => s.DestinationMapId).ThenBy(s => s.SourceY).ThenBy(s => s.SourceX).ToList();
            foreach (PortalDTO portal in listPortals1)
            {
                PortalDTO p = listPortals1.Except(listPortals2).FirstOrDefault(s => s.SourceMapId == portal.DestinationMapId && s.DestinationMapId == portal.SourceMapId);
                if (p == null)
                {
                    continue;
                }

                portal.DestinationX = p.SourceX;
                portal.DestinationY = p.SourceY;
                p.DestinationY = portal.SourceY;
                p.DestinationX = portal.SourceX;
                listPortals2.Add(p);
                listPortals2.Add(portal);
            }

            // foreach portal in the new list of Portals where none (=> !Any()) are found in the existing
            int portalCounter = listPortals2.Count(portal => !DaoFactory.PortalDao.LoadByMap(portal.SourceMapId).Any(
                s => s.DestinationMapId == portal.DestinationMapId && s.SourceX == portal.SourceX && s.SourceY == portal.SourceY));

            // so this dude doesnt exist yet in DAOFactory -> insert it
            DaoFactory.PortalDao.Insert(listPortals2.Where(portal => !DaoFactory.PortalDao.LoadByMap(portal.SourceMapId).Any(
                s => s.DestinationMapId == portal.DestinationMapId && s.SourceX == portal.SourceX && s.SourceY == portal.SourceY)).ToList());

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("PORTALS_PARSED"), portalCounter));
        }

        public void ImportHardcodedItemRecipes()
        {
            // Production Tools for Adventurers
            InsertRecipe(6, 1035, 1, new short[] { 1027, 10, 2038, 8, 1035, 1 });
            InsertRecipe(16, 1035, 1, new short[] { 1027, 8, 2042, 6, 1035, 1 });
            InsertRecipe(204, 1035, 1, new short[] { 1027, 15, 2042, 10, 1035, 1 });
            InsertRecipe(206, 1035, 1, new short[] { 1027, 8, 2046, 7, 1035, 1 });
            InsertRecipe(501, 1035, 1, new short[] { 1027, 14, 500, 1, 1035, 1 });

            // Production Tools for Swordsmen
            InsertRecipe(22, 1039, 1, new short[] { 1027, 30, 2035, 32, 1039, 1 });
            InsertRecipe(26, 1039, 1, new short[] { 1027, 43, 2035, 44, 1039, 1 });
            InsertRecipe(30, 1039, 1, new short[] { 1027, 54, 2036, 56, 1039, 1 });
            InsertRecipe(73, 1039, 1, new short[] { 1027, 33, 2035, 10, 2039, 23, 1039, 1 });
            InsertRecipe(76, 1039, 1, new short[] { 1027, 53, 2036, 14, 2040, 39, 1039, 1 });
            InsertRecipe(96, 1039, 1, new short[] { 1027, 20, 2034, 6, 2046, 14, 1039, 1 });
            InsertRecipe(100, 1039, 1, new short[] { 1027, 35, 2035, 12, 2047, 23, 1039, 1 });
            InsertRecipe(104, 1039, 1, new short[] { 1027, 53, 2036, 18, 2048, 35, 1039, 1 });

            // Production Tools for Archers
            InsertRecipe(36, 1040, 1, new short[] { 1027, 30, 2039, 32, 1040, 1 });
            InsertRecipe(40, 1040, 1, new short[] { 1027, 43, 2039, 35, 1040, 1 });
            InsertRecipe(44, 1040, 1, new short[] { 1027, 54, 2040, 56, 1040, 1 });
            InsertRecipe(81, 1040, 1, new short[] { 1027, 33, 2035, 32, 1040, 1 });
            InsertRecipe(84, 1040, 1, new short[] { 1027, 53, 2036, 54, 1040, 1 });
            InsertRecipe(109, 1040, 1, new short[] { 1027, 20, 2042, 8, 2046, 12, 1040, 1 });
            InsertRecipe(113, 1040, 1, new short[] { 1027, 35, 2043, 13, 2047, 22, 1040, 1 });
            InsertRecipe(117, 1040, 1, new short[] { 1027, 53, 2044, 20, 2048, 33, 1040, 1 });

            // Production Tools for Sorcerers
            InsertRecipe(50, 1041, 1, new short[] { 1027, 30, 2039, 32, 1041, 1 });
            InsertRecipe(54, 1041, 1, new short[] { 1027, 43, 2039, 45, 1041, 1 });
            InsertRecipe(58, 1041, 1, new short[] { 1027, 54, 2040, 56, 1041, 1 });
            InsertRecipe(89, 1041, 1, new short[] { 1027, 33, 2035, 34, 1041, 1 });
            InsertRecipe(92, 1041, 1, new short[] { 1027, 53, 2036, 54, 1041, 1 });
            InsertRecipe(122, 1041, 1, new short[] { 1027, 20, 2042, 14, 2046, 6, 1041, 1 });
            InsertRecipe(126, 1041, 1, new short[] { 1027, 35, 2043, 28, 2047, 7, 1041, 1 });
            InsertRecipe(130, 1041, 1, new short[] { 1027, 53, 2044, 42, 2048, 11, 1041, 1 });

            // Production Tools for Accessories
            InsertRecipe(508, 1047, 1, new short[] { 1027, 24, 1032, 5, 1047, 1 });
            InsertRecipe(509, 1047, 1, new short[] { 1027, 25, 1031, 5, 1047, 1 });
            InsertRecipe(510, 1047, 1, new short[] { 1027, 26, 1031, 7, 1047, 1 });
            InsertRecipe(514, 1047, 1, new short[] { 1027, 33, 1033, 10, 1047, 1 });
            InsertRecipe(516, 1047, 1, new short[] { 1027, 35, 1032, 12, 1047, 1 });
            InsertRecipe(517, 1047, 1, new short[] { 1027, 36, 1034, 15, 1047, 1 });
            InsertRecipe(522, 1047, 1, new short[] { 1027, 43, 1033, 20, 1047, 1 });
            InsertRecipe(523, 1047, 1, new short[] { 1027, 44, 1031, 24, 1047, 1 });
            InsertRecipe(525, 1047, 1, new short[] { 1027, 46, 1034, 28, 1047, 1 });
            InsertRecipe(531, 1047, 1, new short[] { 1027, 54, 1032, 36, 1047, 1 });
            InsertRecipe(534, 1047, 1, new short[] { 1027, 57, 1033, 42, 1047, 1 });

            // Production Tools for Gems, Cellons and Crystals
            InsertRecipe(1016, 1072, 1, new short[] { 1014, 99, 1015, 5, 1072, 1 });
            InsertRecipe(1018, 1072, 1, new short[] { 1014, 5, 1017, 5, 1072, 1 });
            InsertRecipe(1019, 1072, 1, new short[] { 1014, 10, 1018, 5, 1072, 1 });
            InsertRecipe(1020, 1072, 1, new short[] { 1014, 17, 1019, 5, 1072, 1 });
            InsertRecipe(1021, 1072, 1, new short[] { 1014, 25, 1020, 5, 1072, 1 });
            InsertRecipe(1022, 1072, 1, new short[] { 1014, 35, 1021, 5, 1072, 1 });
            InsertRecipe(1023, 1072, 1, new short[] { 1014, 50, 1022, 5, 1072, 1 });
            InsertRecipe(1024, 1072, 1, new short[] { 1014, 75, 1023, 5, 1072, 1 });
            InsertRecipe(1025, 1072, 1, new short[] { 1014, 110, 1024, 5, 1072, 1 });
            InsertRecipe(1026, 1072, 1, new short[] { 1014, 160, 1025, 5, 1072, 1 });
            InsertRecipe(1029, 1072, 1, new short[] { 1014, 20, 1028, 5, 1072, 1 });
            InsertRecipe(1030, 1072, 1, new short[] { 1014, 50, 1029, 5, 1072, 1 });
            InsertRecipe(1031, 1072, 4, new short[] { 1028, 1, 2097, 5, 1072, 1 });
            InsertRecipe(1032, 1072, 4, new short[] { 1028, 1, 2097, 5, 1072, 1 });
            InsertRecipe(1033, 1072, 4, new short[] { 1028, 1, 2097, 5, 1072, 1 });
            InsertRecipe(1034, 1072, 4, new short[] { 1028, 1, 2097, 5, 1072, 1 });

            // Production Tools for Raw Materials
            InsertRecipe(2035, 1073, 1, new short[] { 1014, 5, 2034, 5, 1073, 1 });
            InsertRecipe(2036, 1073, 1, new short[] { 1014, 10, 2035, 5, 1073, 1 });
            InsertRecipe(2037, 1073, 1, new short[] { 1014, 20, 2036, 5, 1073, 1 });
            InsertRecipe(2039, 1073, 1, new short[] { 1014, 5, 2038, 5, 1073, 1 });
            InsertRecipe(2040, 1073, 1, new short[] { 1014, 10, 2039, 5, 1073, 1 });
            InsertRecipe(2041, 1073, 1, new short[] { 1014, 20, 2040, 5, 1073, 1 });
            InsertRecipe(2043, 1073, 1, new short[] { 1014, 5, 2042, 5, 1073, 1 });
            InsertRecipe(2044, 1073, 1, new short[] { 1014, 10, 2043, 5, 1073, 1 });
            InsertRecipe(2045, 1073, 1, new short[] { 1014, 20, 2044, 5, 1073, 1 });
            InsertRecipe(2047, 1073, 1, new short[] { 1014, 5, 2046, 5, 1073, 1 });
            InsertRecipe(2048, 1073, 1, new short[] { 1014, 10, 2047, 5, 1073, 1 });
            InsertRecipe(2049, 1073, 1, new short[] { 1014, 20, 2048, 5, 1073, 1 });

            // Production Tools for Gloves and Shoes
            InsertRecipe(718, 1083, 1, new short[] { 1027, 5, 1028, 1, 2042, 4, 1083, 1 });
            InsertRecipe(703, 1083, 1, new short[] { 1027, 7, 1028, 2, 2034, 5, 1083, 1 });
            InsertRecipe(705, 1083, 1, new short[] { 1027, 9, 1028, 3, 2035, 3, 1083, 1 });
            InsertRecipe(719, 1083, 1, new short[] { 1027, 12, 1028, 4, 2047, 5, 1083, 1 });
            InsertRecipe(722, 1083, 1, new short[] { 1027, 5, 1028, 1, 2046, 5, 1083, 1 });
            InsertRecipe(723, 1083, 1, new short[] { 1027, 7, 1028, 2, 2046, 7, 1083, 1 });
            InsertRecipe(724, 1083, 1, new short[] { 1027, 9, 1028, 3, 2047, 4, 1083, 1 });
            InsertRecipe(725, 1083, 1, new short[] { 1027, 14, 1028, 4, 2047, 7, 1083, 1 });
            InsertRecipe(325, 1083, 1, new short[] { 2044, 10, 2048, 10, 2093, 50, 1083, 1 });

            // Construction Plan (Level 1)
            InsertRecipe(3121, 1235, 1, new short[] { 2036, 50, 2037, 30, 2040, 20, 2105, 10, 2189, 20, 2205, 20, 1, 1235 });
            InsertRecipe(3122, 1235, 1, new short[] { 2040, 50, 2041, 30, 2048, 20, 2109, 10, 2190, 20, 2206, 20, 1, 1235 });
            InsertRecipe(3123, 1235, 1, new short[] { 2044, 20, 2048, 50, 2049, 30, 2117, 10, 2191, 20, 2207, 20, 1, 1235 });
            InsertRecipe(3124, 1235, 1, new short[] { 2036, 20, 2044, 50, 2045, 30, 2118, 10, 2192, 20, 2208, 20, 1, 1235 });

            // Construction Plan (Level 2)
            InsertRecipe(3125, 1236, 1, new short[] { 2037, 70, 2041, 40, 2048, 20, 2105, 20, 2189, 30, 2193, 30, 2197, 20, 2205, 40, 1236, 1 });
            InsertRecipe(3126, 1236, 1, new short[] { 2041, 70, 2044, 20, 2049, 40, 2109, 20, 2190, 30, 2194, 30, 2198, 20, 2206, 40, 1236, 1 });
            InsertRecipe(3127, 1236, 1, new short[] { 2036, 20, 2045, 40, 2049, 70, 2117, 20, 2191, 30, 2195, 30, 2199, 20, 2207, 40, 1236, 1 });
            InsertRecipe(3128, 1236, 1, new short[] { 2037, 40, 2040, 20, 2045, 70, 2118, 20, 2192, 30, 2196, 30, 2200, 20, 2208, 40, 1236, 1 });

            // Boot Combination Recipe A
            InsertRecipe(384, 1237, 1, new short[] { 1027, 30, 1032, 10, 2010, 10, 2044, 30, 2208, 10, 1237, 1 });
            InsertRecipe(385, 1237, 1, new short[] { 1027, 30, 1031, 10, 2010, 10, 2036, 30, 2205, 10, 1237, 1 });
            InsertRecipe(386, 1237, 1, new short[] { 1027, 30, 1033, 10, 2010, 10, 2040, 30, 2206, 10, 1237, 1 });
            InsertRecipe(387, 1237, 1, new short[] { 1027, 30, 1034, 10, 2010, 10, 2048, 30, 2207, 10, 1237, 1 });

            // Boot Combination Recipe B
            InsertRecipe(388, 1238, 1, new short[] { 1027, 50, 1030, 5, 2010, 20, 2204, 10, 2210, 5, 1238, 1 });
            InsertRecipe(389, 1238, 1, new short[] { 1027, 50, 1030, 5, 2010, 20, 2201, 10, 2209, 5, 1238, 1 });
            InsertRecipe(390, 1238, 1, new short[] { 1027, 50, 1030, 5, 2010, 20, 2202, 10, 2211, 5, 1238, 1 });
            InsertRecipe(391, 1238, 1, new short[] { 1027, 50, 1030, 5, 2010, 20, 2203, 10, 2212, 5, 1238, 1 });

            // Glove Combination Recipe A
            InsertRecipe(376, 1239, 1, new short[] { 1027, 30, 1032, 10, 2010, 10, 2044, 30, 2208, 10, 1239, 1 });
            InsertRecipe(377, 1239, 1, new short[] { 1027, 30, 1031, 10, 2010, 10, 2036, 30, 2205, 10, 1239, 1 });
            InsertRecipe(378, 1239, 1, new short[] { 1027, 30, 1033, 10, 2010, 10, 2040, 30, 2206, 10, 1239, 1 });
            InsertRecipe(379, 1239, 1, new short[] { 1027, 30, 1034, 10, 2010, 10, 2048, 30, 2207, 10, 1239, 1 });

            // Glove Combination Recipe B
            InsertRecipe(380, 1240, 1, new short[] { 1027, 50, 1030, 5, 2010, 20, 2204, 10, 2210, 5, 1240, 1 });
            InsertRecipe(381, 1240, 1, new short[] { 1027, 50, 1030, 5, 2010, 20, 2201, 10, 2209, 5, 1240, 1 });
            InsertRecipe(382, 1240, 1, new short[] { 1027, 50, 1030, 5, 2010, 20, 2202, 10, 2211, 5, 1240, 1 });
            InsertRecipe(383, 1240, 1, new short[] { 1027, 50, 1030, 5, 2010, 20, 2203, 10, 2212, 5, 1240, 1 });

            // Consumables Recipe
            InsertRecipe(1245, 1241, 1, new short[] { 2029, 5, 2097, 5, 2196, 5, 2208, 5, 2215, 1, 1241, 1 });
            InsertRecipe(1246, 1241, 1, new short[] { 2029, 5, 2097, 5, 2193, 5, 2206, 5, 1241, 1 });
            InsertRecipe(1247, 1241, 1, new short[] { 2029, 5, 2097, 5, 2194, 5, 2207, 5, 1241, 1 });
            InsertRecipe(1248, 1241, 1, new short[] { 2029, 5, 2097, 5, 2195, 5, 2205, 5, 1241, 1 });
            InsertRecipe(1249, 1241, 1, new short[] { 2029, 5, 2097, 5, 2195, 5, 2205, 5, 1241, 1 });

            // Amir's Armour Parchment
            InsertRecipe(409, 1312, 1, new short[] { 298, 1, 2049, 70, 2227, 80, 2254, 5, 2265, 80, 1312, 1 });
            InsertRecipe(410, 1312, 1, new short[] { 296, 1, 2037, 70, 2246, 80, 2255, 5, 2271, 80, 1312, 1 });
            InsertRecipe(411, 1312, 1, new short[] { 272, 1, 2041, 70, 2252, 5, 2253, 80, 2270, 80, 1312, 1 });

            // Amir's Weapon Parchment A
            InsertRecipe(400, 1313, 1, new short[] { 263, 1, 2036, 60, 2218, 40, 2250, 10, 1313, 1 });
            InsertRecipe(402, 1313, 1, new short[] { 292, 1, 2040, 60, 2217, 50, 2249, 5, 2263, 30, 2279, 3, 1313, 1 });
            InsertRecipe(403, 1313, 1, new short[] { 266, 1, 2040, 60, 2217, 40, 2249, 10, 1313, 1 });
            InsertRecipe(405, 1313, 1, new short[] { 290, 1, 2044, 60, 2224, 50, 2251, 5, 2262, 3, 2275, 30, 1313, 1 });
            InsertRecipe(406, 1313, 1, new short[] { 269, 1, 2048, 60, 2224, 40, 2251, 10, 1313, 1 });
            InsertRecipe(408, 1313, 1, new short[] { 264, 1, 2036, 60, 2218, 50, 2222, 3, 2250, 5, 2276, 30, 1313, 1 });

            // Amir's Weapon Parchment B
            InsertRecipe(401, 1314, 1, new short[] { 400, 1, 2037, 99, 2222, 3, 2231, 70, 2257, 99, 1314, 1 });
            InsertRecipe(404, 1314, 1, new short[] { 403, 1, 2041, 99, 2219, 3, 2226, 70, 2277, 99, 1314, 1 });
            InsertRecipe(407, 1314, 1, new short[] { 406, 1, 2049, 99, 2245, 3, 2261, 70, 2269, 99, 1314, 1 });

            // Amir's Weapon Specification Book Cover
            InsertRecipe(1315, 1316, 1, new short[] { 1312, 10, 1313, 10, 1314, 10, 1316, 1 });

            // Ancelloan's Accessory Production Scroll
            InsertRecipe(4942, 5884, 1, new short[] { 4940, 1, 2805, 15, 2816, 5, 5881, 5, 2811, 30, 5884, 1 });
            InsertRecipe(4943, 5884, 1, new short[] { 4938, 1, 2805, 10, 2816, 3, 5881, 3, 2811, 20, 5884, 1 });
            InsertRecipe(4944, 5884, 1, new short[] { 4936, 1, 2805, 12, 2816, 4, 5881, 4, 2811, 25, 5884, 1 });
            InsertRecipe(4946, 5884, 1, new short[] { 4940, 1, 2805, 15, 2816, 5, 5880, 5, 2811, 30, 5884, 1 });
            InsertRecipe(4947, 5884, 1, new short[] { 4938, 1, 2805, 10, 2816, 3, 5880, 3, 2811, 20, 5884, 1 });
            InsertRecipe(4948, 5884, 1, new short[] { 4936, 1, 2805, 12, 2816, 4, 5880, 4, 2811, 25, 5884, 1 });

            // Ancelloan's Weapon Production Scroll
            InsertRecipe(4958, 5885, 1, new short[] { 4901, 1, 2805, 80, 2816, 60, 5880, 70, 2812, 35, 5885, 1 });
            InsertRecipe(4959, 5885, 1, new short[] { 4907, 1, 2805, 80, 2816, 60, 5880, 70, 2812, 35, 5885, 1 });
            InsertRecipe(4960, 5885, 1, new short[] { 4904, 1, 2805, 80, 2816, 60, 5880, 70, 2812, 35, 5885, 1 });
            InsertRecipe(4964, 5885, 1, new short[] { 4901, 1, 2805, 80, 2816, 60, 5881, 70, 2812, 35, 5885, 1 });
            InsertRecipe(4965, 5885, 1, new short[] { 4907, 1, 2805, 80, 2816, 60, 5881, 70, 2812, 35, 5885, 1 });
            InsertRecipe(4966, 5885, 1, new short[] { 4904, 1, 2805, 80, 2816, 60, 5881, 70, 2812, 35, 5885, 1 });

            // Ancelloan's Secondary Weapon Production Scroll
            InsertRecipe(4955, 5886, 1, new short[] { 4913, 1, 2805, 80, 2816, 60, 5880, 70, 2812, 35, 5886, 1 });
            InsertRecipe(4956, 5886, 1, new short[] { 4910, 1, 2805, 80, 2816, 60, 5880, 70, 2812, 35, 5886, 1 });
            InsertRecipe(4957, 5886, 1, new short[] { 4916, 1, 2805, 80, 2816, 60, 5880, 70, 2812, 35, 5886, 1 });
            InsertRecipe(4961, 5886, 1, new short[] { 4913, 1, 2805, 80, 2816, 60, 5881, 70, 2812, 35, 5886, 1 });
            InsertRecipe(4962, 5886, 1, new short[] { 4910, 1, 2805, 80, 2816, 60, 5881, 70, 2812, 35, 5886, 1 });
            InsertRecipe(4963, 5886, 1, new short[] { 4916, 1, 2805, 80, 2816, 60, 5881, 70, 2812, 35, 5886, 1 });

            // Ancelloan's Armour Production Scroll
            InsertRecipe(4949, 5887, 1, new short[] { 4919, 1, 2805, 80, 2816, 40, 5880, 10, 2818, 20, 2819, 10, 2811, 70, 5887, 1 });
            InsertRecipe(4950, 5887, 1, new short[] { 4925, 1, 2805, 60, 2816, 15, 5880, 10, 2814, 70, 2818, 10, 2819, 20, 5887, 1 });
            InsertRecipe(4951, 5887, 1, new short[] { 4922, 1, 2805, 70, 2816, 30, 5880, 70, 2814, 35, 2818, 15, 2819, 15, 2811, 35, 5887, 1 });
            InsertRecipe(4952, 5887, 1, new short[] { 4919, 1, 2805, 80, 2816, 40, 5881, 10, 2818, 20, 2819, 10, 2811, 90, 5887, 1 });
            InsertRecipe(4953, 5887, 1, new short[] { 4925, 1, 2805, 60, 2816, 15, 5881, 10, 2814, 70, 2818, 10, 2819, 20, 5887, 1 });
            InsertRecipe(4954, 5887, 1, new short[] { 4922, 1, 2805, 70, 2816, 30, 5881, 70, 2814, 35, 2818, 15, 2819, 15, 2811, 35, 5887, 1 });

            // Charred Mask Parchment
            InsertRecipe(4927, 5900, 1, new short[] { 2505, 3, 2506, 2, 2353, 30, 2355, 20, 5900, 1 });
            InsertRecipe(4928, 5900, 1, new short[] { 2505, 10, 2506, 8, 2507, 1, 2353, 90, 2356, 60, 5900, 3 });

            // Grenigas Accessories Parchment
            InsertRecipe(4936, 5901, 1, new short[] { 4935, 1, 2505, 4, 2506, 4, 2359, 20, 2360, 20, 2509, 5, 5901, 1 });
            InsertRecipe(4938, 5901, 1, new short[] { 4937, 1, 2505, 6, 2506, 2, 2359, 20, 2360, 20, 2510, 5, 5901, 1 });
            InsertRecipe(4940, 5901, 1, new short[] { 4939, 1, 2505, 2, 2506, 6, 2359, 20, 2360, 20, 2508, 5, 5901, 1 });

            // this implementation takes a FUCKTON of hardcoding, for fucks sake ENTWELL why u suck
            // soo much -_-
        }

        private static void InsertRecipe(short itemVNum, short triggerVNum, byte amount = 1, short[] recipeItems = null)
        {
            var recipe = new RecipeDTO
            {
                ItemVNum = itemVNum,
                Amount = amount,
                ProduceItemVNum = triggerVNum
            };
            if (DaoFactory.RecipeDao.LoadByItemVNum(recipe.ItemVNum) == null)
            {
                DaoFactory.RecipeDao.Insert(recipe);
            }

            recipe = DaoFactory.RecipeDao.LoadByItemVNum(itemVNum);
            if (recipeItems == null || recipe == null)
            {
                return;
            }

            for (int i = 0; i < recipeItems.Length; i += 2)
            {
                var recipeItem = new RecipeItemDTO
                {
                    ItemVNum = recipeItems[i],
                    Amount = recipeItems[i + 1],
                    RecipeId = recipe.RecipeId
                };
                if (!DaoFactory.RecipeItemDao.LoadByRecipeAndItem(recipe.RecipeId, recipeItem.ItemVNum).Any())
                {
                    DaoFactory.RecipeItemDao.Insert(recipeItem);
                }
            }
        }

        public void ImportRecipe()
        {
            int count = 0;
            int mapnpcid = 0;
            short item = 0;
            RecipeDTO recipe;

            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("n_run") || o[0].Equals("pdtse") || o[0].Equals("m_list")))
            {
                if (currentPacket.Length > 4 && currentPacket[0] == "n_run")
                {
                    int.TryParse(currentPacket[4], out mapnpcid);
                    continue;
                }

                if (currentPacket.Length > 1 && currentPacket[0] == "m_list" && (currentPacket[1] == "2" || currentPacket[1] == "4"))
                {
                    for (int i = 2; i < currentPacket.Length - 1; i++)
                    {
                        if (DaoFactory.MapNpcDao.LoadById(mapnpcid) == null)
                        {
                            continue;
                        }

                        recipe = new RecipeDTO
                        {
                            ItemVNum = short.Parse(currentPacket[i]),
                            MapNpcId = mapnpcid
                        };
                        if (DaoFactory.RecipeDao.LoadByNpc(mapnpcid).Any(s => s.ItemVNum == recipe.ItemVNum))
                        {
                            continue;
                        }

                        DaoFactory.RecipeDao.Insert(recipe);
                        count++;
                    }

                    continue;
                }

                if (currentPacket.Length > 2 && currentPacket[0] == "pdtse")
                {
                    item = short.Parse(currentPacket[2]);
                    continue;
                }

                if (currentPacket.Length > 1 && currentPacket[0] == "m_list" && (currentPacket[1] == "3" || currentPacket[1] == "5"))
                {
                    for (int i = 3; i < currentPacket.Length - 1; i += 2)
                    {
                        RecipeDTO rec = DaoFactory.RecipeDao.LoadByNpc(mapnpcid).FirstOrDefault(s => s.ItemVNum == item);
                        if (rec != null)
                        {
                            rec.Amount = byte.Parse(currentPacket[2]);
                            DaoFactory.RecipeDao.Update(rec);
                            RecipeDTO recipedto = DaoFactory.RecipeDao.LoadByNpc(mapnpcid).FirstOrDefault(s => s.ItemVNum == item);
                            if (recipedto != null)
                            {
                                short recipeId = recipedto.RecipeId;

                                var recipeitem = new RecipeItemDTO
                                {
                                    ItemVNum = short.Parse(currentPacket[i]),
                                    Amount = byte.Parse(currentPacket[i + 1]),
                                    RecipeId = recipeId
                                };

                                if (!DaoFactory.RecipeItemDao.LoadByRecipeAndItem(recipeId, recipeitem.ItemVNum).Any())
                                {
                                    DaoFactory.RecipeItemDao.Insert(recipeitem);
                                }
                            }
                        }
                    }

                    item = -1;
                }
            }

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("RECIPES_PARSED"), count));
        }

        //Need fix
        public void ImportRespawnMapType()
        {
            List<RespawnMapTypeDTO> respawnmaptypemaps = new List<RespawnMapTypeDTO>
            {
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.DefaultAct1,
                    DefaultMapId = 1,
                    DefaultX = 80,
                    DefaultY = 116,
                    Name = "Default"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.ReturnAct1,
                    DefaultMapId = 0,
                    DefaultX = 0,
                    DefaultY = 0,
                    Name = "Return"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.DefaultAct5,
                    DefaultMapId = 170,
                    DefaultX = 86,
                    DefaultY = 48,
                    Name = "DefaultAct5"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.ReturnAct5,
                    DefaultMapId = 0,
                    DefaultX = 0,
                    DefaultY = 0,
                    Name = "ReturnAct5"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.DefaultAct6,
                    DefaultMapId = 228,
                    DefaultX = 72,
                    DefaultY = 102,
                    Name = "DefaultAct6"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.DefaultAct62,
                    DefaultMapId = 228,
                    DefaultX = 72,
                    DefaultY = 102,
                    Name = "DefaultAct62"
                },
                new RespawnMapTypeDTO
                {
                    RespawnMapTypeId = (long)RespawnType.DefaultOasis,
                    DefaultMapId = 261,
                    DefaultX = 66,
                    DefaultY = 70,
                    Name = "DefaultOasis"
                }
            };
            DaoFactory.RespawnMapTypeDao.Insert(respawnmaptypemaps);
            Logger.Log.Info(Language.Instance.GetMessageFromKey("RESPAWNTYPE_PARSED"));
        }

        public void ImportShopItems()
        {
            List<ShopItemDTO> shopitems = new List<ShopItemDTO>();
            int itemCounter = 0;
            byte type = 0;
            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("n_inv") || o[0].Equals("shopping")))
            {
                if (currentPacket[0].Equals("n_inv"))
                {
                    if (DaoFactory.ShopDao.LoadByNpc(short.Parse(currentPacket[2])) == null)
                    {
                        continue;
                    }

                    for (int i = 5; i < currentPacket.Length; i++)
                    {
                        string[] item = currentPacket[i].Split('.');
                        ShopItemDTO sitem = null;

                        if (item.Length == 5)
                        {
                            sitem = new ShopItemDTO
                            {
                                ShopId = DaoFactory.ShopDao.LoadByNpc(short.Parse(currentPacket[2])).ShopId,
                                Type = type,
                                Slot = byte.Parse(item[1]),
                                ItemVNum = short.Parse(item[2])
                            };
                        }
                        else if (item.Length == 6)
                        {
                            sitem = new ShopItemDTO
                            {
                                ShopId = DaoFactory.ShopDao.LoadByNpc(short.Parse(currentPacket[2])).ShopId,
                                Type = type,
                                Slot = byte.Parse(item[1]),
                                ItemVNum = short.Parse(item[2]),
                                Rare = sbyte.Parse(item[3]),
                                Upgrade = byte.Parse(item[4])
                            };
                        }

                        if (sitem == null || shopitems.Any(s => s.ItemVNum.Equals(sitem.ItemVNum) && s.ShopId.Equals(sitem.ShopId)) ||
                            DaoFactory.ShopItemDao.LoadByShopId(sitem.ShopId).Any(s => s.ItemVNum.Equals(sitem.ItemVNum)))
                        {
                            continue;
                        }

                        shopitems.Add(sitem);
                        itemCounter++;
                    }
                }
                else
                {
                    if (currentPacket.Length > 3)
                    {
                        type = byte.Parse(currentPacket[1]);
                    }
                }
            }

            DaoFactory.ShopItemDao.Insert(shopitems);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPITEMS_PARSED"), itemCounter));
        }

        public void ImportShops()
        {
            int shopCounter = 0;
            List<ShopDTO> shops = new List<ShopDTO>();
            foreach (string[] currentPacket in _packetList.Where(o => o.Length > 6 && o[0].Equals("shop") && o[1].Equals("2")))
            {
                MapNpcDTO npc = DaoFactory.MapNpcDao.LoadById(short.Parse(currentPacket[2]));
                if (npc == null)
                {
                    continue;
                }

                string name = string.Empty;
                for (int j = 6; j < currentPacket.Length; j++)
                {
                    name += $"{currentPacket[j]} ";
                }

                name = name.Trim();

                var shop = new ShopDTO
                {
                    Name = name,
                    MapNpcId = npc.MapNpcId,
                    MenuType = byte.Parse(currentPacket[4]),
                    ShopType = byte.Parse(currentPacket[5])
                };

                if (DaoFactory.ShopDao.LoadByNpc(npc.MapNpcId) == null && shops.All(s => s.MapNpcId != npc.MapNpcId))
                {
                    shops.Add(shop);
                    shopCounter++;
                }
            }

            DaoFactory.ShopDao.Insert(shops);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPS_PARSED"), shopCounter));
        }

        public void ImportShopSkills()
        {
            List<ShopSkillDTO> shopskills = new List<ShopSkillDTO>();
            int itemCounter = 0;
            byte type = 0;
            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("n_inv") || o[0].Equals("shopping")))
            {
                if (currentPacket[0].Equals("n_inv"))
                {
                    if (DaoFactory.ShopDao.LoadByNpc(short.Parse(currentPacket[2])) != null)
                    {
                        for (int i = 5; i < currentPacket.Length; i++)
                        {
                            ShopSkillDTO sskill;
                            if (!currentPacket[i].Contains("."))
                            {
                                sskill = new ShopSkillDTO
                                {
                                    ShopId = DaoFactory.ShopDao.LoadByNpc(short.Parse(currentPacket[2])).ShopId,
                                    Type = type,
                                    Slot = (byte)(i - 5),
                                    SkillVNum = short.Parse(currentPacket[i])
                                };

                                if (shopskills.Any(s => s.SkillVNum.Equals(sskill.SkillVNum) && s.ShopId.Equals(sskill.ShopId)) ||
                                    DaoFactory.ShopSkillDao.LoadByShopId(sskill.ShopId).Any(s => s.SkillVNum.Equals(sskill.SkillVNum)))
                                {
                                    continue;
                                }

                                shopskills.Add(sskill);
                                itemCounter++;
                            }
                        }
                    }
                }
                else
                {
                    if (currentPacket.Length > 3)
                    {
                        type = byte.Parse(currentPacket[1]);
                    }
                }
            }

            DaoFactory.ShopSkillDao.Insert(shopskills);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPSKILLS_PARSED"), itemCounter));
        }


        public void ImportSkills()
        {
            string fileSkillId = $"{_folder}\\Skill.dat";
            string fileSkillLang = $"{_folder}\\_code_{ConfigurationManager.AppSettings["Language"]}_Skill.txt";
            List<SkillDTO> skills = new List<SkillDTO>();

            Dictionary<string, string> dictionaryIdLang = new Dictionary<string, string>();
            var skill = new SkillDTO();
            List<ComboDTO> combo = new List<ComboDTO>();
            List<BCardDTO> skillCards = new List<BCardDTO>();
            string line;
            int counter = 0;
            using (var skillIdLangStream = new StreamReader(fileSkillLang, Encoding.GetEncoding(1252)))
            {
                while ((line = skillIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length > 1 && !dictionaryIdLang.ContainsKey(linesave[0]))
                    {
                        dictionaryIdLang.Add(linesave[0], linesave[1]);
                    }
                }

                skillIdLangStream.Close();
            }

            using (var skillIdStream = new StreamReader(fileSkillId, Encoding.GetEncoding(1252)))
            {
                while ((line = skillIdStream.ReadLine()) != null)
                {
                    string[] currentLine = line.Split('\t');

                    if (currentLine.Length > 2 && currentLine[1] == "VNUM")
                    {
                        skill = new SkillDTO
                        {
                            SkillVNum = short.Parse(currentLine[2])
                        };
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "NAME")
                    {
                        skill.Name = dictionaryIdLang.TryGetValue(currentLine[2], out string name) ? name : string.Empty;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "TYPE")
                    {
                        skill.SkillType = byte.Parse(currentLine[2]);
                        skill.CastId = short.Parse(currentLine[3]);
                        skill.Class = byte.Parse(currentLine[4]);
                        skill.Type = byte.Parse(currentLine[5]);
                        skill.Element = byte.Parse(currentLine[7]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "FCOMBO")
                    {
                        for (int i = 3; i < currentLine.Length - 4; i += 3)
                        {
                            var comb = new ComboDTO
                            {
                                SkillVNum = skill.SkillVNum,
                                Hit = short.Parse(currentLine[i]),
                                Animation = short.Parse(currentLine[i + 1]),
                                Effect = short.Parse(currentLine[i + 2])
                            };

                            if (comb.Hit == 0 && comb.Animation == 0 && comb.Effect == 0)
                            {
                                continue;
                            }

                            if (!DaoFactory.ComboDao.LoadByVNumHitAndEffect(comb.SkillVNum, comb.Hit, comb.Effect).Any())
                            {
                                combo.Add(comb);
                            }
                        }
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "COST")
                    {
                        skill.CPCost = currentLine[2] == "-1" ? (byte)0 : byte.Parse(currentLine[2]);
                        skill.Price = int.Parse(currentLine[3]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "LEVEL")
                    {
                        skill.LevelMinimum = currentLine[2] != "-1" ? byte.Parse(currentLine[2]) : (byte)0;
                        if (skill.Class > 31)
                        {
                            SkillDTO firstskill = skills.FirstOrDefault(s => s.Class == skill.Class);
                            if (firstskill == null || skill.SkillVNum <= firstskill.SkillVNum + 10)
                            {
                                switch (skill.Class)
                                {
                                    case 8:
                                        switch (skills.Count(s => s.Class == skill.Class))
                                        {
                                            case 3:
                                                skill.LevelMinimum = 20;
                                                break;

                                            case 2:
                                                skill.LevelMinimum = 10;
                                                break;

                                            default:
                                                skill.LevelMinimum = 0;
                                                break;
                                        }

                                        break;

                                    case 9:
                                        switch (skills.Count(s => s.Class == skill.Class))
                                        {
                                            case 9:
                                                skill.LevelMinimum = 20;
                                                break;

                                            case 8:
                                                skill.LevelMinimum = 16;
                                                break;

                                            case 7:
                                                skill.LevelMinimum = 12;
                                                break;

                                            case 6:
                                                skill.LevelMinimum = 8;
                                                break;

                                            case 5:
                                                skill.LevelMinimum = 4;
                                                break;

                                            default:
                                                skill.LevelMinimum = 0;
                                                break;
                                        }

                                        break;

                                    case 16:
                                        switch (skills.Count(s => s.Class == skill.Class))
                                        {
                                            case 6:
                                                skill.LevelMinimum = 20;
                                                break;

                                            case 5:
                                                skill.LevelMinimum = 15;
                                                break;

                                            case 4:
                                                skill.LevelMinimum = 10;
                                                break;

                                            case 3:
                                                skill.LevelMinimum = 5;
                                                break;

                                            case 2:
                                                skill.LevelMinimum = 3;
                                                break;

                                            default:
                                                skill.LevelMinimum = 0;
                                                break;
                                        }

                                        break;

                                    default:
                                        switch (skills.Count(s => s.Class == skill.Class))
                                        {
                                            case 10:
                                                skill.LevelMinimum = 20;
                                                break;

                                            case 9:
                                                skill.LevelMinimum = 16;
                                                break;

                                            case 8:
                                                skill.LevelMinimum = 12;
                                                break;

                                            case 7:
                                                skill.LevelMinimum = 8;
                                                break;

                                            case 6:
                                                skill.LevelMinimum = 4;
                                                break;

                                            default:
                                                skill.LevelMinimum = 0;
                                                break;
                                        }

                                        break;
                                }
                            }
                        }

                        skill.MinimumAdventurerLevel = currentLine[3] != "-1" ? byte.Parse(currentLine[3]) : (byte)0;
                        skill.MinimumSwordmanLevel = currentLine[4] != "-1" ? byte.Parse(currentLine[4]) : (byte)0;
                        skill.MinimumArcherLevel = currentLine[5] != "-1" ? byte.Parse(currentLine[5]) : (byte)0;
                        skill.MinimumMagicianLevel = currentLine[6] != "-1" ? byte.Parse(currentLine[6]) : (byte)0;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "EFFECT")
                    {
                        skill.CastEffect = short.Parse(currentLine[3]);
                        skill.CastAnimation = short.Parse(currentLine[4]);
                        skill.Effect = short.Parse(currentLine[5]);
                        skill.AttackAnimation = short.Parse(currentLine[6]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "TARGET")
                    {
                        skill.TargetType = byte.Parse(currentLine[2]);
                        skill.HitType = byte.Parse(currentLine[3]);
                        skill.Range = byte.Parse(currentLine[4]);
                        skill.TargetRange = byte.Parse(currentLine[5]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "DATA")
                    {
                        skill.UpgradeSkill = short.Parse(currentLine[2]);
                        skill.UpgradeType = short.Parse(currentLine[3]);
                        skill.CastTime = short.Parse(currentLine[6]);
                        skill.Cooldown = short.Parse(currentLine[7]);
                        skill.MpCost = short.Parse(currentLine[10]);
                        skill.ItemVNum = short.Parse(currentLine[12]);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "BASIC")
                    {
                        byte type = (byte)int.Parse(currentLine[3]);
                        if (type == 0 || type == 255)
                        {
                            continue;
                        }

                        int first = int.Parse(currentLine[5]);
                        var itemCard = new BCardDTO
                        {
                            SkillVNum = skill.SkillVNum,
                            Type = type,
                            SubType = (byte)((int.Parse(currentLine[4]) + 1) * 10 + 1 + (first < 0 ? 1 : 0)),
                            IsLevelScaled = Convert.ToBoolean(first % 4),
                            IsLevelDivided = (first % 4) == 2,
                            FirstData = (short)((first > 0 ? first : -first) / 4),
                            SecondData = (short)(int.Parse(currentLine[6]) / 4),
                            ThirdData = (short)(int.Parse(currentLine[7]) / 4)
                        };
                        skillCards.Add(itemCard);
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "FCOMBO")
                    {
                        // investigate
                        /*
                        if (currentLine[2] == "1")
                        {
                            combo.FirstActivationHit = byte.Parse(currentLine[3]);
                            combo.FirstComboAttackAnimation = short.Parse(currentLine[4]);
                            combo.FirstComboEffect = short.Parse(currentLine[5]);
                            combo.SecondActivationHit = byte.Parse(currentLine[3]);
                            combo.SecondComboAttackAnimation = short.Parse(currentLine[4]);
                            combo.SecondComboEffect = short.Parse(currentLine[5]);
                            combo.ThirdActivationHit = byte.Parse(currentLine[3]);
                            combo.ThirdComboAttackAnimation = short.Parse(currentLine[4]);
                            combo.ThirdComboEffect = short.Parse(currentLine[5]);
                            combo.FourthActivationHit = byte.Parse(currentLine[3]);
                            combo.FourthComboAttackAnimation = short.Parse(currentLine[4]);
                            combo.FourthComboEffect = short.Parse(currentLine[5]);
                            combo.FifthActivationHit = byte.Parse(currentLine[3]);
                            combo.FifthComboAttackAnimation = short.Parse(currentLine[4]);
                            combo.FifthComboEffect = short.Parse(currentLine[5]);
                        }
                        */
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "CELL")
                    {
                        // investigate
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "Z_DESC")
                    {
                        // investigate
                        if (DaoFactory.SkillDao.LoadById(skill.SkillVNum) != null)
                        {
                            continue;
                        }

                        skills.Add(skill);
                        counter++;
                    }
                }

                DaoFactory.SkillDao.Insert(skills);
                DaoFactory.ComboDao.Insert(combo);
                DaoFactory.BCardDao.Insert(skillCards);

                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SKILLS_PARSED"), counter));
                skillIdStream.Close();
            }
        }

        public void ImportTeleporters()
        {
            int teleporterCounter = 0;
            TeleporterDTO teleporter = null;
            foreach (string[] currentPacket in _packetList.Where(o =>
                o[0].Equals("at") || o[0].Equals("n_run") &&
                (o[1].Equals("16") || o[1].Equals("26") || o[1].Equals("45") || o[1].Equals("301") || o[1].Equals("132") || o[1].Equals("5002") || o[1].Equals("5012"))))
            {
                if (currentPacket.Length > 4 && currentPacket[0] == "n_run")
                {
                    if (DaoFactory.MapNpcDao.LoadById(int.Parse(currentPacket[4])) == null)
                    {
                        continue;
                    }

                    teleporter = new TeleporterDTO
                    {
                        MapNpcId = int.Parse(currentPacket[4]),
                        Index = short.Parse(currentPacket[2])
                    };
                    continue;
                }

                if (currentPacket.Length > 5 && currentPacket[0] == "at")
                {
                    if (teleporter == null)
                    {
                        continue;
                    }

                    teleporter.MapId = short.Parse(currentPacket[2]);
                    teleporter.MapX = short.Parse(currentPacket[3]);
                    teleporter.MapY = short.Parse(currentPacket[4]);

                    if (DaoFactory.TeleporterDao.LoadFromNpc(teleporter.MapNpcId).Any(s => s.Index == teleporter.Index))
                    {
                        continue;
                    }

                    DaoFactory.TeleporterDao.Insert(teleporter);
                    teleporterCounter++;
                    teleporter = null;
                }
            }

            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("TELEPORTERS_PARSED"), teleporterCounter));
        }

        public void ImportScriptedInstances()
        {
            short map = 0;
            List<ScriptedInstanceDTO> listtimespace = new List<ScriptedInstanceDTO>();
            List<ScriptedInstanceDTO> bddlist = new List<ScriptedInstanceDTO>();
            foreach (string[] currentPacket in _packetList.Where(o => o[0].Equals("at") || o[0].Equals("wp") || o[0].Equals("gp") || o[0].Equals("rbr")))
            {
                if (currentPacket.Length > 5 && currentPacket[0] == "at")
                {
                    map = short.Parse(currentPacket[2]);
                    bddlist = DaoFactory.ScriptedInstanceDao.LoadByMap(map).ToList();
                    continue;
                }

                if (currentPacket.Length > 6 && currentPacket[0] == "wp")
                {
                    var ts = new ScriptedInstanceDTO
                    {
                        PositionX = short.Parse(currentPacket[1]),
                        PositionY = short.Parse(currentPacket[2]),
                        MapId = map
                    };

                    if (!bddlist.Concat(listtimespace).Any(s => s.MapId == ts.MapId && s.PositionX == ts.PositionX && s.PositionY == ts.PositionY))
                    {
                        listtimespace.Add(ts);
                    }
                }
                else
                {
                    switch (currentPacket[0])
                    {
                        case "gp":
                            if (sbyte.Parse(currentPacket[4]) == (byte)PortalType.Raid)
                            {
                                var ts = new ScriptedInstanceDTO
                                {
                                    PositionX = short.Parse(currentPacket[1]),
                                    PositionY = short.Parse(currentPacket[2]),
                                    MapId = map,
                                    Type = ScriptedInstanceType.Raid
                                };

                                if (!bddlist.Concat(listtimespace).Any(s => s.MapId == ts.MapId && s.PositionX == ts.PositionX && s.PositionY == ts.PositionY))
                                {
                                    listtimespace.Add(ts);
                                }
                            }

                            break;
                        case "rbr":
                            //someinfo
                            break;
                    }
                }
            }

            var zenasRaid = new ScriptedInstanceDTO
            {
                Name = "Zenas",
                MapId = 232,
                PositionX = 103,
                PositionY = 125,
                Type = ScriptedInstanceType.RaidAct6
            };

            if (!bddlist.Concat(listtimespace).Any(s => s.MapId == zenasRaid.MapId && s.PositionX == zenasRaid.PositionX && s.PositionY == zenasRaid.PositionY))
            {
                listtimespace.Add(zenasRaid);
            }

            var ereniaRaid = new ScriptedInstanceDTO
            {
                Name = "Erenia",
                MapId = 236,
                PositionX = 130,
                PositionY = 117,
                Type = ScriptedInstanceType.RaidAct6
            };

            if (!bddlist.Concat(listtimespace).Any(s => s.MapId == ereniaRaid.MapId && s.PositionX == ereniaRaid.PositionX && s.PositionY == ereniaRaid.PositionY))
            {
                listtimespace.Add(ereniaRaid);
            }

            var hatusRaid = new ScriptedInstanceDTO
            {
                Label = "Hatus",
                Name = "Hatus",
                MapId = 134,
                PositionX = 53,
                PositionY = 53,
                Type = ScriptedInstanceType.RaidAct4
            };

            if (bddlist.Concat(listtimespace).All(s => s.Label != hatusRaid.Label))
            {
                listtimespace.Add(hatusRaid);
            }

            var beriosRaid = new ScriptedInstanceDTO
            {
                Label = "Berios",
                Name = "Berios",
                MapId = 134,
                PositionX = 53,
                PositionY = 53,
                Type = ScriptedInstanceType.RaidAct4
            };

            if (bddlist.Concat(listtimespace).All(s => s.Label != beriosRaid.Label))
            {
                listtimespace.Add(beriosRaid);
            }

            var morcosRaid = new ScriptedInstanceDTO
            {
                Label = "Morcos",
                Name = "Morcos",
                MapId = 134,
                PositionX = 53,
                PositionY = 53,
                Type = ScriptedInstanceType.RaidAct4
            };

            if (bddlist.Concat(listtimespace).All(s => s.Label != morcosRaid.Label))
            {
                listtimespace.Add(morcosRaid);
            }

            var calvinaRaid = new ScriptedInstanceDTO
            {
                Label = "Calvina",
                Name = "Calvina",
                MapId = 134,
                PositionX = 53,
                PositionY = 53,
                Type = ScriptedInstanceType.RaidAct4
            };
            if (bddlist.Concat(listtimespace).All(s => s.Label != calvinaRaid.Label))
            {
                listtimespace.Add(calvinaRaid);
            }

            DaoFactory.ScriptedInstanceDao.Insert(listtimespace);
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("TIMESPACES_PARSED"), listtimespace.Count));
        }

        public void LoadMaps()
        {
            _maps = DaoFactory.MapDao.LoadAll().ToList();
        }

        internal void ImportItems()
        {
            string fileId = $"{_folder}\\Item.dat";
            string fileLang = $"{_folder}\\_code_{ConfigurationManager.AppSettings["Language"]}_Item.txt";
            Dictionary<string, string> dictionaryName = new Dictionary<string, string>();
            string line;
            List<ItemDTO> items = new List<ItemDTO>();
            List<BCardDTO> itemCards = new List<BCardDTO>();
            using (var mapIdLangStream = new StreamReader(fileLang, Encoding.GetEncoding(1252)))
            {
                while ((line = mapIdLangStream.ReadLine()) != null)
                {
                    string[] linesave = line.Split('\t');
                    if (linesave.Length <= 1 || dictionaryName.ContainsKey(linesave[0]))
                    {
                        continue;
                    }

                    dictionaryName.Add(linesave[0], linesave[1]);
                }

                mapIdLangStream.Close();
            }

            using (var npcIdStream = new StreamReader(fileId, Encoding.GetEncoding(1252)))
            {
                var item = new ItemDTO();
                bool itemAreaBegin = false;
                int itemCounter = 0;

                while ((line = npcIdStream.ReadLine()) != null)
                {
                    string[] currentLine = line.Split('\t');

                    if (currentLine.Length > 3 && currentLine[1] == "VNUM")
                    {
                        itemAreaBegin = true;
                        item.VNum = short.Parse(currentLine[2]);
                        item.Price = long.Parse(currentLine[3]);
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "END")
                    {
                        if (!itemAreaBegin)
                        {
                            continue;
                        }

                        if (DaoFactory.ItemDao.LoadById(item.VNum) == null)
                        {
                            items.Add(item);
                            itemCounter++;
                        }

                        item = new ItemDTO();
                        itemAreaBegin = false;
                    }
                    else if (currentLine.Length > 2 && currentLine[1] == "NAME")
                    {
                        item.Name = dictionaryName.TryGetValue(currentLine[2], out string name) ? name : string.Empty;
                    }
                    else if (currentLine.Length > 7 && currentLine[1] == "INDEX")
                    {
                        switch (Convert.ToByte(currentLine[2]))
                        {
                            case 4:
                                item.Type = InventoryType.Equipment;
                                break;

                            case 8:
                                item.Type = InventoryType.Equipment;
                                break;

                            case 9:
                                item.Type = InventoryType.Main;
                                break;

                            case 10:
                                item.Type = InventoryType.Etc;
                                break;

                            default:
                                item.Type = (InventoryType)Enum.Parse(typeof(InventoryType), currentLine[2]);
                                break;
                        }

                        item.ItemType = currentLine[3] != "-1" ? (ItemType)Enum.Parse(typeof(ItemType), $"{(short)item.Type}{currentLine[3]}") : ItemType.Weapon;
                        item.ItemSubType = Convert.ToByte(currentLine[4]);
                        item.EquipmentSlot = (EquipmentType)Enum.Parse(typeof(EquipmentType), currentLine[5] != "-1" ? currentLine[5] : "0");

                        // item.DesignId = Convert.ToInt16(currentLine[6]);
                        switch (item.VNum)
                        {
                            case 4101:
                            case 4102:
                            case 4103:
                            case 4104:
                            case 4105:
                                item.EquipmentSlot = 0;
                                break;

                            case 1906:
                                item.Morph = 2368;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 1907:
                                item.Morph = 2370;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 1965:
                                item.Morph = 2406;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5008:
                                item.Morph = 2411;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5117:
                                item.Morph = 2429;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5152:
                                item.Morph = 2432;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5173:
                                item.Morph = 2511;
                                item.Speed = 16;
                                item.WaitDelay = 3000;
                                break;

                            case 5196:
                                item.Morph = 2517;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5226: // Invisible locomotion, only 5 seconds with booster
                                item.Morph = 1817;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5228: // Invisible locoomotion, only 5 seconds with booster
                                item.Morph = 1819;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5232:
                                item.Morph = 2520;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5234:
                                item.Morph = 2522;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5236:
                                item.Morph = 2524;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5238:
                                item.Morph = 1817;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5240:
                                item.Morph = 1819;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 5319:
                                item.Morph = 2526;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 5321:
                                item.Morph = 2528;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5323:
                                item.Morph = 2530;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 5330:
                                item.Morph = 2928;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 5332:
                                item.Morph = 2930;
                                item.Speed = 14;
                                item.WaitDelay = 3000;
                                break;

                            case 5360:
                                item.Morph = 2932;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 5386:
                                item.Morph = 2934;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5387:
                                item.Morph = 2936;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5388:
                                item.Morph = 2938;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5389:
                                item.Morph = 2940;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5390:
                                item.Morph = 2942;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5391:
                                item.Morph = 2944;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 5914:
                                item.Morph = 2513;
                                item.Speed = 14;
                                item.WaitDelay = 3000;
                                break;

                            case 5997:
                                item.Morph = 3679;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9054:
                                item.Morph = 2368;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 9055:
                                item.Morph = 2370;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 9058:
                                item.Morph = 2406;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 9065:
                                item.Morph = 2411;
                                item.Speed = 20;
                                item.WaitDelay = 3000;
                                break;

                            case 9070:
                                item.Morph = 2429;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9073:
                                item.Morph = 2432;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9078:
                                item.Morph = 2520;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9079:
                                item.Morph = 2522;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9080:
                                item.Morph = 2524;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9081:
                                item.Morph = 1817;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9082:
                                item.Morph = 1819;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9083:
                                item.Morph = 2526;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 9084:
                                item.Morph = 2528;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 9085:
                                item.Morph = 2930;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 9086:
                                item.Morph = 2928;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 9087:
                                item.Morph = 2930;
                                item.Speed = 14;
                                item.WaitDelay = 3000;
                                break;

                            case 9088:
                                item.Morph = 2932;
                                item.Speed = 22;
                                item.WaitDelay = 3000;
                                break;

                            case 9090:
                                item.Morph = 2934;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9091:
                                item.Morph = 2936;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9092:
                                item.Morph = 2938;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9093:
                                item.Morph = 2940;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9094:
                                item.Morph = 2942;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            case 9115:
                                item.Morph = 3679;
                                item.Speed = 21;
                                item.WaitDelay = 3000;
                                break;

                            default:
                                if (item.EquipmentSlot.Equals(EquipmentType.Amulet))
                                {
                                    switch (item.VNum)
                                    {
                                        case 4503:
                                            item.EffectValue = 4544;
                                            break;

                                        case 4504:
                                            item.EffectValue = 4294;
                                            break;

                                        case 282: // Red amulet
                                            item.Effect = 791;
                                            item.EffectValue = 3;
                                            break;

                                        case 283: // Blue amulet
                                            item.Effect = 792;
                                            item.EffectValue = 3;
                                            break;

                                        case 284: // Reinforcement amulet
                                            item.Effect = 793;
                                            item.EffectValue = 3;
                                            break;

                                        case 4264: // Heroic
                                            item.Effect = 794;
                                            item.EffectValue = 3;
                                            break;

                                        case 4262: // Random heroic
                                            item.Effect = 795;
                                            item.EffectValue = 3;
                                            break;

                                        case 4261: // Amulet to reduce rarity
                                            item.Effect = 797;
                                            item.EffectValue = 1;
                                            break;

                                        default:
                                            item.EffectValue = Convert.ToInt16(currentLine[7]);
                                            break;
                                    }
                                }
                                else
                                {
                                    item.Morph = Convert.ToInt16(currentLine[7]);
                                }

                                break;
                        }
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "TYPE")
                    {
                        // currentLine[2] 0-range 2-range 3-magic
                        item.Class = item.EquipmentSlot == EquipmentType.Fairy ? (byte)15 : Convert.ToByte(currentLine[3]);
                    }
                    else if (currentLine.Length > 3 && currentLine[1] == "FLAG")
                    {
                        item.IsSoldable = currentLine[5] == "0";
                        item.IsDroppable = currentLine[6] == "0";
                        item.IsTradable = currentLine[7] == "0";
                        item.IsMinilandActionable = currentLine[8] == "1";
                        item.IsWarehouse = currentLine[9] == "1";
                        item.Flag9 = currentLine[10] == "1";
                        item.Flag1 = currentLine[11] == "1";
                        item.Flag2 = currentLine[12] == "1";
                        item.Flag3 = currentLine[13] == "1";
                        item.Flag4 = currentLine[14] == "1";
                        item.Flag5 = currentLine[15] == "1";
                        item.IsColored = currentLine[16] == "1";
                        item.Sex = currentLine[18] == "1" ? (byte)1 :
                            currentLine[17] == "1" ? (byte)2 : (byte)0;
                        //not used item.Flag6 = currentLine[19] == "1";
                        item.Flag6 = currentLine[20] == "1";
                        if (currentLine[21] == "1")
                        {
                            item.ReputPrice = item.Price;
                        }

                        item.IsHeroic = currentLine[22] == "1";
                        item.Flag7 = currentLine[23] == "1";
                        item.Flag8 = currentLine[24] == "1";
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "DATA")
                    {
                        switch (item.ItemType)
                        {
                            case ItemType.Weapon:
                                item.LevelMinimum = Convert.ToByte(currentLine[2]);
                                item.DamageMinimum = Convert.ToInt16(currentLine[3]);
                                item.DamageMaximum = Convert.ToInt16(currentLine[4]);
                                item.HitRate = Convert.ToInt16(currentLine[5]);
                                item.CriticalLuckRate = Convert.ToByte(currentLine[6]);
                                item.CriticalRate = Convert.ToInt16(currentLine[7]);
                                item.BasicUpgrade = Convert.ToByte(currentLine[10]);
                                item.MaximumAmmo = 100;
                                break;

                            case ItemType.Armor:
                                item.LevelMinimum = Convert.ToByte(currentLine[2]);
                                item.CloseDefence = Convert.ToInt16(currentLine[3]);
                                item.DistanceDefence = Convert.ToInt16(currentLine[4]);
                                item.MagicDefence = Convert.ToInt16(currentLine[5]);
                                item.DefenceDodge = Convert.ToInt16(currentLine[6]);
                                item.DistanceDefenceDodge = Convert.ToInt16(currentLine[6]);
                                item.BasicUpgrade = Convert.ToByte(currentLine[10]);
                                break;

                            case ItemType.Box:
                                switch (item.VNum)
                                {
                                    // add here your custom effect/effectvalue for box item, make
                                    // sure its unique for boxitems

                                    case 287:
                                        item.Effect = 69;
                                        item.EffectValue = 1;
                                        break;

                                    case 4240:
                                        item.Effect = 69;
                                        item.EffectValue = 2;
                                        break;

                                    case 4194:
                                        item.Effect = 69;
                                        item.EffectValue = 3;
                                        break;

                                    case 4106:
                                        item.Effect = 69;
                                        item.EffectValue = 4;
                                        break;

                                    case 185: // Hatus
                                    case 302: // Classic
                                    case 882: // Morcos
                                    case 942: // Calvina
                                    case 999: //Berios
                                        item.Effect = 999;
                                        break;

                                    default:
                                        item.Effect = Convert.ToInt16(currentLine[2]);
                                        item.EffectValue = Convert.ToInt32(currentLine[3]);
                                        item.LevelMinimum = Convert.ToByte(currentLine[4]);
                                        break;
                                }

                                break;

                            case ItemType.Fashion:
                                item.LevelMinimum = Convert.ToByte(currentLine[2]);
                                item.CloseDefence = Convert.ToInt16(currentLine[3]);
                                item.DistanceDefence = Convert.ToInt16(currentLine[4]);
                                item.MagicDefence = Convert.ToInt16(currentLine[5]);
                                item.DefenceDodge = Convert.ToInt16(currentLine[6]);
                                if (item.EquipmentSlot.Equals(EquipmentType.CostumeHat) || item.EquipmentSlot.Equals(EquipmentType.CostumeSuit))
                                {
                                    item.ItemValidTime = Convert.ToInt32(currentLine[13]) * 3600;
                                }

                                break;

                            case ItemType.Food:
                                item.Hp = Convert.ToInt16(currentLine[2]);
                                item.Mp = Convert.ToInt16(currentLine[4]);
                                break;

                            case ItemType.Jewelery:
                                switch (item.EquipmentSlot)
                                {
                                    case EquipmentType.Amulet:
                                        item.LevelMinimum = Convert.ToByte(currentLine[2]);
                                        if (item.VNum > 4055 && item.VNum < 4061 || item.VNum > 4172 && item.VNum < 4176)
                                        {
                                            item.ItemValidTime = 10800;
                                        }
                                        else if (item.VNum > 4045 && item.VNum < 4056 || item.VNum == 967 || item.VNum == 968)
                                        {
                                            // (item.VNum > 8104 && item.VNum < 8115) <= disaled for now
                                            // because doesn't work!
                                            item.ItemValidTime = 10800;
                                        }
                                        else
                                        {
                                            item.ItemValidTime = Convert.ToInt32(currentLine[3]) / 10;
                                        }

                                        break;
                                    case EquipmentType.Fairy:
                                        item.Element = Convert.ToByte(currentLine[2]);
                                        item.ElementRate = Convert.ToInt16(currentLine[3]);
                                        if (item.VNum <= 256)
                                        {
                                            item.MaxElementRate = 50;
                                        }
                                        else
                                        {
                                            if (item.ElementRate == 0)
                                            {
                                                if (item.VNum >= 800 && item.VNum <= 804)
                                                {
                                                    item.MaxElementRate = 50;
                                                }
                                                else
                                                {
                                                    item.MaxElementRate = 70;
                                                }
                                            }
                                            else if (item.ElementRate == 30)
                                            {
                                                if (item.VNum >= 884 && item.VNum <= 887)
                                                {
                                                    item.MaxElementRate = 50;
                                                }
                                                else
                                                {
                                                    item.MaxElementRate = 30;
                                                }
                                            }
                                            else if (item.ElementRate == 35)
                                            {
                                                item.MaxElementRate = 35;
                                            }
                                            else if (item.ElementRate == 40)
                                            {
                                                item.MaxElementRate = 70;
                                            }
                                            else if (item.ElementRate == 50)
                                            {
                                                item.MaxElementRate = 80;
                                            }
                                        }

                                        break;
                                    default:
                                        item.LevelMinimum = Convert.ToByte(currentLine[2]);
                                        item.MaxCellonLvl = Convert.ToByte(currentLine[3]);
                                        item.MaxCellon = Convert.ToByte(currentLine[4]);
                                        break;
                                }

                                break;

                            case ItemType.Event:
                                switch (item.VNum)
                                {
                                    case 1332:
                                        item.EffectValue = 5108;
                                        break;

                                    case 1333:
                                        item.EffectValue = 5109;
                                        break;

                                    case 1334:
                                        item.EffectValue = 5111;
                                        break;

                                    case 1335:
                                        item.EffectValue = 5107;
                                        break;

                                    case 1336:
                                        item.EffectValue = 5106;
                                        break;

                                    case 1337:
                                        item.EffectValue = 5110;
                                        break;

                                    case 1339:
                                        item.EffectValue = 5114;
                                        break;

                                    case 9031:
                                        item.EffectValue = 5108;
                                        break;

                                    case 9032:
                                        item.EffectValue = 5109;
                                        break;

                                    case 9033:
                                        item.EffectValue = 5011;
                                        break;

                                    case 9034:
                                        item.EffectValue = 5107;
                                        break;

                                    case 9035:
                                        item.EffectValue = 5106;
                                        break;

                                    case 9036:
                                        item.EffectValue = 5110;
                                        break;

                                    case 9038:
                                        item.EffectValue = 5114;
                                        break;

                                    // EffectItems aka. fireworks
                                    case 1581:
                                        item.EffectValue = 860;
                                        break;

                                    case 1582:
                                        item.EffectValue = 861;
                                        break;

                                    case 1585:
                                        item.EffectValue = 859;
                                        break;

                                    case 1983:
                                        item.EffectValue = 875;
                                        break;

                                    case 1984:
                                        item.EffectValue = 876;
                                        break;

                                    case 1985:
                                        item.EffectValue = 877;
                                        break;

                                    case 1986:
                                        item.EffectValue = 878;
                                        break;

                                    case 1987:
                                        item.EffectValue = 879;
                                        break;

                                    case 1988:
                                        item.EffectValue = 880;
                                        break;

                                    case 9044:
                                        item.EffectValue = 859;
                                        break;

                                    case 9059:
                                        item.EffectValue = 875;
                                        break;

                                    case 9060:
                                        item.EffectValue = 876;
                                        break;

                                    case 9061:
                                        item.EffectValue = 877;
                                        break;

                                    case 9062:
                                        item.EffectValue = 878;
                                        break;

                                    case 9063:
                                        item.EffectValue = 879;
                                        break;

                                    case 9064:
                                        item.EffectValue = 880;
                                        break;

                                    default:
                                        item.EffectValue = Convert.ToInt16(currentLine[7]);
                                        break;
                                }

                                break;

                            case ItemType.Special:
                                switch (item.VNum)
                                {
                                    case 1246:
                                    case 9020:
                                        item.Effect = 6600;
                                        item.EffectValue = 1;
                                        break;

                                    case 1247:
                                    case 9021:
                                        item.Effect = 6600;
                                        item.EffectValue = 2;
                                        break;

                                    case 1248:
                                    case 9022:
                                        item.Effect = 6600;
                                        item.EffectValue = 3;
                                        break;

                                    case 1249:
                                    case 9023:
                                        item.Effect = 6600;
                                        item.EffectValue = 4;
                                        break;

                                    case 5130:
                                    case 9072:
                                        item.Effect = 1006;
                                        break;

                                    case 1272:
                                    case 1858:
                                    case 9047:
                                        item.Effect = 1005;
                                        item.EffectValue = 10;
                                        break;

                                    case 1273:
                                    case 9024:
                                        item.Effect = 1005;
                                        item.EffectValue = 30;
                                        break;

                                    case 1274:
                                    case 9025:
                                        item.Effect = 1005;
                                        item.EffectValue = 60;
                                        break;

                                    case 1279:
                                    case 9029:
                                        item.Effect = 1007;
                                        item.EffectValue = 30;
                                        break;

                                    case 1280:
                                    case 9030:
                                        item.Effect = 1007;
                                        item.EffectValue = 60;
                                        break;

                                    case 1923:
                                    case 9056:
                                        item.Effect = 1007;
                                        item.EffectValue = 10;
                                        break;

                                    case 1275:
                                    case 1886:
                                    case 9026:
                                        item.Effect = 1008;
                                        item.EffectValue = 10;
                                        break;

                                    case 1276:
                                    case 9027:
                                        item.Effect = 1008;
                                        item.EffectValue = 30;
                                        break;

                                    case 1277:
                                    case 9028:
                                        item.Effect = 1008;
                                        item.EffectValue = 60;
                                        break;

                                    case 5060:
                                    case 9066:
                                        item.Effect = 1003;
                                        item.EffectValue = 30;
                                        break;

                                    case 5061:
                                    case 9067:
                                        item.Effect = 1004;
                                        item.EffectValue = 7;
                                        break;

                                    case 5062:
                                    case 9068:
                                        item.Effect = 1004;
                                        item.EffectValue = 1;
                                        break;

                                    case 5105:
                                        item.Effect = 651;
                                        break;

                                    case 5115:
                                        item.Effect = 652;
                                        break;

                                    case 1981:
                                        item.Effect = 34; // imagined number as for I = √(-1), complex z = a + bi
                                        break;

                                    case 1982:
                                        item.Effect = 6969; // imagined number as for I = √(-1), complex z = a + bi
                                        break;

                                    case 1894:
                                    case 1895:
                                    case 1896:
                                    case 1897:
                                    case 1898:
                                    case 1899:
                                    case 1900:
                                    case 1901:
                                    case 1902:
                                    case 1903:
                                        item.Effect = 789;
                                        item.EffectValue = item.VNum + 2152;
                                        break;

                                    case 4046:
                                    case 4047:
                                    case 4048:
                                    case 4049:
                                    case 4050:
                                    case 4051:
                                    case 4052:
                                    case 4053:
                                    case 4054:
                                    case 4055:
                                        item.Effect = 790;
                                        break;

                                    case 5119: // Speed booster
                                        item.Effect = 998;
                                        break;

                                    case 180: // attack amulet
                                        item.Effect = 932;
                                        break;

                                    case 181: // defense amulet
                                        item.Effect = 933;
                                        break;

                                    default:
                                        if (item.VNum > 5891 && item.VNum < 5900 || item.VNum > 9100 && item.VNum < 9109)
                                        {
                                            item.Effect = 69; // imagined number as for I = √(-1), complex z = a + bi
                                        }
                                        else
                                        {
                                            item.Effect = Convert.ToInt16(currentLine[2]);
                                        }

                                        break;
                                }

                                switch (item.Effect)
                                {
                                    case 150:
                                    case 151:
                                        if (Convert.ToInt32(currentLine[4]) == 1)
                                        {
                                            item.EffectValue = 30000;
                                        }
                                        else if (Convert.ToInt32(currentLine[4]) == 2)
                                        {
                                            item.EffectValue = 70000;
                                        }
                                        else if (Convert.ToInt32(currentLine[4]) == 3)
                                        {
                                            item.EffectValue = 180000;
                                        }
                                        else
                                        {
                                            item.EffectValue = Convert.ToInt32(currentLine[4]);
                                        }

                                        break;

                                    case 204:
                                        item.EffectValue = 10000;
                                        break;

                                    case 305:
                                        item.EffectValue = Convert.ToInt32(currentLine[5]);
                                        item.Morph = Convert.ToInt16(currentLine[4]);
                                        break;

                                    default:
                                        item.EffectValue = item.EffectValue == 0 ? Convert.ToInt32(currentLine[4]) : item.EffectValue;
                                        break;
                                }

                                item.WaitDelay = 5000;
                                break;

                            case ItemType.Magical:
                                if (item.VNum > 2059 && item.VNum < 2070)
                                {
                                    item.Effect = 10;
                                }
                                else
                                {
                                    item.Effect = Convert.ToInt16(currentLine[2]);
                                }

                                item.EffectValue = Convert.ToInt32(currentLine[4]);
                                break;

                            case ItemType.Specialist:

                                // item.isSpecialist = Convert.ToByte(currentLine[2]); item.Unknown = Convert.ToInt16(currentLine[3]);
                                item.ElementRate = Convert.ToInt16(currentLine[4]);
                                item.Speed = Convert.ToByte(currentLine[5]);
                                item.SpType = Convert.ToByte(currentLine[13]);

                                // item.Morph = Convert.ToInt16(currentLine[14]) + 1;
                                item.FireResistance = Convert.ToByte(currentLine[15]);
                                item.WaterResistance = Convert.ToByte(currentLine[16]);
                                item.LightResistance = Convert.ToByte(currentLine[17]);
                                item.DarkResistance = Convert.ToByte(currentLine[18]);

                                // item.PartnerClass = Convert.ToInt16(currentLine[19]);
                                item.LevelJobMinimum = Convert.ToByte(currentLine[20]);
                                item.ReputationMinimum = Convert.ToByte(currentLine[21]);

                                Dictionary<int, int> elementdic = new Dictionary<int, int> { { 0, 0 } };
                                if (item.FireResistance != 0)
                                {
                                    elementdic.Add(1, item.FireResistance);
                                }

                                if (item.WaterResistance != 0)
                                {
                                    elementdic.Add(2, item.WaterResistance);
                                }

                                if (item.LightResistance != 0)
                                {
                                    elementdic.Add(3, item.LightResistance);
                                }

                                if (item.DarkResistance != 0)
                                {
                                    elementdic.Add(4, item.DarkResistance);
                                }

                                item.Element = (byte)elementdic.OrderByDescending(s => s.Value).First().Key;
                                if (elementdic.Count > 1 && elementdic.OrderByDescending(s => s.Value).First().Value == elementdic.OrderByDescending(s => s.Value).ElementAt(1).Value)
                                {
                                    item.SecondaryElement = (byte)elementdic.OrderByDescending(s => s.Value).ElementAt(1).Key;
                                }

                                // needs to be hardcoded
                                switch (item.VNum)
                                {
                                    case 901:
                                        item.Element = 1;
                                        break;

                                    case 903:
                                        item.Element = 2;
                                        break;

                                    case 906:
                                        item.Element = 3;
                                        break;

                                    case 909:
                                        item.Element = 3;
                                        break;
                                }

                                break;

                            case ItemType.Shell:

                                // item.ShellMinimumLevel = Convert.ToInt16(linesave[3]);
                                // item.ShellMaximumLevel = Convert.ToInt16(linesave[4]);
                                // item.ShellType = Convert.ToByte(linesave[5]); // 3 shells of each type
                                break;

                            case ItemType.Main:
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                item.EffectValue = Convert.ToInt32(currentLine[4]);
                                break;

                            case ItemType.Upgrade:
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                switch (item.VNum)
                                {
                                    // UpgradeItems (needed to be hardcoded)
                                    case 1218:
                                        item.EffectValue = 26;
                                        break;

                                    case 1363:
                                        item.EffectValue = 27;
                                        break;

                                    case 1364:
                                        item.EffectValue = 28;
                                        break;

                                    case 5107:
                                        item.EffectValue = 47;
                                        break;

                                    case 5207:
                                        item.EffectValue = 50;
                                        break;

                                    case 5369:
                                        item.EffectValue = 61;
                                        break;

                                    case 5519:
                                        item.EffectValue = 60;
                                        break;

                                    default:
                                        item.EffectValue = Convert.ToInt32(currentLine[4]);
                                        break;
                                }

                                break;

                            case ItemType.Production:
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                item.EffectValue = Convert.ToInt32(currentLine[4]);
                                break;

                            case ItemType.Map:
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                item.EffectValue = Convert.ToInt32(currentLine[4]);
                                break;

                            case ItemType.Potion:
                                item.Hp = Convert.ToInt16(currentLine[2]);
                                item.Mp = Convert.ToInt16(currentLine[4]);
                                break;

                            case ItemType.Snack:
                                item.Hp = Convert.ToInt16(currentLine[2]);
                                item.Mp = Convert.ToInt16(currentLine[4]);
                                break;

                            case ItemType.Teacher:
                                item.Effect = Convert.ToInt16(currentLine[2]);
                                item.EffectValue = Convert.ToInt32(currentLine[4]);

                                // item.PetLoyality = Convert.ToInt16(linesave[4]); item.PetFood = Convert.ToInt16(linesave[7]);
                                break;

                            case ItemType.Part:

                                // nothing to parse
                                break;

                            case ItemType.Sell:

                                // nothing to parse
                                break;

                            case ItemType.Quest2:

                                // nothing to parse
                                break;

                            case ItemType.Quest1:

                                // nothing to parse
                                break;

                            case ItemType.Ammo:

                                // nothing to parse
                                break;
                        }

                        if (item.Type == InventoryType.Miniland)
                        {
                            item.MinilandObjectPoint = int.Parse(currentLine[2]);
                            item.EffectValue = short.Parse(currentLine[8]);
                            item.Width = Convert.ToByte(currentLine[9]);
                            item.Height = Convert.ToByte(currentLine[10]);
                        }

                        if (item.EquipmentSlot != EquipmentType.Boots && item.EquipmentSlot != EquipmentType.Gloves || item.Type != 0)
                        {
                            continue;
                        }

                        item.FireResistance = Convert.ToByte(currentLine[7]);
                        item.WaterResistance = Convert.ToByte(currentLine[8]);
                        item.LightResistance = Convert.ToByte(currentLine[9]);
                        item.DarkResistance = Convert.ToByte(currentLine[11]);
                    }
                    else if (currentLine.Length > 1 && currentLine[1] == "BUFF")
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            byte type = (byte)int.Parse(currentLine[2 + 5 * i]);
                            if (type == 0 || type == 255)
                            {
                                continue;
                            }

                            int first = int.Parse(currentLine[3 + 5 * i]);
                            var itemCard = new BCardDTO
                            {
                                ItemVNum = item.VNum,
                                Type = type,
                                SubType = (byte)((int.Parse(currentLine[5 + 5 * i]) + 1) * 10 + 1),
                                IsLevelScaled = Convert.ToBoolean(first % 4),
                                IsLevelDivided = (first % 4) == 2,
                                FirstData = (short)((first > 0 ? first : -first) / 4),
                                SecondData = (short)(int.Parse(currentLine[4 + 5 * i]) / 4),
                                ThirdData = (short)(int.Parse(currentLine[6 + 5 * i]) / 4)
                            };
                            itemCards.Add(itemCard);
                        }
                    }
                }

                DaoFactory.ItemDao.Insert(items);
                DaoFactory.BCardDao.Insert(itemCards);
                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("ITEMS_PARSED"), itemCounter));
                npcIdStream.Close();
            }
        }

        #endregion
    }
}