namespace SuperSight.Util
{
    using Rage;
    using Rage.Native;

    internal class Sound
    {
        public int Id { get; set; }

        public Sound(int id)
        {
            this.Id = id;
        }
        public Sound() : this(GetId()) { }

        public void Play(string soundName, string setName, bool p3 = false, int p4 = 0, bool p5 = true)
        {
            if (setName != null) NativeFunction.Natives.PlaySound(this.Id, soundName, setName, p3, p4, p5);
            else NativeFunction.Natives.PlaySound(this.Id, soundName, 0, p3, p4, p5);
        }

        public void PlayFrontend(string soundName, string setName, bool p3 = false)
        {
            if (setName != null) NativeFunction.Natives.PlaySoundFrontend(this.Id, soundName, setName, p3);
            else NativeFunction.Natives.PlaySoundFrontend(this.Id, soundName, 0, p3);
        }

        public void PlayFromEntity(string soundName, string setName, Entity entity, bool p4 = false, int p5 = 0)
        {
            if (setName != null) NativeFunction.Natives.PlaySoundFromEntity(this.Id, soundName, entity, setName, p4, p5);
            else NativeFunction.Natives.PlaySoundFromEntity(this.Id, soundName, entity, 0, p4, p5);
        }

        public void PlayFromPosition(string soundName, string setName, Vector3 position, bool p6 = false, int p7 = 0, bool p8 = false)
        {
            if (setName != null) NativeFunction.Natives.PlaySoundFromCoord(this.Id, soundName, position.X, position.Y, position.Z, setName, p6, p7, p8);
            else NativeFunction.Natives.PlaySoundFromCoord(this.Id, soundName, position.X, position.Y, position.Z, 0, p6, p7, p8);
        }

        public void SetVariable(string variableName, float value)
        {
            NativeFunction.Natives.SetVariableOnSound(this.Id, variableName, value);
        }

        public void Stop()
        {
            NativeFunction.Natives.StopSound(this.Id);
        }

        public bool HasFinished()
        {
            return NativeFunction.Natives.HasSoundFinished<bool>(this.Id);
        }

        public void ReleaseId()
        {
            NativeFunction.Natives.ReleaseSoundId(this.Id);
            this.Id = -1;
        }

        public static int GetId()
        {
            return NativeFunction.CallByName<int>("GET_SOUND_ID");
        }

        public static bool RequestMissionAudioBank(string audioBankName, bool p1 = true)
        {
            return NativeFunction.CallByName<bool>("REQUEST_MISSION_AUDIO_BANK", audioBankName, p1);
        }
        public static bool RequestAmbientAudioBank(string audioBankName, bool p1 = true)
        {
            return NativeFunction.CallByName<bool>("REQUEST_AMBIENT_AUDIO_BANK", audioBankName, p1);
        }
        public static bool RequestScriptAudioBank(string audioBankName, bool p1 = true)
        {
            return NativeFunction.CallByName<bool>("REQUEST_SCRIPT_AUDIO_BANK", audioBankName, p1);
        }
    }
}
