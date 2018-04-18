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

using OpenNos.DAL.EF;

namespace OpenNos.DAL
{
    public class DaoFactory
    {
        #region Members

        private static AccountDAO _accountDao;
        private static BazaarItemDAO _bazaarItemDao;
        private static CardDAO _cardDao;
        private static BCardDAO _bcardDao;
        private static RollGeneratedItemDAO _rollGeneratedItemDao;
        private static EquipmentOptionDAO _equipmentOptionDao;
        private static CharacterDAO _characterDao;
        private static CharacterRelationDAO _characterRelationDao;
        private static CharacterSkillDAO _characterskillDao;
        private static CharacterQuestDAO _characterQuestDao;
        private static ComboDAO _comboDao;
        private static DropDAO _dropDao;
        private static FamilyCharacterDAO _familycharacterDao;
        private static FamilyDAO _familyDao;
        private static FamilyLogDAO _familylogDao;
        private static GeneralLogDAO _generallogDao;
        private static ItemDAO _itemDao;
        private static ItemInstanceDAO _iteminstanceDao;
        private static LogChatDAO _logChatDao;
        private static LogCommandsDAO _logCommandsDao;
        private static LogVIPDAO _logVipDao;
        private static MailDAO _mailDao;
        private static MapDAO _mapDao;
        private static MapMonsterDAO _mapmonsterDao;
        private static MapNpcDAO _mapnpcDao;
        private static MapTypeDAO _maptypeDao;
        private static MapTypeMapDAO _maptypemapDao;
        private static MateDAO _mateDao;
        private static MinilandObjectDAO _minilandobjectDao;
        private static NpcMonsterDAO _npcmonsterDao;
        private static NpcMonsterSkillDAO _npcmonsterskillDao;
        private static PenaltyLogDAO _penaltylogDao;
        private static PortalDAO _portalDao;
        private static QuestDAO _questDao;
        private static QuestLogDAO _questLogDao;
        private static QuestRewardDAO _questRewardDao;
        private static QuestObjectiveDAO _questObjectiveDao;
        private static QuicklistEntryDAO _quicklistDao;
        private static RaidLogDAO _raidLogDao;
        private static RecipeDAO _recipeDao;
        private static RecipeItemDAO _recipeitemDao;
        private static RespawnDAO _respawnDao;
        private static RespawnMapTypeDAO _respawnMapTypeDao;
        private static ScriptedInstanceDAO _scriptedinstanceDao;
        private static ShopDAO _shopDao;
        private static ShopItemDAO _shopitemDao;
        private static ShopSkillDAO _shopskillDao;
        private static SkillDAO _skillDao;
        private static StaticBonusDAO _staticBonusDao;
        private static StaticBuffDAO _staticBuffDao;
        private static TeleporterDAO _teleporterDao;

        #endregion

        #region Instantiation

        #endregion

        #region Properties

        public static AccountDAO AccountDao => _accountDao ?? (_accountDao = new AccountDAO());

        public static BazaarItemDAO BazaarItemDao => _bazaarItemDao ?? (_bazaarItemDao = new BazaarItemDAO());

        public static CardDAO CardDao => _cardDao ?? (_cardDao = new CardDAO());

        public static EquipmentOptionDAO EquipmentOptionDao => _equipmentOptionDao ?? (_equipmentOptionDao = new EquipmentOptionDAO());

        public static CharacterDAO CharacterDao => _characterDao ?? (_characterDao = new CharacterDAO());

        public static CharacterRelationDAO CharacterRelationDao => _characterRelationDao ?? (_characterRelationDao = new CharacterRelationDAO());

        public static CharacterSkillDAO CharacterSkillDao => _characterskillDao ?? (_characterskillDao = new CharacterSkillDAO());

        public static CharacterQuestDAO CharacterQuestDao => _characterQuestDao ?? (_characterQuestDao = new CharacterQuestDAO());

        public static ComboDAO ComboDao => _comboDao ?? (_comboDao = new ComboDAO());

        public static DropDAO DropDao => _dropDao ?? (_dropDao = new DropDAO());

        public static FamilyCharacterDAO FamilyCharacterDao => _familycharacterDao ?? (_familycharacterDao = new FamilyCharacterDAO());

        public static FamilyDAO FamilyDao => _familyDao ?? (_familyDao = new FamilyDAO());

        public static FamilyLogDAO FamilyLogDao => _familylogDao ?? (_familylogDao = new FamilyLogDAO());

        public static GeneralLogDAO GeneralLogDao => _generallogDao ?? (_generallogDao = new GeneralLogDAO());

        public static ItemDAO ItemDao => _itemDao ?? (_itemDao = new ItemDAO());

