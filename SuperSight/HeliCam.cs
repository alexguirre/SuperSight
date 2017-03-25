namespace SuperSight
{
    using System.Windows.Forms;

    using Rage;
    using Rage.Native;

    using SuperSight.Util;

    internal class HeliCam : ISight
    {
        // TODO: add settings
        public const Keys ToggleCameraKey = Keys.U, ToggleNightVisionKey = Keys.NumPad9, ToggleThermalVisionKey = Keys.NumPad7;


        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (value != isActive)
                {
                    isActive = value;

                    if (isActive)
                    {
                        OnActivate();
                    }
                    else
                    {
                        OnDeactivate();
                    }
                }
            }
        }

        public bool MustBeActivated
        {
            get
            {
                return Game.LocalPlayer.Character.IsInHelicopter && Game.IsKeyDown(ToggleCameraKey);
            }
        }

        public bool MustBeDeactivated
        {
            get
            {
                Ped playerPed = Game.LocalPlayer.Character;
                Vehicle veh = playerPed.CurrentVehicle;
                return Game.IsKeyDown(ToggleCameraKey) || !playerPed || playerPed.IsDead || !veh || veh.IsDead || !veh.IsHelicopter;
            }
        }

        private bool lsCountyLogoEnabled = true;
        public bool LSCountyLogoEnabled
        {
            get { return lsCountyLogoEnabled; }
            set
            {
                lsCountyLogoEnabled = value;
                hudScaleform?.CallFunction("SET_CAM_LOGO", lsCountyLogoEnabled);
            }
        }

        private Camera camera;
        private Scaleform hudScaleform;
        private Sound backgroundSound, turnSound, zoomSound, searchLoopSound, searchSuccessSound;

        public void OnActivate()
        {
            camera = new Camera(true);
            camera.SetRotationYaw(Game.LocalPlayer.Character.CurrentVehicle.Heading);
            camera.AttachToEntity(Game.LocalPlayer.Character.CurrentVehicle, GetCameraPositionOffsetForModel(Game.LocalPlayer.Character.CurrentVehicle.Model), true);

            hudScaleform = new Scaleform();
            hudScaleform.Load("heli_cam");

            NativeFunction.Natives.RequestStreamedTextureDict("helicopterhud", true);

            hudScaleform.CallFunction("SET_CAM_LOGO", lsCountyLogoEnabled);

            backgroundSound = new Sound();
            turnSound = new Sound();
            zoomSound = new Sound();
            searchLoopSound = new Sound();
            searchSuccessSound = new Sound();

            Sound.RequestAmbientAudioBank("SCRIPT\\POLICE_CHOPPER_CAM");

            NativeFunction.Natives.SetNoiseoveride(true);
            NativeFunction.Natives.SetNoisinessoveride(0.15f);
        }

        public void OnDeactivate()
        {
            NativeFunction.Natives.SetNoiseoveride(false);
            NativeFunction.Natives.SetNoisinessoveride(0.0f);

            if (camera)
                camera.Delete();
            camera = null;

            hudScaleform.Dispose();
            hudScaleform = null;

            backgroundSound.Stop();
            turnSound.Stop();
            zoomSound.Stop();
            searchLoopSound.Stop();
            searchSuccessSound.Stop();

            backgroundSound.ReleaseId();
            turnSound.ReleaseId();
            zoomSound.ReleaseId();
            searchLoopSound.ReleaseId();
            searchSuccessSound.ReleaseId();

            backgroundSound = null;
            turnSound = null;
            zoomSound = null;
            searchLoopSound = null;
            searchSuccessSound = null;

            if (IsNightVisionActive())
            {
                SetNightVision(false);
            }

            if (IsThermalVisionActive())
            {
                SetThermalVision(false);
            }
        }

        public void OnActiveUpdate()
        {
            if (backgroundSound.HasFinished())
                backgroundSound.PlayFrontend("COP_HELI_CAM_BACKGROUND", null);

            NativeFunction.Natives.HideHudAndRadarThisFrame();
            DisableControls(GameControl.Enter, GameControl.VehicleExit, GameControl.VehicleAim, GameControl.VehicleAttack, GameControl.VehicleAttack2, GameControl.VehicleDropProjectile, GameControl.VehicleDuck/*, GameControl.VehicleFlyAttack, GameControl.VehicleFlyAttack2*/, GameControl.VehicleFlyAttackCamera, GameControl.VehicleFlyDuck, GameControl.VehicleFlySelectNextWeapon, GameControl.VehicleFlySelectPrevWeapon, GameControl.VehicleHandbrake, GameControl.VehicleJump, GameControl.LookLeftRight, GameControl.LookUpDown, GameControl.WeaponWheelPrev, GameControl.WeaponWheelNext);

            CheckCameraControls();

            DrawHud();
        }

        private void DrawHud()
        {
            float fovPercentage = -((camera.FOV - 70/*max FOV*/)) / 35/*min FOV*/;
            fovPercentage = MathHelper.Clamp(fovPercentage, 0.0f, 1.0f);

            hudScaleform.CallFunction("SET_ALT_FOV_HEADING", camera.Position.Z, fovPercentage, camera.Rotation.Yaw);

            hudScaleform.Render2D();
        }

        private void CheckCameraControls()
        {
            bool usingController = IsUsingController();

            // rotation
            float moveSpeedMultiplier = (camera.FOV / 100) * (usingController ? 3.5f : 5.25f);

            float yRotMagnitude = NativeFunction.Natives.GetDisabledControlNormal<float>(0, (int)GameControl.LookUpDown) * moveSpeedMultiplier;
            float xRotMagnitude = NativeFunction.Natives.GetDisabledControlNormal<float>(0, (int)GameControl.LookLeftRight) * moveSpeedMultiplier;

            float newPitch = camera.Rotation.Pitch - yRotMagnitude;
            float newYaw = camera.Rotation.Yaw - xRotMagnitude;
            const float PitchMinimum = -70.0f, PitchMaximum = 25.0f;
            camera.Rotation = new Rotator((newPitch >= PitchMaximum || newPitch <= PitchMinimum) ? camera.Rotation.Pitch : newPitch, 0f, newYaw);

            if (yRotMagnitude != 0f || xRotMagnitude != 0)
            {
                if (turnSound.HasFinished())
                {
                    turnSound.PlayFrontend("COP_HELI_CAM_TURN", null);
                }
            }
            else if (!turnSound.HasFinished())
            {
                turnSound.Stop();
            }


            // zoom
            float wheelForwards = 0.0f, wheelBackwards = 0.0f;

            if (usingController && Game.IsControllerButtonDownRightNow(ControllerButtons.A))
            {
                DisableControls(GameControl.VehicleFlyThrottleUp, GameControl.VehicleFlyThrottleDown);
                wheelForwards = NativeFunction.Natives.GetDisabledControlNormal<float>(0, (int)GameControl.VehicleFlyThrottleUp);
                wheelBackwards = NativeFunction.Natives.GetDisabledControlNormal<float>(0, (int)GameControl.VehicleFlyThrottleDown);
            }
            else if(!usingController)
            {
                DisableControls(GameControl.WeaponWheelPrev, GameControl.WeaponWheelNext);
                wheelForwards = NativeFunction.Natives.GetDisabledControlNormal<float>(0, (int)GameControl.WeaponWheelPrev) * 1.725f;
                wheelBackwards = NativeFunction.Natives.GetDisabledControlNormal<float>(0, (int)GameControl.WeaponWheelNext) * 1.725f;
            }

            float fov = camera.FOV - wheelForwards + wheelBackwards;
            fov = MathHelper.Clamp(fov, 1.0f, 70.0f);
            camera.FOV = fov;

            if (wheelForwards != 0.0f || wheelBackwards != 0.0f)
            {
                if (zoomSound.HasFinished())
                {
                    zoomSound.PlayFrontend("COP_HELI_CAM_ZOOM", null);
                }
            }
            else if (!zoomSound.HasFinished())
            {
                zoomSound.Stop();
            }


            // night/thermal vision
            if (Game.IsKeyDown(ToggleNightVisionKey))
            {
                SetNightVision(!IsNightVisionActive());
            }

            if (Game.IsKeyDown(ToggleThermalVisionKey))
            {
                SetThermalVision(!IsNightVisionActive());
            }
        }

        private static void PlayToggleVisionOnSound() => new Sound(-1).PlayFrontend("THERMAL_VISION_GOGGLES_ON_MASTER", null);
        private static void PlayToggleVisionOffSound() => new Sound(-1).PlayFrontend("THERMAL_VISION_GOGGLES_OFF_MASTER", null);

        private static void SetNightVision(bool toggle, bool playSound = true)
        {
            NativeFunction.Natives.SetNightvision(toggle);
            if (playSound)
            {
                if (toggle)
                    PlayToggleVisionOnSound();
                else
                    PlayToggleVisionOffSound();
            }
        }
        private static void SetThermalVision(bool toggle, bool playSound = true)
        {
            NativeFunction.Natives.SetSeethrough(toggle);
            if (playSound)
            {
                if (toggle)
                    PlayToggleVisionOnSound();
                else
                    PlayToggleVisionOffSound();
            }
        }
        private static bool IsNightVisionActive() => NativeFunction.Natives.x2202a3f42c8e5f79<bool>();
        private static bool IsThermalVisionActive() => NativeFunction.Natives.x44b80abab9d80bd3<bool>();

        private static bool IsUsingController() => !NativeFunction.Natives.xa571d46727e2b718<bool>(2);

        private static void DisableControls(params GameControl[] controls)
        {
            foreach (GameControl control in controls)
            {
                Game.DisableControlAction(0, control, true);
            }
        }

        private static Vector3 GetCameraPositionOffsetForModel(Model model)
        {
            if (model == new Model("valkyrie") || model == new Model("valkyrie2"))
                return new Vector3(0.0f, 3.615f, -1.15f);
            else if (model == new Model("polmav"))
                return new Vector3(0.0f, 2.75f, -1.25f);
            else if (model == new Model("maverick"))
                return new Vector3(0.0f, 3.5f, -0.9225f);
            else if (model == new Model("savage"))
                return new Vector3(0.0f, 5.475f, -0.84115f);
            else if (model == new Model("buzzard") || model == new Model("buzzard2"))
                return new Vector3(0.0f, 1.958f, -0.75f);
            else if (model == new Model("cargobob") || model == new Model("cargobob3") || model == new Model("cargobob4"))
                return new Vector3(-0.58225f, 7.15f, -0.95f);
            else if (model == new Model("cargobob2"))
                return new Vector3(0.0f, 6.9625f, -1.0f);
            else if (model == new Model("frogger") || model == new Model("frogger2"))
                return new Vector3(0.0f, 3.25f, -0.5975f);
            else if (model == new Model("annihilator"))
                return new Vector3(-0.5715f, 4.0f, -0.686875f);
            else if (model == new Model("skylift"))
                return new Vector3(0.0f, 4.8385f, -2.275f);
            else if (model == new Model("swift") || model == new Model("swift2"))
                return new Vector3(0.0f, 4.765f, -0.6f);
            else if (model == new Model("supervolito") || model == new Model("supervolito2"))
                return new Vector3(0.0f, 3.145f, -0.9675f);
            else
                return new Vector3(0.0f, 2.75f, -1.25f);
        }
    }
}
