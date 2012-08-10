using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HyperJumpSystem
{
    /// <summary>
    /// The RemoteSpaceDock is a wrapper for the Vessel of the spaceDock (and potentially nearby extensions of it)
    /// </summary>
    public class RemoteEndpoint
    {
        private Vessel dockVessel;
        public RemoteEndpoint(Vessel v)
        {
            dockVessel = v;
        }

        public bool CanFacilityBuildThisVessel(Vessel v)
        {
            return true;
        }



        public Vector3 GetPreciseDistanceToDestination(Vessel currentVessel)
        {
            Vector3 currentVesselPos = currentVessel.transform.position;
            Vector3 spaceDockPos = dockVessel.transform.position;
            //MonoBehaviour.print("Current position: " + currentVesselPos);
            //MonoBehaviour.print("SpaceDock position: " + spaceDockPos);
            Vector3 spaceDockVector_Accurate = spaceDockPos - currentVesselPos;
            return spaceDockVector_Accurate;
        }

        public Vector3 GetSafeDistanceToDestination(Vessel currentVessel)
        {
            Vector3 preciseDistance = GetPreciseDistanceToDestination(currentVessel);
            Vector3 safeDistanceVector = new Vector3();
            if (IsDestinationLanded())
            {
                //special logic about up vector
                Vector3 scalar = new Vector3(8000, 8000, 8000);
                Vector3 spaceDockLandedNormal = dockVessel.transform.up;
                spaceDockLandedNormal.Scale(scalar);
                safeDistanceVector = preciseDistance + spaceDockLandedNormal;
            }
            else
            {
                safeDistanceVector = preciseDistance;
                safeDistanceVector.y += 8000;
            }
            //MonoBehaviour.print("SafeDistance: " + safeDistanceVector);
            return safeDistanceVector;
        }

        public bool IsDestinationLanded()
        {
            return dockVessel.Landed;
        }

        public Vector3 GetVelocityOfDestination(Vessel currentVessel)
        {
            if (IsDestinationLanded())
            {
                return dockVessel.srf_velocity;
            }
            else
            {
                return dockVessel.obt_velocity;
            }
        }



        public Quaternion GetHeadingOfDestination()
        {
            return dockVessel.srfRelRotation;
        }
    }
}
