using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HyperJumpSystem
{
    public enum CargoMode
    {
        Idle,
        ReasonableWaitBeforeJump,
        TeleportRequested,
        TeleportingToSpaceDock,
        JumpRequested,
        Jumping,
        AcceleratingTowardsDock,
        DriftingTowardsDock,
        HoldingPosition
    }
    public class Jumper : Part
    {
        private CargoMode mode;
        private RemoteEndpoint destination;
        private int teleCount;
        private int jumpCount;
        private int approachCount;
        private int waitCount;
        private Vector3 originalVesselVelocity;
        private int numberOfJumps;

        public void SetCargoMode(CargoMode mode)
        {
            print("Jumper acknowledge: mode now at: " + mode);
            this.mode = mode;
        }

        public CargoMode GetCargoMode()
        {
            return this.mode;
        }

        public void SetDestination(RemoteEndpoint destination)
        {
            this.destination = destination;
        }

        protected override void onPartAwake()
        {
            numberOfJumps = 0;
            mode = CargoMode.Idle;
            base.onPartAwake();
        }

        public int GetNumberOfJumps()
        {
            return numberOfJumps;
        }

        #region MovementMethods

        private void Teleport(Vector3 position)
        {
            print("======================");
            print("Zorkinian: setting position to: " + position.ToString());
            vessel.GoOnRails();
            vessel.Translate(position);
            vessel.GoOffRails();
        }

        private void BumpUp()
        {
            vessel.SetWorldVelocity(new Vector3d(0, 0, -10));
        }

        private void MatchSpeedWithDock()
        {
            vessel.SetWorldVelocity(destination.GetVelocityOfDestination(vessel));
        }

        #endregion

        protected override void onPartFixedUpdate()
        {
            if (mode == CargoMode.Idle)
            {
                 teleCount = 0;
                 jumpCount = 0;
                 approachCount = 0;
                 waitCount = 0;
            }
            if (mode == CargoMode.ReasonableWaitBeforeJump)
            {
                waitCount++;
                if (waitCount > 300)
                {
                    mode = CargoMode.JumpRequested;
                }
            }  

            if (mode == CargoMode.HoldingPosition)
            {
                vessel.SetWorldVelocity(destination.GetVelocityOfDestination(vessel));
            }

            if (mode == CargoMode.AcceleratingTowardsDock)
            {
                int safeDistance = 50;
                if (destination.IsDestinationLanded())
                {
                    safeDistance = 50;
                }

                if (approachCount == 50)
                {
                    approachCount++;
                    originalVesselVelocity = vessel.obt_velocity;
                }
                else if (approachCount > 2000)
                {
                    print("Approach is taking too long. Giving up.");
                    vessel.SetWorldVelocity(destination.GetVelocityOfDestination(vessel));
                    mode = CargoMode.Idle;
                }
                else if (destination.GetPreciseDistanceToDestination(vessel).magnitude < safeDistance)
                {
                    print("Approach successful");
                    vessel.SetWorldVelocity(destination.GetVelocityOfDestination(vessel));
                    mode = CargoMode.HoldingPosition;
                }
                else if (approachCount < 100)
                {
                    approachCount++;
                }
                else
                {

                    approachCount++;

                    Vector3 finalVelocity;
                    //print("OriginalVelocity: " + originalVesselVelocity);
                    //print("Current Velocity: " + vessel.obt_velocity);

                    Vector3 reducedVector = new Vector3();
                    Vector3 scalar = new Vector3(1, 1, 1);
                    scalar = scalar / 1;
                    reducedVector = destination.GetPreciseDistanceToDestination(vessel);
                    //print("Reduced Vector - starts at: " + reducedVector);
                    reducedVector.Scale(scalar);
                    //print("Reduced Vector - scales to: " + reducedVector);
                    finalVelocity = originalVesselVelocity + (reducedVector);
                    //print("FinalVelocity: " + finalVelocity);

                    vessel.SetWorldVelocity(finalVelocity);
                }
            }

            if (mode == CargoMode.DriftingTowardsDock)
            {

            }

            if (mode == CargoMode.TeleportRequested)
            {
                mode = CargoMode.TeleportingToSpaceDock;
                teleCount = 0;
                approachCount = 0;
                jumpCount = 0;
            }

            if (mode == CargoMode.TeleportingToSpaceDock)
            {
                if (teleCount < 1)
                {
                    print("Zork: Going onto Rails");
                    Teleport(new Vector3(0, 0, -3000000));
                    print("Zork: Teleporting: count at " + teleCount);

                    //vessel.transform.position = whereToGoFifth;

                    teleCount++;
                }
                else if (teleCount == 1)
                {
                    Vector3 safeVector = destination.GetSafeDistanceToDestination(vessel);
                    print("Zork: translating to " + safeVector);
                    Teleport(safeVector);
                    RotateToDock();
                    teleCount++;
                }
                else
                {
                    print("Teleport finished.");
                    print("normalizing speed with target");
                    MatchSpeedWithDock();
                    mode = CargoMode.AcceleratingTowardsDock;
                }
            }

            if (mode == CargoMode.JumpRequested)
            {
                numberOfJumps++;
                jumpCount = 0;
                mode = CargoMode.Jumping;
                BumpUp();
            }

            if (mode == CargoMode.Jumping)
            {
                jumpCount++;
                if (jumpCount > 15)
                {
                    mode = CargoMode.TeleportRequested;
                }
            }


            base.onPartFixedUpdate();
        }

        private void RotateToDock()
        {
            vessel.GoOnRails();
            Quaternion necessaryRotation = destination.GetHeadingOfDestination();
            print("Rotating to: " + necessaryRotation);
            vessel.SetRotation(necessaryRotation);
            vessel.GoOffRails();
        }


    }
}
