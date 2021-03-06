﻿using ArkSavegameToolkitNet.Data;
using ArkSavegameToolkitNet.Exceptions;
using ArkSavegameToolkitNet.Property;
using ArkSavegameToolkitNet.Types;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkSavegameToolkitNet
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GameObject : IGameObject, INameContainer
    {
        private static ILog _logger = LogManager.GetLogger(typeof(GameObject));
        private static readonly IDictionary<Tuple<long, long>, Guid> _uuidCache = new ConcurrentDictionary<Tuple<long, long>, Guid>();
        protected internal int _propertiesOffset;

        [JsonProperty]
        public int ObjectId { get; set; }
        [JsonProperty]
        public Guid Uuid { get; set; }
        [JsonProperty]
        public ArkName ClassName { get; set; }
        public bool IsItem { get; set; }
        [JsonProperty]
        public IList<ArkName> Names { get; set; }
        public bool UnkBool { get; set; }
        public int UnkIndex { get; set; }
        [JsonProperty]
        public LocationData Location { get; set; }
        [JsonProperty]
        public IExtraData ExtraData { get; set; }

        //query helper fields
        public bool IsCreature { get; set; }
        public bool IsTamedCreature { get; set; }
        public bool IsWildCreature { get; set; }
        public bool IsRaftCreature { get; set; }
        public bool IsStructure { get; set; }
        public bool IsInventory { get; set; }
        public bool IsTamedCreatureInventory { get; set; }
        public bool IsWildCreatureInventory { get; set; }
        public bool IsStructureInventory { get; set; }
        public bool IsPlayerCharacterInventory { get; set; }
        public bool IsStatusComponent { get; set; }
        public bool IsDinoStatusComponent { get; set; }
        public bool IsPlayerCharacterStatusComponent { get; set; }
        public bool IsDroppedItem { get; set; }
        public bool IsPlayerCharacter { get; set; }
        public bool IsStructurePaintingComponent { get; set; }
        public bool IsSomethingElse { get; set; }

        private ArkNameCache _arkNameCache;

        public GameObject() { _arkNameCache = new ArkNameCache(); }

        public GameObject(ArkArchive archive, ArkNameCache arkNameCache = null)
        {
            _arkNameCache = arkNameCache ?? new ArkNameCache();
            read(archive);
        }

        [JsonProperty]
        public IDictionary<ArkName, IProperty> Properties
        {
            get
            {
                return properties;
            }
            set
            {
                if (value == null) throw new NullReferenceException("Null pointer exception from java");
                properties = value;
            }
        }
        protected internal IDictionary<ArkName, IProperty> properties = new Dictionary<ArkName, IProperty>();

        //public int getSize(bool nameTable)
        //{
        //    // UUID item names.size() unkBool unkIndex (locationData!=null) propertiesOffset unkInt
        //    int size = 16 + Integer.BYTES * 7;

        //    size += ArkArchive.getNameLength(className, nameTable);

        //    if (names != null)
        //    {
        //        size += names.Select(n => ArkArchive.getNameLength(n, nameTable)).Sum();
        //    }

        //    if (locationData != null)
        //    {
        //        size += (int)locationData.Size;
        //    }

        //    return size;
        //}

        //public int getPropertiesSize(bool nameTable)
        //{
        //    int size = ArkArchive.getNameLength(qowyn.ark.properties.Property_Fields.NONE_NAME, nameTable);

        //    size += properties.Select(p => p.calculateSize(nameTable)).Sum();

        //    if (extraData != null)
        //    {
        //        size += extraData.calculateSize(nameTable);
        //    }

        //    return size;
        //}

        public void read(ArkArchive archive)
        {
            var uuidMostSig = archive.GetLong();
            var uuidLeastSig = archive.GetLong();
            var key = Tuple.Create(uuidMostSig, uuidLeastSig);

            Guid uuid = Guid.Empty;
            if (!_uuidCache.TryGetValue(key, out uuid))
            {
                var bytes = new byte[16];
                Array.Copy(BitConverter.GetBytes(uuidMostSig), bytes, 8);
                Array.Copy(BitConverter.GetBytes(uuidLeastSig), 0, bytes, 8, 8);
                uuid = new Guid(bytes);
                _uuidCache.Add(key, uuid);
            }
            Uuid = uuid;

            ClassName = archive.GetName();

            IsItem = archive.GetBoolean();

            var countNames = archive.GetInt();
            Names = new List<ArkName>();

            for (int nameIndex = 0; nameIndex < countNames; nameIndex++)
            {
                Names.Add(archive.GetName());
            }

            UnkBool = archive.GetBoolean();
            UnkIndex = archive.GetInt();

            var countLocationData = archive.GetInt();

            if (countLocationData > 1) _logger.Warn($"countLocationData > 1 at {archive.Position - 4:X}");

            if (countLocationData != 0)
            {
                Location = new LocationData(archive);
            }

            _propertiesOffset = archive.GetInt();

            var shouldBeZero = archive.GetInt();
            if (shouldBeZero != 0) _logger.Warn($"Expected int after propertiesOffset to be 0 but found {shouldBeZero} at {archive.Position - 4:X}");
        }

        //private static ArkName _itemId = ArkName.Create("ItemId", 0);
        private static ArkName _dinoId1 = ArkName.Create("DinoID1", 0);
        private static ArkName _tamerString = ArkName.Create("TamerString", 0);
        private static ArkName _tamingTeamID = ArkName.Create("TamingTeamID", 0);
        private static ArkName _ownerName = ArkName.Create("OwnerName", 0);
        private static ArkName _bHasResetDecayTime = ArkName.Create("bHasResetDecayTime", 0);
        private static ArkName _bInitializedMe = ArkName.Create("bInitializedMe", 0);
        private static ArkName _currentStatusValues = ArkName.Create("CurrentStatusValues", 0);
        private static ArkName _raft_bp_c = ArkName.Create("Raft_BP_C", 0);
        private static ArkName _structurePaintingComponent = ArkName.Create("StructurePaintingComponent", 0);
        private static ArkName _male = ArkName.Create("PlayerPawnTest_Male_C", 0);
        private static ArkName _female = ArkName.Create("PlayerPawnTest_Female_C", 0);
        private static ArkName _droppedItem = ArkName.Create("DroppedItemGenericLowQuality_C", 0);

        public void loadProperties(ArkArchive archive, GameObject next, long propertiesBlockOffset, int? nextGameObjectPropertiesOffset = null)
        {
            var offset = propertiesBlockOffset + _propertiesOffset;
            var nextOffset = nextGameObjectPropertiesOffset != null ? propertiesBlockOffset + nextGameObjectPropertiesOffset.Value : (next != null) ? propertiesBlockOffset + next._propertiesOffset : archive.Size - 1;

            archive.Position = offset;

            properties.Clear();
            try
            {
                var property = PropertyRegistry.readProperty(archive);

                while (property != null)
                {
                    properties.Add(_arkNameCache.Create(property.Name.Token, property.Index), property);
                    property = PropertyRegistry.readProperty(archive);
                }
            }
            catch (UnreadablePropertyException)
            {
                // Stop reading and ignore possible extra data for now, needs a new field in ExtraDataHandler
                return;
            }
            finally
            {
                //these are in order of most common to least common to keep lookups at a minimum
                if (IsItem) goto SkipRest;

                IsStructure = (Properties.ContainsKey(_ownerName) || Properties.ContainsKey(_bHasResetDecayTime));
                if (IsStructure) goto SkipRest;

                IsCreature = Properties.ContainsKey(_dinoId1);
                IsTamedCreature = IsCreature && (Properties.ContainsKey(_tamerString) || Properties.ContainsKey(_tamingTeamID));
                IsWildCreature = IsCreature && !IsTamedCreature;
                IsRaftCreature = IsTamedCreature && ClassName.Equals(_raft_bp_c);
                if (IsCreature) goto SkipRest;

                IsStatusComponent = Properties.ContainsKey(_currentStatusValues);
                IsDinoStatusComponent = IsStatusComponent && ClassName.Token.StartsWith("DinoCharacterStatusComponent_");
                IsPlayerCharacterStatusComponent = IsStatusComponent && !IsDinoStatusComponent && ClassName.Token.StartsWith("PlayerCharacterStatusComponent_");
                if (IsStatusComponent) goto SkipRest;

                IsInventory = Properties.ContainsKey(_bInitializedMe);
                IsStructureInventory = IsInventory && ClassName.Token.StartsWith("PrimalInventoryBP_");
                IsTamedCreatureInventory = IsInventory && !IsStructureInventory && ClassName.Token.StartsWith("DinoTamedInventoryComponent_");
                IsPlayerCharacterInventory = IsInventory && !(IsStructureInventory || IsTamedCreatureInventory) && ClassName.Token.StartsWith("PrimalInventoryComponent");
                IsWildCreatureInventory = IsInventory && !(IsStructureInventory || IsTamedCreatureInventory || IsPlayerCharacterInventory) && ClassName.Token.StartsWith("DinoWildInventoryComponent_");
                if (IsInventory) goto SkipRest;

                IsStructurePaintingComponent = ClassName.Equals(_structurePaintingComponent);
                if (IsStructurePaintingComponent) goto SkipRest;

                IsDroppedItem = ClassName.Equals(_droppedItem);
                if (IsDroppedItem) goto SkipRest;

                IsPlayerCharacter = ClassName.Equals(_male) || ClassName.Equals(_female);
                if (IsPlayerCharacter) goto SkipRest;

                IsSomethingElse = true;

                SkipRest:;
            }

            var distance = nextOffset - archive.Position;

            if (distance > 0)
            {
                ExtraData = ExtraDataRegistry.getExtraData(this, archive, distance);
            }
            else
            {
                ExtraData = null;
            }
        }
        
        public void CollectNames(ISet<string> nameTable)
        {
            nameTable.Add(ClassName.Name);

            if (Names != null)
                foreach (var name in Names) nameTable.Add(name.Name);

            foreach (var property in properties.Values) property.CollectNames(nameTable);

            if (ExtraData is INameContainer) ((INameContainer)ExtraData).CollectNames(nameTable);
        }
    }
}
