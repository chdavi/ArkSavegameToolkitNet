﻿using ArkSavegameToolkitNet.Arrays;
using ArkSavegameToolkitNet.Structs;
using ArkSavegameToolkitNet.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkSavegameToolkitNet.Domain
{
    public class ArkLocation
    {
        //latitude shift, latitude divisor, longitude shift, longitude divisor
        private static Dictionary<string, Tuple<float, float, float, float>> _latlonCalcs = new Dictionary<string, Tuple<float, float, float, float>>
        {
            { "TheIsland", Tuple.Create(50.0f, 8000.0f, 50.0f, 8000.0f) },
            { "TheCenter", Tuple.Create(30.34223747253418f, 9584.0f, 55.10416793823242f, 9600.0f) },
            { "ScorchedEarth_P", Tuple.Create(50.0f, 8000.0f, 50.0f, 8000.0f) },
            { "Valhalla", Tuple.Create(48.813560485839844f, 14750.0f, 48.813560485839844f, 14750.0f) },
            { "MortemTupiu", Tuple.Create(32.479148864746094f, 20000.0f, 40.59893798828125f, 16000.0f) },
            { "ShigoIslands", Tuple.Create(50.0f, 8128.0f, 50.0f, 8128.0f) },
            { "Ragnarok", Tuple.Create(50.009388f, 13100f, 50.009388f, 13100f) },
            { "TheVolcano", Tuple.Create(50.0f, 9181.0f, 50.0f, 9181.0f) },
            { "PGARK", Tuple.Create(0.0f, 6080.0f, 0.0f, 6080.0f) }
        };

        //width, height, latitude-top, longitude-left, longitude-right, latitude-bottom
        private static Dictionary<string, Tuple<int, int, float, float, float, float>> _topoMapCalcs = new Dictionary<string, Tuple<int, int, float, float, float, float>>
        {
            { "TheIsland", Tuple.Create(MapResources.topo_map_TheIsland.Width, MapResources.topo_map_TheIsland.Height, 7.2f, 7.2f, 92.8f, 92.8f) },
            { "TheCenter", Tuple.Create(MapResources.topo_map_TheCenter.Width, MapResources.topo_map_TheCenter.Height, -2.5f, 1f, 104.5f, 101f) },
            { "ScorchedEarth_P", Tuple.Create(MapResources.topo_map_ScorchedEarth_P.Width, MapResources.topo_map_ScorchedEarth_P.Height, 7.2f, 7.2f, 92.8f, 92.8f) },
            { "Ragnarok", Tuple.Create(MapResources.topo_map_Ragnarok.Width, MapResources.topo_map_Ragnarok.Height, 0.0f, 0.0f, 100.0f, 100.0f) }
        };

        public ArkLocation() { }

        public ArkLocation(LocationData locationData, ISaveState savedState)
        {
            X = locationData.X;
            Y = locationData.Y;
            Z = locationData.Z;

            if (savedState?.MapName != null)
            {
                MapName = savedState.MapName;

                Tuple<float, float, float, float> vals = null;
                if (_latlonCalcs.TryGetValue(savedState.MapName, out vals))
                {
                    Latitude = vals.Item1 + Y / vals.Item2;
                    Longitude = vals.Item3 + X / vals.Item4;

                    Tuple<int, int, float, float, float, float> mapvals = null;
                    if (_topoMapCalcs.TryGetValue(savedState.MapName, out mapvals))
                    {
                        TopoMapX = (Longitude - mapvals.Item4) * mapvals.Item1 / (mapvals.Item5 - mapvals.Item4);
                        TopoMapY = (Latitude - mapvals.Item3) * mapvals.Item2 / (mapvals.Item6 - mapvals.Item3);
                    }
                }
            }
        }

        public string MapName { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? TopoMapX { get; set; }
        public float? TopoMapY { get; set; }
    }
}
