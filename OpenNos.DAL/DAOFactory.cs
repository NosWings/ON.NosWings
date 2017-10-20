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

using OpenNos.DAL.Interface;
using OpenNos.DAL.EF;

namespace OpenNos.DAL
{
    public class DaoFactory
    {
        #region Members

        private static IAccountDAO _accountDao;
        private static IBazaarItemDAO _bazaarItemDao;
        private static ICardDAO _cardDao;
        private static IBCardDAO _bcardDao;
        private static IRollGeneratedItemDAO _rollGeneratedItemDao;
        private static IEquipmentOptionDAO _equipmentOptionDao;
        private static ICharacterDAO _characterDao;
        private static ICharacterRelationDAO _characterRelationDao;
        private static ICharacterSkillDAO _characterskillDao;
        private static IComboDAO _comboDao;
        private static IDropDAO _dropDao;
        private static IFamilyCharacterDAO _familycharacterDao;
        private static IFamilyDAO _familyDao;
        private static IFamilyLogDAO _familylogDao;
        private static IGeneralLogDAO _generallogDao;
        private static IItemDAO _itemDao;
        private static IItemInstanceDAO _iteminstanceDao;
        private static ILogChatDAO _logChatDao;
        private static ILogCommandsDAO _logCommandsDao;
        private static IMailDAO _mailDao;
        private static IMapDAO _mapDao;
        private static IMapMonsterDAO _mapmonsterDao;
        private static IMapNpcDAO _mapnpcDao;
        private static IMapTypeDAO _maptypeDao;
        private static IMapTypeMapDAO _maptypemapDao;
        private static IMateDAO _mateDao;
        private static IMinilandObjectDAO _minilandobjectDao;
        private static INpcMonsterDAO _npcmonsterDao;
        private static INpcMonsterSkillDAO _npcmonsterskillDao;
        private static IPenaltyLogDAO _penaltylogDao;
        private static IPortalDAO _portalDao;
        private static IQuicklistEntryDAO _quicklistDao;
        private static IRecipeDAO _recipeDao;
        private static IRecipeItemDAO _recipeitemDao;
        private static IRespawnDAO _respawnDao;
        private static IRespawnMapTypeDAO _respawnMapTypeDao;
        private static IScriptedInstanceDAO _scriptedinstanceDao;
        private static IShopDAO _shopDao;
        private static IShopItemDAO _shopitemDao;
        private static IShopSkillDAO _shopskillDao;
        private static ISkillDAO _skillDao;
        private static IStaticBonusDAO _staticBonusDao;
        private static IStaticBuffDAO _staticBuffDao;
        private static ITeleporterDAO _teleporterDao;

        #endregion

        #region Instantiation

        #endregion

        #region Properties

        public static IAccountDAO AccountDao
        {
            get { return _accountDao ?? (_accountDao = new AccountDAO()); }
        }

        public static IBazaarItemDAO BazaarItemDao
        {
            get { return _bazaarItemDao ?? (_bazaarItemDao = new BazaarItemDAO()); }
        }

        public static ICardDAO CardDao
        {
            get { return _cardDao ?? (_cardDao = new CardDAO()); }
        }

        public static IEquipmentOptionDAO EquipmentOptionDao
        {
            get { return _equipmentOptionDao ?? (_equipmentOptionDao = new EquipmentOptionDAO()); }
        }

        public static ICharacterDAO CharacterDao
        {
            get { return _characterDao ?? (_characterDao = new CharacterDAO()); }
        }

        public static ICharacterRelationDAO CharacterRelationDao
        {
            get { return _characterRelationDao ?? (_characterRelationDao = new CharacterRelationDAO()); }
        }

        public static ICharacterSkillDAO CharacterSkillDao
        {
            get { return _characterskillDao ?? (_characterskillDao = new CharacterSkillDAO()); }
        }

        public static IComboDAO ComboDao
        {
            get { return _comboDao ?? (_comboDao = new ComboDAO()); }
        }

        public static IDropDAO DropDao
        {
            get { return _dropDao ?? (_dropDao = new DropDAO()); }
        }

        public static IFamilyCharacterDAO FamilyCharacterDao
        {
            get { return _familycharacterDao ?? (_familycharacterDao = new FamilyCharacterDAO()); }
        }

        public static IFamilyDAO FamilyDao
        {
            get { return _familyDao ?? (_familyDao = new FamilyDAO()); }
        }

        public static IFamilyLogDAO FamilyLogDao
        {
            get { return _familylogDao ?? (_familylogDao = new FamilyLogDAO()); }
        }

        public static IGeneralLogDAO GeneralLogDao
        {
            get { return _generallogDao ?? (_generallogDao = new GeneralLogDAO()); }
        }

        public static IItemDAO ItemDao
        {
            get { return _itemDao ?? (_itemDao = new ItemDAO()); }
        }

