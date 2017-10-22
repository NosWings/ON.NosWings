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

using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Service;
using log4net;
using Microsoft.Owin.Hosting;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.DAL.EF.Helpers;
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Interface;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.Master.Server
{
    internal class Program
    {
        #region Members

        private static ManualResetEvent run = new ManualResetEvent(true);

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            try
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

                // initialize Logger
                Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));

                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                Console.Title = $"OpenNos Master Server v{fileVersionInfo.ProductVersion}dev";
                string ipAddress = ConfigurationManager.AppSettings["MasterIP"];
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["MasterPort"]);
                string text = $"MASTER SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {port} by OpenNos Team";
                int offset = Console.WindowWidth / 2 + text.Length / 2;
                string separator = new string('=', Console.WindowWidth);
                Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);

                // initialize DB
                if (!DataAccessHelper.Initialize())
                {
                    Console.ReadLine();
                    return;
                }

                Logger.Log.Info(Language.Instance.GetMessageFromKey("CONFIG_LOADED"));

                try
                {
                    // register EF -> GO and GO -> EF mappings
                    RegisterMappings();

                    // configure Services and Service Host
                    IScsServiceApplication server = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(ipAddress, port));
                    server.AddService<ICommunicationService, CommunicationService>(new CommunicationService());
                    server.ClientConnected += OnClientConnected;
                    server.ClientDisconnected += OnClientDisconnected;
                    WebApp.Start<Startup>(url: ConfigurationManager.AppSettings["WebAppURL"]);
                    server.Start();

                    // AUTO SESSION KICK
                    Observable.Interval(TimeSpan.FromMinutes(3)).Subscribe(x =>
                    {
                        Parallel.ForEach(MSManager.Instance.ConnectedAccounts.Where(s => s.LastPulse.AddMinutes(3) <= DateTime.Now), connection =>
                        {
                            CommunicationServiceClient.Instance.KickSession(connection.AccountId, null);
                        });
                    });

                    CommunicationServiceClient.Instance.Authenticate(ConfigurationManager.AppSettings["MasterAuthKey"]);
                    Logger.Log.Info(Language.Instance.GetMessageFromKey("STARTED"));
                    Console.Title = $"MASTER SERVER - Channels :{MSManager.Instance.WorldServers.Count} - Players : {MSManager.Instance.ConnectedAccounts.Count}";
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("General Error Server", ex);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
                Console.ReadKey();
            }
        }

        private static void OnClientConnected(object sender, ServiceClientEventArgs e)
        {
            Logger.Log.Info(Language.Instance.GetMessageFromKey("NEW_CONNECT") + e.Client.ClientId);
        }

        private static void OnClientDisconnected(object sender, ServiceClientEventArgs e)
        {
            Logger.Log.Info(Language.Instance.GetMessageFromKey("DISCONNECT") + e.Client.ClientId);
        }

        private static void RegisterMappings()
        {
            //Prepare mappings for future use

            // register mappings for items
            DaoFactory.IteminstanceDao.RegisterMapping(typeof(BoxInstance));
            DaoFactory.IteminstanceDao.RegisterMapping(typeof(SpecialistInstance));
            DaoFactory.IteminstanceDao.RegisterMapping(typeof(WearableInstance));
            DaoFactory.IteminstanceDao.InitializeMapper(typeof(ItemInstance));

            // entities
            DaoFactory.AccountDao.RegisterMapping(typeof(Account)).InitializeMapper();
            DaoFactory.EquipmentOptionDao.RegisterMapping(typeof(EquipmentOptionDTO)).InitializeMapper();
            DaoFactory.CharacterDao.RegisterMapping(typeof(Character)).InitializeMapper();
            DaoFactory.CharacterRelationDao.RegisterMapping(typeof(CharacterRelationDTO)).InitializeMapper();
            DaoFactory.CharacterSkillDao.RegisterMapping(typeof(CharacterSkill)).InitializeMapper();
            DaoFactory.ComboDao.RegisterMapping(typeof(ComboDTO)).InitializeMapper();
            DaoFactory.DropDao.RegisterMapping(typeof(DropDTO)).InitializeMapper();
            DaoFactory.GeneralLogDao.RegisterMapping(typeof(GeneralLogDTO)).InitializeMapper();
            DaoFactory.ItemDao.RegisterMapping(typeof(ItemDTO)).InitializeMapper();
            DaoFactory.BazaarItemDao.RegisterMapping(typeof(BazaarItemDTO)).InitializeMapper();
            DaoFactory.MailDao.RegisterMapping(typeof(MailDTO)).InitializeMapper();
            DaoFactory.MapDao.RegisterMapping(typeof(MapDTO)).InitializeMapper();
            DaoFactory.MapMonsterDao.RegisterMapping(typeof(MapMonster)).InitializeMapper();
            DaoFactory.MapNpcDao.RegisterMapping(typeof(MapNpc)).InitializeMapper();
            DaoFactory.FamilyDao.RegisterMapping(typeof(FamilyDTO)).InitializeMapper();
            DaoFactory.FamilyCharacterDao.RegisterMapping(typeof(FamilyCharacterDTO)).InitializeMapper();
            DaoFactory.FamilyLogDao.RegisterMapping(typeof(FamilyLogDTO)).InitializeMapper();
            DaoFactory.MapTypeDao.RegisterMapping(typeof(MapTypeDTO)).InitializeMapper();
            DaoFactory.MapTypeMapDao.RegisterMapping(typeof(MapTypeMapDTO)).InitializeMapper();
            DaoFactory.NpcMonsterDao.RegisterMapping(typeof(NpcMonster)).InitializeMapper();
            DaoFactory.NpcMonsterSkillDao.RegisterMapping(typeof(NpcMonsterSkill)).InitializeMapper();
            DaoFactory.PenaltyLogDao.RegisterMapping(typeof(PenaltyLogDTO)).InitializeMapper();
            DaoFactory.PortalDao.RegisterMapping(typeof(PortalDTO)).InitializeMapper();
            DaoFactory.PortalDao.RegisterMapping(typeof(Portal)).InitializeMapper();
            DaoFactory.QuicklistEntryDao.RegisterMapping(typeof(QuicklistEntryDTO)).InitializeMapper();
            DaoFactory.RecipeDao.RegisterMapping(typeof(Recipe)).InitializeMapper();
            DaoFactory.RecipeItemDao.RegisterMapping(typeof(RecipeItemDTO)).InitializeMapper();
            DaoFactory.MinilandObjectDao.RegisterMapping(typeof(MinilandObjectDTO)).InitializeMapper();
            DaoFactory.MinilandObjectDao.RegisterMapping(typeof(MapDesignObject)).InitializeMapper();
            DaoFactory.RespawnDao.RegisterMapping(typeof(RespawnDTO)).InitializeMapper();
            DaoFactory.RespawnMapTypeDao.RegisterMapping(typeof(RespawnMapTypeDTO)).InitializeMapper();
            DaoFactory.ShopDao.RegisterMapping(typeof(Shop)).InitializeMapper();
            DaoFactory.ShopItemDao.RegisterMapping(typeof(ShopItemDTO)).InitializeMapper();
            DaoFactory.ShopSkillDao.RegisterMapping(typeof(ShopSkillDTO)).InitializeMapper();
            DaoFactory.CardDao.RegisterMapping(typeof(CardDTO)).InitializeMapper();
            DaoFactory.BCardDao.RegisterMapping(typeof(BCardDTO)).InitializeMapper();
            DaoFactory.SkillDao.RegisterMapping(typeof(Skill)).InitializeMapper();
            DaoFactory.MateDao.RegisterMapping(typeof(MateDTO)).InitializeMapper();
            DaoFactory.MateDao.RegisterMapping(typeof(Mate)).InitializeMapper();
            DaoFactory.TeleporterDao.RegisterMapping(typeof(TeleporterDTO)).InitializeMapper();
            DaoFactory.StaticBonusDao.RegisterMapping(typeof(StaticBonusDTO)).InitializeMapper();
            DaoFactory.FamilyDao.RegisterMapping(typeof(Family)).InitializeMapper();
            DaoFactory.FamilyCharacterDao.RegisterMapping(typeof(FamilyCharacter)).InitializeMapper();
            DaoFactory.ScriptedInstanceDao.RegisterMapping(typeof(ScriptedInstanceDTO)).InitializeMapper();
            DaoFactory.ScriptedInstanceDao.RegisterMapping(typeof(ScriptedInstance)).InitializeMapper();
        }

        #endregion
    }
}