﻿using ArkSavegameToolkitNet.Arrays;
using ArkSavegameToolkitNet.Structs;
using ArkSavegameToolkitNet.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkSavegameToolkitNet.Domain
{
    public class ArkCloudInventory : ArkClusterDataContainerBase
    {
        private static readonly ArkName _myArkData = ArkName.Create("MyArkData");
        private static readonly ArkName _arkItems = ArkName.Create("ArkItems");
        private static readonly ArkName _arkTamedDinosData = ArkName.Create("ArkTamedDinosData");

        internal IGameObject _cloudinv;

        internal void Decouple()
        {
            _cloudinv = null;
        }

        public ArkCloudInventory()
        {
            Items = new ArkCloudInventoryItem[] { };
            Dinos = new ArkCloudInventoryDino[] { };
        }

        public ArkCloudInventory(string steamId, IGameObject cloudinv, ISaveState saveState, ICloudInventoryDinoData[] dinoData) : this()
        {
            if (saveState == null) throw new ApplicationException("Save state must be set in ArkCloudInventory::ArkCloudInventory");

            _cloudinv = cloudinv;

            SteamId = steamId;
            var mydata = cloudinv.GetPropertyValue<StructPropertyList>(_myArkData);
            var items = mydata.GetPropertyValue<ArkArrayStruct>(_arkItems);
            var dinos = mydata.GetPropertyValue<ArkArrayStruct>(_arkTamedDinosData);
            if (items != null) Items = items.OfType<StructPropertyList>().Select(x => new ArkCloudInventoryItem(x)).ToArray();
            if (dinos != null) Dinos = dinos.OfType<StructPropertyList>().Select((x, i) => new ArkCloudInventoryDino(x, 
                dinoData?.ElementAtOrDefault(i)?.Creature, 
                dinoData?.ElementAtOrDefault(i)?.Status, 
                dinoData?.ElementAtOrDefault(i)?.Inventory,
                saveState)).ToArray();

            SavedAt = saveState.SaveTime;
        }

        public string SteamId { get; set; }
        public ArkCloudInventoryItem[] Items { get; set; }
        public ArkCloudInventoryDino[] Dinos { get; set; }
        public DateTime SavedAt { get; set; }
    }
}