        public static ItemInstanceDAO IteminstanceDao => _iteminstanceDao ?? (_iteminstanceDao = new ItemInstanceDAO());

        public static LogChatDAO LogChatDao => _logChatDao ?? (_logChatDao = new LogChatDAO());

        public static LogCommandsDAO LogCommandsDao => _logCommandsDao ?? (_logCommandsDao = new LogCommandsDAO());

        public static LogVIPDAO LogVipDao => _logVipDao ?? (_logVipDao = new LogVIPDAO());

        public static MailDAO MailDao => _mailDao ?? (_mailDao = new MailDAO());

        public static MapDAO MapDao => _mapDao ?? (_mapDao = new MapDAO());

        public static MapMonsterDAO MapMonsterDao => _mapmonsterDao ?? (_mapmonsterDao = new MapMonsterDAO());

        public static MapNpcDAO MapNpcDao => _mapnpcDao ?? (_mapnpcDao = new MapNpcDAO());

        public static MapTypeDAO MapTypeDao => _maptypeDao ?? (_maptypeDao = new MapTypeDAO());

        public static MapTypeMapDAO MapTypeMapDao => _maptypemapDao ?? (_maptypemapDao = new MapTypeMapDAO());

        public static MateDAO MateDao => _mateDao ?? (_mateDao = new MateDAO());

        public static MinilandObjectDAO MinilandObjectDao => _minilandobjectDao ?? (_minilandobjectDao = new MinilandObjectDAO());

        public static NpcMonsterDAO NpcMonsterDao => _npcmonsterDao ?? (_npcmonsterDao = new NpcMonsterDAO());

        public static NpcMonsterSkillDAO NpcMonsterSkillDao => _npcmonsterskillDao ?? (_npcmonsterskillDao = new NpcMonsterSkillDAO());

        public static PenaltyLogDAO PenaltyLogDao => _penaltylogDao ?? (_penaltylogDao = new PenaltyLogDAO());

        public static PortalDAO PortalDao => _portalDao ?? (_portalDao = new PortalDAO());

        public static QuestDAO QuestDao => _questDao ?? (_questDao = new QuestDAO());

        public static QuestLogDAO QuestLogDao => _questLogDao ?? (_questLogDao = new QuestLogDAO());

        public static QuestObjectiveDAO QuestObjectiveDao => _questObjectiveDao ?? (_questObjectiveDao = new QuestObjectiveDAO());

        public static QuestRewardDAO QuestRewardDao => _questRewardDao ?? (_questRewardDao = new QuestRewardDAO());

        public static QuicklistEntryDAO QuicklistEntryDao => _quicklistDao ?? (_quicklistDao = new QuicklistEntryDAO());

        public static RaidLogDAO RaidLogDao => _raidLogDao ?? (_raidLogDao = new RaidLogDAO());

        public static RecipeDAO RecipeDao => _recipeDao ?? (_recipeDao = new RecipeDAO());

        public static RecipeItemDAO RecipeItemDao => _recipeitemDao ?? (_recipeitemDao = new RecipeItemDAO());

        public static RespawnDAO RespawnDao => _respawnDao ?? (_respawnDao = new RespawnDAO());

        public static RespawnMapTypeDAO RespawnMapTypeDao => _respawnMapTypeDao ?? (_respawnMapTypeDao = new RespawnMapTypeDAO());

        public static ShopDAO ShopDao => _shopDao ?? (_shopDao = new ShopDAO());

        public static ShopItemDAO ShopItemDao => _shopitemDao ?? (_shopitemDao = new ShopItemDAO());

        public static ShopSkillDAO ShopSkillDao => _shopskillDao ?? (_shopskillDao = new ShopSkillDAO());

        public static SkillDAO SkillDao => _skillDao ?? (_skillDao = new SkillDAO());

        public static StaticBonusDAO StaticBonusDao => _staticBonusDao ?? (_staticBonusDao = new StaticBonusDAO());

        public static StaticBuffDAO StaticBuffDao => _staticBuffDao ?? (_staticBuffDao = new StaticBuffDAO());

        public static TeleporterDAO TeleporterDao => _teleporterDao ?? (_teleporterDao = new TeleporterDAO());

        public static ScriptedInstanceDAO ScriptedInstanceDao => _scriptedinstanceDao ?? (_scriptedinstanceDao = new ScriptedInstanceDAO());

        public static BCardDAO BCardDao => _bcardDao ?? (_bcardDao = new BCardDAO());

        public static RollGeneratedItemDAO RollGeneratedItemDao => _rollGeneratedItemDao ?? (_rollGeneratedItemDao = new RollGeneratedItemDAO());

        #endregion
    }
}