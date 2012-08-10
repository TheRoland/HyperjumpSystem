using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace HyperJumpSystem
{
    //The Hyperjump drive is the main jump part.
    class HyperJumpDrive : Jumper
    {
        public static float LOCAL_JUMP_DISTANCE = 70000;
        protected Rect windowPos;

        RemoteEndpoint chosenLaunchFacility;
        private bool clamped;
        private bool driveUnlocked = false;
        private int targetIndex = -1;
        private int previousIndex = -1;
        private Vessel targetVessel = null;

        private ParticleEmitter emitter;
        private static float PARTICLES_IDLE = 0.5f;
        private static float PARTICLES_ON = 20;
        protected override void onPartAwake()
        {
            clamped = true;
            base.onPartAwake();
            configureParticles();
            

        }
        public bool isEndpointTunnelable()
        {
            List<Vessel> endpoints = SpaceDockUtilities.GetSpaceDocks();
            foreach (Vessel sd in endpoints)
            {
                if (sd == this.vessel)
                {
                    continue;
                }
                RemoteEndpoint potentialDock = new RemoteEndpoint(sd);
                if (potentialDock.GetPreciseDistanceToDestination(vessel).magnitude < HyperEndpoint.TUNNEL_JUMP_DISTANCE)
                {
                    return true;
                }
            }
            return false;
        }
        public String getDistanceApprox(Vessel v)
        {
            String retVal;
            float distance = Vector3.Distance( (Vector3d)v.transform.position , vessel.transform.position) ;

            if (distance > 10000)
            {
                retVal = (distance / 1000).ToString("F1") + "km";
            }
            else
            {
                retVal = distance.ToString("F1") + "m";
            }
            return retVal;

        }
        public bool isEndpointLocal(Vessel v)
        {
            RemoteEndpoint potentialDock = new RemoteEndpoint(v);
            return (potentialDock.GetPreciseDistanceToDestination(vessel).magnitude < LOCAL_JUMP_DISTANCE);


        }
        private void configureParticles()
        {
            GameObject obj = Instantiate(UnityEngine.Resources.Load("Effects/fx_exhaustFlame_blue")) as GameObject;
            obj.transform.parent = transform;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localPosition = Vector3.zero;

            emitter = obj.particleEmitter;
            emitter.emit = true;
            emitter.minEnergy = 0.5f;
            emitter.maxEnergy = 1.5f;

            emitter.minSize = 0.2f;
            emitter.maxSize = 0.4f;

            emitter.minEmission = PARTICLES_IDLE;
            emitter.maxEmission = PARTICLES_IDLE * 2;

            emitter.rndVelocity = Vector3.one * 2;
            emitter.localVelocity = Vector3.zero;

            emitter.useWorldSpace = false;
            (obj.GetComponent<ParticleAnimator>() as ParticleAnimator).sizeGrow = 2f;
        }

        private bool AttemptToBeBuiltAtDestination()
        {
            //@TODO: Actually do some science-fiction rules here. Maybe:
            // 1. hightAboveTerrain > 5000
            //or
            // 2/ veloctity about 88 MPH (or whatever)?
            //Let's try the first one.
            return true;
        }

        #region UI

        private void WindowGUI(int windowID)
        {
            Boolean isLegal = false;
            GUIStyle mySty = new GUIStyle(GUI.skin.button);
            mySty.normal.textColor = mySty.focused.textColor = Color.white;
            mySty.hover.textColor = mySty.active.textColor = Color.yellow;
            mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
            mySty.padding = new RectOffset(8, 8, 8, 8);

            GUIStyle headingStyle = new GUIStyle(GUI.skin.label);
            headingStyle.alignment = TextAnchor.MiddleCenter;
            headingStyle.fontStyle = FontStyle.Bold;

            GUILayout.BeginVertical();
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);


            if (GetCargoMode() == CargoMode.Idle)
            {
                GUILayout.Label("Drive Targeting:", headingStyle);
                List<Vessel> dockList = new List<Vessel>();
                foreach (Vessel v in FlightGlobals.Vessels)
                {
                    //skip our own vessel
                    if (v == this.vessel)
                    {
                        continue;
                    }
                    //Add all spacedocks to the list.
                    if (SpaceDockUtilities.IsSpaceDock(v))
                    {
                        dockList.Add(v);
                    }
                }

               targetIndex = GUILayout.SelectionGrid(targetIndex, dockList.ConvertAll(v => v.vesselName).ToArray(), 1, mySty, GUILayout.ExpandWidth(true));
               if (targetIndex != previousIndex)
               {
                   try
                   {
                       Vessel v = dockList[targetIndex];
                       print("Choosing: " + v.name);
                       chosenLaunchFacility = new RemoteEndpoint(v);
                       SetDestination(chosenLaunchFacility);
                       previousIndex = targetIndex;
                       targetVessel = v;
                   }
                   catch
                   {
                   }
               }
            }
            String range = "No Target";
            String jumpLabel = "Hyperjump Not Possible";
            if (targetVessel != null)
            {
                
                if (isEndpointLocal(targetVessel))
                {
                    range = "In Range";
                    jumpLabel = "Activate Local Jump";
                    isLegal = true;
                }
                else if (isEndpointTunnelable())
                {
                    range = "In Tunnel Range";
                    jumpLabel = "Activate Tunnel Jump";
                    isLegal = true;
                }
                else
                {
                    range = "Out of Range";
                    jumpLabel = "Hyperjump Not Possible";
                    isLegal = false;
                }

            }
             GUILayout.Label("Drive Controls:", headingStyle);
             driveUnlocked = GUILayout.Toggle(driveUnlocked, "Unlock Drive Controls");
             if (driveUnlocked)
             {
                 GUIStyle jumpSty = new GUIStyle(GUI.skin.button);
                 jumpSty.normal.textColor = Color.green;
                 jumpSty.active.textColor = jumpSty.hover.textColor = Color.red;
                 if(isLegal)
                 {

                     if (GUILayout.Button(jumpLabel, jumpSty))
                     {
                         SetCargoMode(CargoMode.JumpRequested);
                         emitter.minEmission = PARTICLES_ON;
                         emitter.maxEmission = PARTICLES_ON * 2;


                     }
                }
                 if (GUILayout.Button("Set Hyperjump Drive Idle"))
                 {
                     SetCargoMode(CargoMode.Idle);
                     emitter.minEmission = PARTICLES_IDLE;
                     emitter.maxEmission = PARTICLES_IDLE * 2;
   
                 }
                 
             }

            
            GUILayout.Label("Drive Status:", headingStyle);
            GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.normal.textColor = Color.yellow;
            if (targetVessel != null)
            {
                GUILayout.Label("Target: " + Vessel.GetSituationString(targetVessel) + "\n" + range + " (" + getDistanceApprox(targetVessel) + ")", statusStyle);
            }
            GUILayout.Label(getModeStatus(GetCargoMode()), statusStyle);
            GUILayout.EndVertical();

            //DragWindow makes the window draggable. The Rect specifies which part of the window it can by dragged by, and is 
            //clipped to the actual boundary of the window. You can also pass no argument at all and then the window can by
            //dragged by any part of it. Make sure the DragWindow command is AFTER all your other GUI input stuff, or else
            //it may "cover up" your controls and make them stop responding to the mouse.
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

        }
        //Get flavor-text for the cargoMode.
        private String getModeStatus(CargoMode m)
        {
            String r = "Unknown";
            switch (m)
            {
                case CargoMode.AcceleratingTowardsDock:
                    r = "Returning to normal space.";
                    break;
                case CargoMode.DriftingTowardsDock:
                    r = "Drifting towards hyperspace anchor.";
                    break;
                case CargoMode.HoldingPosition:
                case CargoMode.Idle:
                    r = "Hyperdrive offline.";
                    break;
                case CargoMode.Jumping:
                    r = "Jumping!";
                    break;
                case CargoMode.JumpRequested:
                case CargoMode.TeleportingToSpaceDock:
                case CargoMode.TeleportRequested:
                    r = "Hyperdrive activated!";
                    break;
                case CargoMode.ReasonableWaitBeforeJump:
                    r = "Hyperdrive charging for jump.";
                    break;

            }
            return r;
        }
        private void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(35, windowPos, WindowGUI, "Hyperjump Drive", GUILayout.MinWidth(200), GUILayout.MinHeight(80));
        }
        protected override void onFlightStart()  //Called when vessel is placed on the launchpad
        {
            if (vessel.isActiveVessel)
            {
                RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
            }
        }
        protected override void onPartStart()
        {
            if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
            {
                windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
            }
        }
        protected override void onPartDestroy()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
        }

        #endregion
    }
}

