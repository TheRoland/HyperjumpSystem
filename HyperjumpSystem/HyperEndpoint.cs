using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HyperJumpSystem;

/// <summary>
/// The spacedock only has one function - to mark down that it is, in fact, a spacedock.
/// </summary>
class HyperEndpoint : Part
{
    public static float TUNNEL_JUMP_DISTANCE = 15000;
    protected Rect windowPos;
    private string textOfInventory;

    protected override void onPartAwake()
    {
        textOfInventory = "";
        base.onPartAwake();
    }


    public override void onFlightStateSave(Dictionary<string, KSPParseable> partDataCollection)
    {
        partDataCollection["HyperEndpoint"] = new KSPParseable(1, KSPParseable.Type.INT);
        base.onFlightStateSave(partDataCollection);
    }

    private string GetSpaceDockName()
    {
        print("Vessel name: " + vessel.vesselName);
        return vessel.vesselName;
    }






}

