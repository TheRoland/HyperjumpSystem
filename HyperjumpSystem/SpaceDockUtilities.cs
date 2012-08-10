using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.IO;
using KSP.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace HyperJumpSystem
{


    public static class SpaceDockUtilities
    {
        public static string fuelIdentifier = "FuelInSpaceDock";
        public static string rcsFuelIdentifier = "RcsFuelInSpaceDock";
        public static string sparePartsIdentifier = "SparePartsMassInSpaceDock";
        //private static Dictionary<string, int> partsAndCount;
        private static bool binaryFiles = false;

        public static bool IsSpaceDock(Vessel v)
        {
            foreach (ProtoPartSnapshot pps in v.protoVessel.protoPartSnapshots)
            {
                if (pps.partStateValues.ContainsKey("HyperEndpoint"))
                {
                    if (pps.partStateValues["HyperEndpoint"].value_int == 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<Vessel> GetSpaceDocks()
        {
            List<Vessel> spaceDocks = new List<Vessel>();
            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (SpaceDockUtilities.IsSpaceDock(v))
                {
                    spaceDocks.Add(v);
                }
            }
            return spaceDocks;
        }

    }


    public class VesselFuels
    {
        public float Fuel { get; set; }
        public float RcsFuel { get; set; }
    }

}