        public static IItemInstanceDAO IteminstanceDao
        {
            get { return _iteminstanceDao ?? (_iteminstanceDao = new ItemInstanceDAO()); }
        }

        public static ILogChatDAO LogChatDao
        {
            get { return _logChatDao ?? (_logChatDao = new LogChatDAO()); }
        }

        public static ILogCommandsDAO LogCommandsDao
        {
            get { return _logCommandsDao ?? (_logCommandsDao = new LogCommandsDAO()); }
        }

        public static IMailDAO MailDao
        {
            get { return _mailDao ?? (_mailDao = new MailDAO()); }
        }

        public static IMapDAO MapDao
        {
            get { return _mapDao ?? (_mapDao = new MapDAO()); }
        }

        public static IMapMonsterDAO MapMonsterDao
        {
            get { return _mapmonsterDao ?? (_mapmonsterDao = new MapMonsterDAO()); }
        }

        public static IMapNpcDAO MapNpcDao
        {
            get { return _mapnpcDao ?? (_mapnpcDao = new MapNpcDAO()); }
        }

        public static IMapTypeDAO MapTypeDao
        {
            get { return _maptypeDao ?? (_maptypeDao = new MapTypeDAO()); }
        }

        public static IMapTypeMapDAO MapTypeMapDao
        {
            get { return _maptypemapDao ?? (_maptypemapDao = new MapTypeMapDAO()); }
        }

        public static IMateDAO MateDao
        {
            get { return _mateDao ?? (_mateDao = new MateDAO()); }
        }

        public static IMinilandObjectDAO MinilandObjectDao
        {
            get { return _minilandobjectDao ?? (_minilandobjectDao = new MinilandObjectDAO()); }
        }

        public static INpcMonsterDAO NpcMonsterDao
        {
            get { return _npcmonsterDao ?? (_npcmonsterDao = new NpcMonsterDAO()); }
        }

        public static INpcMonsterSkillDAO NpcMonsterSkillDao
        {
            get { return _npcmonsterskillDao ?? (_npcmonsterskillDao = new NpcMonsterSkillDAO()); }
        }

        public static IPenaltyLogDAO PenaltyLogDao
        {
            get { return _penaltylogDao ?? (_penaltylogDao = new PenaltyLogDAO()); }
        }

        public static IPortalDAO PortalDao
        {
            get { return _portalDao ?? (_portalDao = new PortalDAO()); }
        }

        public static IQuicklistEntryDAO QuicklistEntryDao
        {
            get { return _quicklistDao ?? (_quicklistDao = new QuicklistEntryDAO()); }
        }

        public static IRecipeDAO RecipeDao
        {
            get { return _recipeDao ?? (_recipeDao = new RecipeDAO()); }
        }

        public static IRecipeItemDAO RecipeItemDao
        {
            get { return _recipeitemDao ?? (_recipeitemDao = new RecipeItemDAO()); }
        }

        public static IRespawnDAO RespawnDao
        {
            get { return _respawnDao ?? (_respawnDao = new RespawnDAO()); }
        }

        public static IRespawnMapTypeDAO RespawnMapTypeDao
        {
            get { return _respawnMapTypeDao ?? (_respawnMapTypeDao = new RespawnMapTypeDAO()); }
        }

        public static IShopDAO ShopDao
        {
            get { return _shopDao ?? (_shopDao = new ShopDAO()); }
        }

        public static IShopItemDAO ShopItemDao
        {
            get { return _shopitemDao ?? (_shopitemDao = new ShopItemDAO()); }
        }

        public static IShopSkillDAO ShopSkillDao
        {
            get { return _shopskillDao ?? (_shopskillDao = new ShopSkillDAO()); }
        }

        public static ISkillDAO SkillDao
        {
            get { return _skillDao ?? (_skillDao = new SkillDAO()); }
        }

        public static IStaticBonusDAO StaticBonusDao
        {
            get { return _staticBonusDao ?? (_staticBonusDao = new StaticBonusDAO()); }
        }

        public static IStaticBuffDAO StaticBuffDao
        {
            get { return _staticBuffDao ?? (_staticBuffDao = new StaticBuffDAO()); }
        }

        public static ITeleporterDAO TeleporterDao
        {
            get { return _teleporterDao ?? (_teleporterDao = new TeleporterDAO()); }
        }

        public static IScriptedInstanceDAO ScriptedInstanceDao
        {
            get { return _scriptedinstanceDao ?? (_scriptedinstanceDao = new ScriptedInstanceDAO()); }
        }

        public static IBCardDAO BCardDao
        {
            get { return _bcardDao ?? (_bcardDao = new BCardDAO()); }
        }

        public static IRollGeneratedItemDAO RollGeneratedItemDao
        {
            get { return _rollGeneratedItemDao ?? (_rollGeneratedItemDao = new RollGeneratedItemDAO()); }
        }

        #endregion
    }
}