using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.PenaltyLog", "AccountId", "dbo.Account");
            DropForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account");
            DropForeignKey("dbo.Character", "AccountId", "dbo.Account");
            DropForeignKey("dbo.Respawn", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.QuicklistEntry", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.Mail", "ReceiverId", "dbo.Character");
            DropForeignKey("dbo.Mail", "SenderId", "dbo.Character");
            DropForeignKey("dbo.ItemInstance", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.GeneralLog", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.Character", "FamilyCharacterId", "dbo.FamilyCharacter");
            DropForeignKey("dbo.FamilyCharacter", "FamilyId", "dbo.Family");
            DropForeignKey("dbo.FamilyLog", "FamilyId", "dbo.Family");
            DropForeignKey("dbo.CharacterSkill", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.ShopSkill", "SkillVNum", "dbo.Skill");
            DropForeignKey("dbo.NpcMonsterSkill", "SkillVNum", "dbo.Skill");
            DropForeignKey("dbo.NpcMonsterSkill", "NpcMonsterVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.MapNpc", "NpcVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.MapMonster", "MonsterVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.Drop", "MonsterVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.ShopItem", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.RecipeItem", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.Recipe", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.RecipeItem", "RecipeId", "dbo.Recipe");
            DropForeignKey("dbo.Teleporter", "MapNpcId", "dbo.MapNpc");
            DropForeignKey("dbo.Shop", "MapNpcId", "dbo.MapNpc");
            DropForeignKey("dbo.ShopSkill", "ShopId", "dbo.Shop");
            DropForeignKey("dbo.ShopItem", "ShopId", "dbo.Shop");
            DropForeignKey("dbo.Recipe", "MapNpcId", "dbo.MapNpc");
            DropForeignKey("dbo.Teleporter", "MapId", "dbo.Map");
            DropForeignKey("dbo.Portal", "SourceMapId", "dbo.Map");
            DropForeignKey("dbo.Portal", "DestinationMapId", "dbo.Map");
            DropForeignKey("dbo.MapTypeMap", "MapTypeId", "dbo.MapType");
            DropForeignKey("dbo.MapType", "ReturnMapTypeId", "dbo.RespawnMapType");
            DropForeignKey("dbo.MapType", "RespawnMapTypeId", "dbo.RespawnMapType");
            DropForeignKey("dbo.Respawn", "RespawnMapTypeId", "dbo.RespawnMapType");
            DropForeignKey("dbo.Respawn", "MapId", "dbo.Map");
            DropForeignKey("dbo.RespawnMapType", "DefaultMapId", "dbo.Map");
            DropForeignKey("dbo.Drop", "MapTypeId", "dbo.MapType");
            DropForeignKey("dbo.MapTypeMap", "MapId", "dbo.Map");
            DropForeignKey("dbo.MapNpc", "MapId", "dbo.Map");
            DropForeignKey("dbo.MapMonster", "MapId", "dbo.Map");
            DropForeignKey("dbo.Character", "MapId", "dbo.Map");
            DropForeignKey("dbo.Mail", "AttachmentVNum", "dbo.Item");
            DropForeignKey("dbo.ItemInstance", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.CellonOption", "WearableInstanceId", "dbo.ItemInstance");
            DropForeignKey("dbo.ItemInstance", "BoundCharacterId", "dbo.Character");
            DropForeignKey("dbo.Drop", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.Combo", "SkillVNum", "dbo.Skill");
            DropForeignKey("dbo.CharacterSkill", "SkillVNum", "dbo.Skill");
            DropIndex("dbo.PenaltyLog", new[] { "AccountId" });
            DropIndex("dbo.QuicklistEntry", new[] { "CharacterId" });
            DropIndex("dbo.GeneralLog", new[] { "CharacterId" });
            DropIndex("dbo.GeneralLog", new[] { "AccountId" });
            DropIndex("dbo.FamilyLog", new[] { "FamilyId" });
            DropIndex("dbo.FamilyCharacter", new[] { "FamilyId" });
            DropIndex("dbo.RecipeItem", new[] { "RecipeId" });
            DropIndex("dbo.RecipeItem", new[] { "ItemVNum" });
            DropIndex("dbo.ShopSkill", new[] { "SkillVNum" });
            DropIndex("dbo.ShopSkill", new[] { "ShopId" });
            DropIndex("dbo.ShopItem", new[] { "ShopId" });
            DropIndex("dbo.ShopItem", new[] { "ItemVNum" });
            DropIndex("dbo.Shop", new[] { "MapNpcId" });
            DropIndex("dbo.Teleporter", new[] { "MapNpcId" });
            DropIndex("dbo.Teleporter", new[] { "MapId" });
            DropIndex("dbo.Portal", new[] { "SourceMapId" });
            DropIndex("dbo.Portal", new[] { "DestinationMapId" });
            DropIndex("dbo.Respawn", new[] { "RespawnMapTypeId" });
            DropIndex("dbo.Respawn", new[] { "MapId" });
            DropIndex("dbo.Respawn", new[] { "CharacterId" });
            DropIndex("dbo.RespawnMapType", new[] { "DefaultMapId" });
            DropIndex("dbo.MapType", new[] { "ReturnMapTypeId" });
            DropIndex("dbo.MapType", new[] { "RespawnMapTypeId" });
            DropIndex("dbo.MapTypeMap", new[] { "MapTypeId" });
            DropIndex("dbo.MapTypeMap", new[] { "MapId" });
            DropIndex("dbo.MapMonster", new[] { "MonsterVNum" });
            DropIndex("dbo.MapMonster", new[] { "MapId" });
            DropIndex("dbo.MapNpc", new[] { "NpcVNum" });
            DropIndex("dbo.MapNpc", new[] { "MapId" });
            DropIndex("dbo.Recipe", new[] { "MapNpcId" });
            DropIndex("dbo.Recipe", new[] { "ItemVNum" });
            DropIndex("dbo.Mail", new[] { "SenderId" });
            DropIndex("dbo.Mail", new[] { "ReceiverId" });
            DropIndex("dbo.Mail", new[] { "AttachmentVNum" });
            DropIndex("dbo.CellonOption", new[] { "WearableInstanceId" });
            DropIndex("dbo.ItemInstance", new[] { "ItemVNum" });
            DropIndex("dbo.ItemInstance", "IX_SlotAndType");
            DropIndex("dbo.ItemInstance", new[] { "BoundCharacterId" });
            DropIndex("dbo.Drop", new[] { "MonsterVNum" });
            DropIndex("dbo.Drop", new[] { "MapTypeId" });
            DropIndex("dbo.Drop", new[] { "ItemVNum" });
            DropIndex("dbo.NpcMonsterSkill", new[] { "SkillVNum" });
            DropIndex("dbo.NpcMonsterSkill", new[] { "NpcMonsterVNum" });
            DropIndex("dbo.Combo", new[] { "SkillVNum" });
            DropIndex("dbo.CharacterSkill", new[] { "SkillVNum" });
            DropIndex("dbo.CharacterSkill", new[] { "CharacterId" });
            DropIndex("dbo.Character", new[] { "MapId" });
            DropIndex("dbo.Character", new[] { "FamilyCharacterId" });
            DropIndex("dbo.Character", new[] { "AccountId" });
            DropTable("dbo.PenaltyLog");
            DropTable("dbo.QuicklistEntry");
            DropTable("dbo.GeneralLog");
            DropTable("dbo.FamilyLog");
            DropTable("dbo.Family");
            DropTable("dbo.FamilyCharacter");
            DropTable("dbo.RecipeItem");
            DropTable("dbo.ShopSkill");
            DropTable("dbo.ShopItem");
            DropTable("dbo.Shop");
            DropTable("dbo.Teleporter");
            DropTable("dbo.Portal");
            DropTable("dbo.Respawn");
            DropTable("dbo.RespawnMapType");
            DropTable("dbo.MapType");
            DropTable("dbo.MapTypeMap");
            DropTable("dbo.MapMonster");
            DropTable("dbo.Map");
            DropTable("dbo.MapNpc");
            DropTable("dbo.Recipe");
            DropTable("dbo.Mail");
            DropTable("dbo.CellonOption");
            DropTable("dbo.ItemInstance");
            DropTable("dbo.Item");
            DropTable("dbo.Drop");
            DropTable("dbo.NpcMonster");
            DropTable("dbo.NpcMonsterSkill");
            DropTable("dbo.Combo");
            DropTable("dbo.Skill");
            DropTable("dbo.CharacterSkill");
            DropTable("dbo.Character");
            DropTable("dbo.Account");
        }

        public override void Up()
        {
            CreateTable(
                    "dbo.Account",
                    c => new
                    {
                        AccountId = c.Long(false, true),
                        Authority = c.Short(false),
                        Email = c.String(maxLength: 255),
                        LastCompliment = c.DateTime(false),
                        LastSession = c.Int(false),
                        Name = c.String(maxLength: 255),
                        Password = c.String(maxLength: 255, unicode: false),
                        RegistrationIP = c.String(maxLength: 45),
                        VerificationToken = c.String(maxLength: 32)
                    })
                .PrimaryKey(t => t.AccountId);

            CreateTable(
                    "dbo.Character",
                    c => new
                    {
                        CharacterId = c.Long(false, true),
                        AccountId = c.Long(false),
                        Act4Dead = c.Int(false),
                        Act4Kill = c.Int(false),
                        Act4Points = c.Int(false),
                        ArenaWinner = c.Int(false),
                        Backpack = c.Int(false),
                        Biography = c.String(maxLength: 255),
                        BuffBlocked = c.Boolean(false),
                        Class = c.Byte(false),
                        Compliment = c.Short(false),
                        Dignity = c.Single(false),
                        EmoticonsBlocked = c.Boolean(false),
                        ExchangeBlocked = c.Boolean(false),
                        Faction = c.Int(false),
                        FamilyCharacterId = c.Long(),
                        FamilyRequestBlocked = c.Boolean(false),
                        FriendRequestBlocked = c.Boolean(false),
                        Gender = c.Byte(false),
                        Gold = c.Long(false),
                        GroupRequestBlocked = c.Boolean(false),
                        HairColor = c.Byte(false),
                        HairStyle = c.Byte(false),
                        HeroChatBlocked = c.Boolean(false),
                        HeroLevel = c.Byte(false),
                        HeroXp = c.Long(false),
                        Hp = c.Int(false),
                        HpBlocked = c.Boolean(false),
                        JobLevel = c.Byte(false),
                        JobLevelXp = c.Long(false),
                        LastLogin = c.DateTime(false),
                        Level = c.Byte(false),
                        LevelXp = c.Long(false),
                        MapId = c.Short(false),
                        MapX = c.Short(false),
                        MapY = c.Short(false),
                        MasterPoints = c.Int(false),
                        MasterTicket = c.Int(false),
                        MinilandInviteBlocked = c.Boolean(false),
                        MouseAimLock = c.Boolean(false),
                        Mp = c.Int(false),
                        Name = c.String(maxLength: 255, unicode: false),
                        QuickGetUp = c.Boolean(false),
                        RagePoint = c.Long(false),
                        Reput = c.Long(false),
                        Slot = c.Byte(false),
                        SpAdditionPoint = c.Int(false),
                        SpPoint = c.Int(false),
                        State = c.Byte(false),
                        TalentLose = c.Int(false),
                        TalentSurrender = c.Int(false),
                        TalentWin = c.Int(false),
                        WhisperBlocked = c.Boolean(false)
                    })
                .PrimaryKey(t => t.CharacterId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.FamilyCharacter", t => t.FamilyCharacterId)
                .ForeignKey("dbo.Account", t => t.AccountId)
                .Index(t => t.AccountId)
                .Index(t => t.FamilyCharacterId)
                .Index(t => t.MapId);

            CreateTable(
                    "dbo.CharacterSkill",
                    c => new
                    {
                        Id = c.Guid(false),
                        CharacterId = c.Long(false),
                        SkillVNum = c.Short(false)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId)
                .Index(t => t.SkillVNum);

            CreateTable(
                    "dbo.Skill",
                    c => new
                    {
                        SkillVNum = c.Short(false),
                        AttackAnimation = c.Short(false),
                        BuffId = c.Short(false),
                        CastAnimation = c.Short(false),
                        CastEffect = c.Short(false),
                        CastId = c.Short(false),
                        CastTime = c.Short(false),
                        Class = c.Byte(false),
                        Cooldown = c.Short(false),
                        CPCost = c.Byte(false),
                        Damage = c.Short(false),
                        Duration = c.Short(false),
                        Effect = c.Short(false),
                        Element = c.Byte(false),
                        ElementalDamage = c.Short(false),
                        HitType = c.Byte(false),
                        ItemVNum = c.Short(false),
                        Level = c.Byte(false),
                        LevelMinimum = c.Byte(false),
                        MinimumAdventurerLevel = c.Byte(false),
                        MinimumArcherLevel = c.Byte(false),
                        MinimumMagicianLevel = c.Byte(false),
                        MinimumSwordmanLevel = c.Byte(false),
                        MpCost = c.Short(false),
                        Name = c.String(maxLength: 255),
                        Price = c.Int(false),
                        Range = c.Byte(false),
                        SecondarySkillVNum = c.Short(false),
                        SkillChance = c.Short(false),
                        SkillType = c.Byte(false),
                        TargetRange = c.Byte(false),
                        TargetType = c.Byte(false),
                        Type = c.Byte(false),
                        UpgradeSkill = c.Short(false),
                        UpgradeType = c.Short(false)
                    })
                .PrimaryKey(t => t.SkillVNum);

            CreateTable(
                    "dbo.Combo",
                    c => new
                    {
                        ComboId = c.Int(false, true),
                        Animation = c.Short(false),
                        Effect = c.Short(false),
                        Hit = c.Short(false),
                        SkillVNum = c.Short(false)
                    })
                .PrimaryKey(t => t.ComboId)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .Index(t => t.SkillVNum);

            CreateTable(
                    "dbo.NpcMonsterSkill",
                    c => new
                    {
                        NpcMonsterSkillId = c.Long(false, true),
                        NpcMonsterVNum = c.Short(false),
                        Rate = c.Short(false),
                        SkillVNum = c.Short(false)
                    })
                .PrimaryKey(t => t.NpcMonsterSkillId)
                .ForeignKey("dbo.NpcMonster", t => t.NpcMonsterVNum)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .Index(t => t.NpcMonsterVNum)
                .Index(t => t.SkillVNum);

            CreateTable(
                    "dbo.NpcMonster",
                    c => new
                    {
                        NpcMonsterVNum = c.Short(false),
                        AmountRequired = c.Byte(false),
                        AttackClass = c.Byte(false),
                        AttackUpgrade = c.Byte(false),
                        BasicArea = c.Byte(false),
                        BasicCooldown = c.Short(false),
                        BasicRange = c.Byte(false),
                        BasicSkill = c.Short(false),
                        CloseDefence = c.Short(false),
                        Concentrate = c.Short(false),
                        CriticalChance = c.Byte(false),
                        CriticalRate = c.Short(false),
                        DamageMaximum = c.Short(false),
                        DamageMinimum = c.Short(false),
                        DarkResistance = c.Short(false),
                        DefenceDodge = c.Short(false),
                        DefenceUpgrade = c.Byte(false),
                        DistanceDefence = c.Short(false),
                        DistanceDefenceDodge = c.Short(false),
                        Element = c.Byte(false),
                        ElementRate = c.Short(false),
                        FireResistance = c.Short(false),
                        HeroLevel = c.Byte(false),
                        IsHostile = c.Boolean(false),
                        JobXP = c.Int(false),
                        Level = c.Byte(false),
                        LightResistance = c.Short(false),
                        MagicDefence = c.Short(false),
                        MaxHP = c.Int(false),
                        MaxMP = c.Int(false),
                        MonsterType = c.Byte(false),
                        Name = c.String(maxLength: 255),
                        NoAggresiveIcon = c.Boolean(false),
                        NoticeRange = c.Byte(false),
                        Race = c.Byte(false),
                        RaceType = c.Byte(false),
                        RespawnTime = c.Int(false),
                        Speed = c.Byte(false),
                        VNumRequired = c.Short(false),
                        WaterResistance = c.Short(false),
                        XP = c.Int(false)
                    })
                .PrimaryKey(t => t.NpcMonsterVNum);

            CreateTable(
                    "dbo.Drop",
                    c => new
                    {
                        DropId = c.Short(false, true),
                        Amount = c.Int(false),
                        DropChance = c.Int(false),
                        ItemVNum = c.Short(false),
                        MapTypeId = c.Short(),
                        MonsterVNum = c.Short()
                    })
                .PrimaryKey(t => t.DropId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .ForeignKey("dbo.MapType", t => t.MapTypeId)
                .ForeignKey("dbo.NpcMonster", t => t.MonsterVNum)
                .Index(t => t.ItemVNum)
                .Index(t => t.MapTypeId)
                .Index(t => t.MonsterVNum);

            CreateTable(
                    "dbo.Item",
                    c => new
                    {
                        VNum = c.Short(false),
                        BasicUpgrade = c.Byte(false),
                        CellonLvl = c.Byte(false),
                        Class = c.Byte(false),
                        CloseDefence = c.Short(false),
                        Color = c.Byte(false),
                        Concentrate = c.Short(false),
                        CriticalLuckRate = c.Byte(false),
                        CriticalRate = c.Short(false),
                        DamageMaximum = c.Short(false),
                        DamageMinimum = c.Short(false),
                        DarkElement = c.Byte(false),
                        DarkResistance = c.Short(false),
                        DefenceDodge = c.Short(false),
                        DistanceDefence = c.Short(false),
                        DistanceDefenceDodge = c.Short(false),
                        Effect = c.Short(false),
                        EffectValue = c.Int(false),
                        Element = c.Byte(false),
                        ElementRate = c.Short(false),
                        EquipmentSlot = c.Byte(false),
                        FireElement = c.Byte(false),
                        FireResistance = c.Short(false),
                        HitRate = c.Short(false),
                        Hp = c.Short(false),
                        HpRegeneration = c.Short(false),
                        IsBlocked = c.Boolean(false),
                        IsColored = c.Boolean(false),
                        IsConsumable = c.Boolean(false),
                        IsDroppable = c.Boolean(false),
                        IsHeroic = c.Boolean(false),
                        IsHolder = c.Boolean(false),
                        IsMinilandObject = c.Boolean(false),
                        IsSoldable = c.Boolean(false),
                        IsTradable = c.Boolean(false),
                        ItemSubType = c.Byte(false),
                        ItemType = c.Byte(false),
                        ItemValidTime = c.Long(false),
                        LevelJobMinimum = c.Byte(false),
                        LevelMinimum = c.Byte(false),
                        LightElement = c.Byte(false),
                        LightResistance = c.Short(false),
                        MagicDefence = c.Short(false),
                        MaxCellon = c.Byte(false),
                        MaxCellonLvl = c.Byte(false),
                        MaxElementRate = c.Short(false),
                        MaximumAmmo = c.Byte(false),
                        MoreHp = c.Short(false),
                        MoreMp = c.Short(false),
                        Morph = c.Short(false),
                        Mp = c.Short(false),
                        MpRegeneration = c.Short(false),
                        Name = c.String(maxLength: 255),
                        Price = c.Long(false),
                        PvpDefence = c.Short(false),
                        PvpStrength = c.Byte(false),
                        ReduceOposantResistance = c.Short(false),
                        ReputationMinimum = c.Byte(false),
                        ReputPrice = c.Long(false),
                        SecondaryElement = c.Byte(false),
                        Sex = c.Byte(false),
                        Speed = c.Byte(false),
                        SpType = c.Byte(false),
                        Type = c.Byte(false),
                        WaitDelay = c.Short(false),
                        WaterElement = c.Byte(false),
                        WaterResistance = c.Short(false)
                    })
                .PrimaryKey(t => t.VNum);

            CreateTable(
                    "dbo.ItemInstance",
                    c => new
                    {
                        Id = c.Guid(false),
                        Amount = c.Int(false),
                        BoundCharacterId = c.Long(),
                        CharacterId = c.Long(false),
                        Design = c.Short(false),
                        DurabilityPoint = c.Int(false),
                        ItemDeleteTime = c.DateTime(),
                        ItemVNum = c.Short(false),
                        Rare = c.Short(false),
                        Slot = c.Short(false),
                        Type = c.Byte(false),
                        Upgrade = c.Byte(false),
                        HP = c.Short(),
                        MP = c.Short(),
                        Ammo = c.Byte(),
                        Cellon = c.Byte(),
                        CellonOptionId = c.Guid(),
                        CloseDefence = c.Short(),
                        Concentrate = c.Short(),
                        CriticalDodge = c.Short(),
                        CriticalLuckRate = c.Byte(),
                        CriticalRate = c.Short(),
                        DamageMaximum = c.Short(),
                        DamageMinimum = c.Short(),
                        DarkElement = c.Byte(),
                        DarkResistance = c.Short(),
                        DefenceDodge = c.Short(),
                        DistanceDefence = c.Short(),
                        DistanceDefenceDodge = c.Short(),
                        ElementRate = c.Short(),
                        FireElement = c.Byte(),
                        FireResistance = c.Short(),
                        HitRate = c.Short(),
                        HP1 = c.Short(),
                        IsEmpty = c.Boolean(),
                        IsFixed = c.Boolean(),
                        LightElement = c.Byte(),
                        LightResistance = c.Short(),
                        MagicDefence = c.Short(),
                        MaxElementRate = c.Short(),
                        MP1 = c.Short(),
                        WaterElement = c.Byte(),
                        WaterResistance = c.Short(),
                        XP = c.Long(),
                        SlDamage = c.Short(),
                        SlDefence = c.Short(),
                        SlElement = c.Short(),
                        SlHP = c.Short(),
                        SpDamage = c.Byte(),
                        SpDark = c.Byte(),
                        SpDefence = c.Byte(),
                        SpElement = c.Byte(),
                        SpFire = c.Byte(),
                        SpHP = c.Byte(),
                        SpLevel = c.Byte(),
                        SpLight = c.Byte(),
                        SpStoneUpgrade = c.Byte(),
                        SpWater = c.Byte(),
                        Discriminator = c.String(false, 128)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Character", t => t.BoundCharacterId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.BoundCharacterId)
                .Index(t => new { t.CharacterId, t.Slot, t.Type }, "IX_SlotAndType")
                .Index(t => t.ItemVNum);

            CreateTable(
                    "dbo.CellonOption",
                    c => new
                    {
                        Id = c.Guid(false),
                        Level = c.Byte(false),
                        Type = c.Byte(false),
                        Value = c.Int(false),
                        WearableInstanceId = c.Guid(false)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItemInstance", t => t.WearableInstanceId, true)
                .Index(t => t.WearableInstanceId);

            CreateTable(
                    "dbo.Mail",
                    c => new
                    {
                        MailId = c.Long(false, true),
                        AttachmentAmount = c.Byte(false),
                        AttachmentRarity = c.Byte(false),
                        AttachmentUpgrade = c.Byte(false),
                        AttachmentVNum = c.Short(),
                        Date = c.DateTime(false),
                        EqPacket = c.String(maxLength: 255),
                        IsOpened = c.Boolean(false),
                        IsSenderCopy = c.Boolean(false),
                        Message = c.String(maxLength: 255),
                        ReceiverId = c.Long(false),
                        SenderClass = c.Byte(false),
                        SenderGender = c.Byte(false),
                        SenderHairColor = c.Byte(false),
                        SenderHairStyle = c.Byte(false),
                        SenderId = c.Long(false),
                        SenderMorphId = c.Short(false),
                        Title = c.String(maxLength: 255)
                    })
                .PrimaryKey(t => t.MailId)
                .ForeignKey("dbo.Item", t => t.AttachmentVNum)
                .ForeignKey("dbo.Character", t => t.SenderId)
                .ForeignKey("dbo.Character", t => t.ReceiverId)
                .Index(t => t.AttachmentVNum)
                .Index(t => t.ReceiverId)
                .Index(t => t.SenderId);

            CreateTable(
                    "dbo.Recipe",
                    c => new
                    {
                        RecipeId = c.Short(false, true),
                        Amount = c.Byte(false),
                        ItemVNum = c.Short(false),
                        MapNpcId = c.Int(false)
                    })
                .PrimaryKey(t => t.RecipeId)
                .ForeignKey("dbo.MapNpc", t => t.MapNpcId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .Index(t => t.ItemVNum)
                .Index(t => t.MapNpcId);

            CreateTable(
                    "dbo.MapNpc",
                    c => new
                    {
                        MapNpcId = c.Int(false),
                        Dialog = c.Short(false),
                        Effect = c.Short(false),
                        EffectDelay = c.Short(false),
                        IsDisabled = c.Boolean(false),
                        IsMoving = c.Boolean(false),
                        IsSitting = c.Boolean(false),
                        MapId = c.Short(false),
                        MapX = c.Short(false),
                        MapY = c.Short(false),
                        NpcVNum = c.Short(false),
                        Position = c.Byte(false)
                    })
                .PrimaryKey(t => t.MapNpcId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.NpcMonster", t => t.NpcVNum)
                .Index(t => t.MapId)
                .Index(t => t.NpcVNum);

            CreateTable(
                    "dbo.Map",
                    c => new
                    {
                        MapId = c.Short(false),
                        Data = c.Binary(),
                        Music = c.Int(false),
                        Name = c.String(maxLength: 255),
                        ShopAllowed = c.Boolean(false)
                    })
                .PrimaryKey(t => t.MapId);

            CreateTable(
                    "dbo.MapMonster",
                    c => new
                    {
                        MapMonsterId = c.Int(false),
                        IsDisabled = c.Boolean(false),
                        IsMoving = c.Boolean(false),
                        MapId = c.Short(false),
                        MapX = c.Short(false),
                        MapY = c.Short(false),
                        MonsterVNum = c.Short(false),
                        Position = c.Byte(false)
                    })
                .PrimaryKey(t => t.MapMonsterId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.NpcMonster", t => t.MonsterVNum)
                .Index(t => t.MapId)
                .Index(t => t.MonsterVNum);

            CreateTable(
                    "dbo.MapTypeMap",
                    c => new
                    {
                        MapId = c.Short(false),
                        MapTypeId = c.Short(false)
                    })
                .PrimaryKey(t => new { t.MapId, t.MapTypeId })
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.MapType", t => t.MapTypeId)
                .Index(t => t.MapId)
                .Index(t => t.MapTypeId);

            CreateTable(
                    "dbo.MapType",
                    c => new
                    {
                        MapTypeId = c.Short(false, true),
                        MapTypeName = c.String(),
                        PotionDelay = c.Short(false),
                        RespawnMapTypeId = c.Long(),
                        ReturnMapTypeId = c.Long()
                    })
                .PrimaryKey(t => t.MapTypeId)
                .ForeignKey("dbo.RespawnMapType", t => t.RespawnMapTypeId)
                .ForeignKey("dbo.RespawnMapType", t => t.ReturnMapTypeId)
                .Index(t => t.RespawnMapTypeId)
                .Index(t => t.ReturnMapTypeId);

            CreateTable(
                    "dbo.RespawnMapType",
                    c => new
                    {
                        RespawnMapTypeId = c.Long(false),
                        DefaultMapId = c.Short(false),
                        DefaultX = c.Short(false),
                        DefaultY = c.Short(false),
                        Name = c.String(maxLength: 255)
                    })
                .PrimaryKey(t => t.RespawnMapTypeId)
                .ForeignKey("dbo.Map", t => t.DefaultMapId)
                .Index(t => t.DefaultMapId);

            CreateTable(
                    "dbo.Respawn",
                    c => new
                    {
                        RespawnId = c.Long(false, true),
                        CharacterId = c.Long(false),
                        MapId = c.Short(false),
                        RespawnMapTypeId = c.Long(false),
                        X = c.Short(false),
                        Y = c.Short(false)
                    })
                .PrimaryKey(t => t.RespawnId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.RespawnMapType", t => t.RespawnMapTypeId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId)
                .Index(t => t.MapId)
                .Index(t => t.RespawnMapTypeId);

            CreateTable(
                    "dbo.Portal",
                    c => new
                    {
                        PortalId = c.Int(false, true),
                        DestinationMapId = c.Short(false),
                        DestinationX = c.Short(false),
                        DestinationY = c.Short(false),
                        IsDisabled = c.Boolean(false),
                        SourceMapId = c.Short(false),
                        SourceX = c.Short(false),
                        SourceY = c.Short(false),
                        Type = c.Short(false)
                    })
                .PrimaryKey(t => t.PortalId)
                .ForeignKey("dbo.Map", t => t.DestinationMapId)
                .ForeignKey("dbo.Map", t => t.SourceMapId)
                .Index(t => t.DestinationMapId)
                .Index(t => t.SourceMapId);

            CreateTable(
                    "dbo.Teleporter",
                    c => new
                    {
                        TeleporterId = c.Short(false, true),
                        Index = c.Short(false),
                        MapId = c.Short(false),
                        MapNpcId = c.Int(false),
                        MapX = c.Short(false),
                        MapY = c.Short(false)
                    })
                .PrimaryKey(t => t.TeleporterId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.MapNpc", t => t.MapNpcId)
                .Index(t => t.MapId)
                .Index(t => t.MapNpcId);

            CreateTable(
                    "dbo.Shop",
                    c => new
                    {
                        ShopId = c.Int(false, true),
                        MapNpcId = c.Int(false),
                        MenuType = c.Byte(false),
                        Name = c.String(maxLength: 255),
                        ShopType = c.Byte(false)
                    })
                .PrimaryKey(t => t.ShopId)
                .ForeignKey("dbo.MapNpc", t => t.MapNpcId)
                .Index(t => t.MapNpcId);

            CreateTable(
                    "dbo.ShopItem",
                    c => new
                    {
                        ShopItemId = c.Int(false, true),
                        Color = c.Byte(false),
                        ItemVNum = c.Short(false),
                        Rare = c.Short(false),
                        ShopId = c.Int(false),
                        Slot = c.Byte(false),
                        Type = c.Byte(false),
                        Upgrade = c.Byte(false)
                    })
                .PrimaryKey(t => t.ShopItemId)
                .ForeignKey("dbo.Shop", t => t.ShopId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .Index(t => t.ItemVNum)
                .Index(t => t.ShopId);

            CreateTable(
                    "dbo.ShopSkill",
                    c => new
                    {
                        ShopSkillId = c.Int(false, true),
                        ShopId = c.Int(false),
                        SkillVNum = c.Short(false),
                        Slot = c.Byte(false),
                        Type = c.Byte(false)
                    })
                .PrimaryKey(t => t.ShopSkillId)
                .ForeignKey("dbo.Shop", t => t.ShopId)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .Index(t => t.ShopId)
                .Index(t => t.SkillVNum);

            CreateTable(
                    "dbo.RecipeItem",
                    c => new
                    {
                        RecipeItemId = c.Short(false, true),
                        Amount = c.Byte(false),
                        ItemVNum = c.Short(false),
                        RecipeId = c.Short(false)
                    })
                .PrimaryKey(t => t.RecipeItemId)
                .ForeignKey("dbo.Recipe", t => t.RecipeId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .Index(t => t.ItemVNum)
                .Index(t => t.RecipeId);

            CreateTable(
                    "dbo.FamilyCharacter",
                    c => new
                    {
                        FamilyCharacterId = c.Long(false, true),
                        Authority = c.Byte(false),
                        DailyMessage = c.String(maxLength: 255),
                        Experience = c.Int(false),
                        FamilyId = c.Long(false),
                        JoinDate = c.DateTime(false),
                        Rank = c.Byte(false)
                    })
                .PrimaryKey(t => t.FamilyCharacterId)
                .ForeignKey("dbo.Family", t => t.FamilyId)
                .Index(t => t.FamilyId);

            CreateTable(
                    "dbo.Family",
                    c => new
                    {
                        FamilyId = c.Long(false, true),
                        FamilyExperience = c.Int(false),
                        FamilyLevel = c.Byte(false),
                        FamilyMessage = c.String(maxLength: 255),
                        MaxSize = c.Byte(false),
                        Name = c.String(maxLength: 255),
                        Size = c.Byte(false)
                    })
                .PrimaryKey(t => t.FamilyId);

            CreateTable(
                    "dbo.FamilyLog",
                    c => new
                    {
                        FamilyLogId = c.Long(false, true),
                        FamilyId = c.Long(false)
                    })
                .PrimaryKey(t => t.FamilyLogId)
                .ForeignKey("dbo.Family", t => t.FamilyId)
                .Index(t => t.FamilyId);

            CreateTable(
                    "dbo.GeneralLog",
                    c => new
                    {
                        LogId = c.Long(false, true),
                        AccountId = c.Long(false),
                        CharacterId = c.Long(),
                        IpAddress = c.String(maxLength: 255),
                        LogData = c.String(maxLength: 255),
                        LogType = c.String(),
                        Timestamp = c.DateTime(false)
                    })
                .PrimaryKey(t => t.LogId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .ForeignKey("dbo.Account", t => t.AccountId)
                .Index(t => t.AccountId)
                .Index(t => t.CharacterId);

            CreateTable(
                    "dbo.QuicklistEntry",
                    c => new
                    {
                        Id = c.Guid(false),
                        CharacterId = c.Long(false),
                        Morph = c.Short(false),
                        Pos = c.Short(false),
                        Q1 = c.Short(false),
                        Q2 = c.Short(false),
                        Slot = c.Short(false),
                        Type = c.Short(false)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId);

            CreateTable(
                    "dbo.PenaltyLog",
                    c => new
                    {
                        PenaltyLogId = c.Int(false, true),
                        AccountId = c.Long(false),
                        AdminName = c.String(),
                        DateEnd = c.DateTime(false),
                        DateStart = c.DateTime(false),
                        Penalty = c.Byte(false),
                        Reason = c.String(maxLength: 255)
                    })
                .PrimaryKey(t => t.PenaltyLogId)
                .ForeignKey("dbo.Account", t => t.AccountId)
                .Index(t => t.AccountId);
        }

        #endregion
    }
}