namespace SuperSight.Util
{
    using System;
    using System.Drawing;

    using Rage;
    using Rage.Native;

    internal class Scaleform
    {
        private int handle;
        private string scaleformID;

        public int Handle { get { return handle; } }

        public Scaleform()
        {
        }

        public Scaleform(int handle)
        {
            this.handle = handle;
        }

        public bool Load(string scaleformID)
        {
            int handle = NativeFunction.CallByName<int>("REQUEST_SCALEFORM_MOVIE", scaleformID);

            if (handle == 0) return false;

            this.handle = handle;
            this.scaleformID = scaleformID;

            return true;
        }


        public void Render2D()
        {
            const ulong DrawScaleformMovieDefault = 0x0df606929c105be1;
            NativeFunction.CallByHash<uint>(DrawScaleformMovieDefault, this.Handle, 255, 255, 255, 255);
        }

        public void Dispose()
        {
            NativeFunction.Natives.SET_SCALEFORM_MOVIE_AS_NO_LONGER_NEEDED(ref handle);
        }

        public void CallFunction(string function, params object[] arguments)
        {
            const ulong PushScaleformMovieFunctionHash = 0xf6e48914c7a8694e;
            const ulong PushScaleformMovieFunctionParameterIntHash = 0xc3d0841a0cc546a6;
            const ulong BeginTextComponentHash = 0x80338406f3475e55;
            const ulong AddTextComponentStringHash = 0x6c188be134e074aa;
            const ulong EndTextComponentHash = 0x362e2d3fe93a9959;
            const ulong PushScaleformMovieFunctionParameterFloatHash = 0xd69736aae04db51a;
            const ulong PushScaleformMovieFunctionParameterBoolHash = 0xc58424ba936eb458;
            const ulong PushScaleformMovieFunctionParameterStringHash = 0xba7148484bd90365;
            const ulong PopScaleformMovieFunctionVoidHash = 0xc6796a8ffa375e53;


            NativeFunction.CallByHash<uint>(PushScaleformMovieFunctionHash, this.handle, function);

            foreach (object obj in arguments)
            {
                if (obj.GetType() == typeof(int))
                {
                    NativeFunction.CallByHash<uint>(PushScaleformMovieFunctionParameterIntHash, (int)obj);
                }
                else if (obj.GetType() == typeof(string))
                {
                    NativeFunction.CallByHash<uint>(BeginTextComponentHash, "STRING");
                    NativeFunction.CallByHash<uint>(AddTextComponentStringHash, (string)obj);
                    NativeFunction.CallByHash<uint>(EndTextComponentHash);
                }
                else if (obj.GetType() == typeof(char))
                {
                    NativeFunction.CallByHash<uint>(BeginTextComponentHash, "STRING");
                    NativeFunction.CallByHash<uint>(AddTextComponentStringHash, ((char)obj).ToString());
                    NativeFunction.CallByHash<uint>(EndTextComponentHash);
                }
                else if (obj.GetType() == typeof(float))
                {
                    NativeFunction.CallByHash<uint>(PushScaleformMovieFunctionParameterFloatHash, (float)obj);
                }
                else if (obj.GetType() == typeof(double))
                {
                    NativeFunction.CallByHash<uint>(PushScaleformMovieFunctionParameterFloatHash, (float)((double)obj));
                }
                else if (obj.GetType() == typeof(bool))
                {
                    NativeFunction.CallByHash<uint>(PushScaleformMovieFunctionParameterBoolHash, (bool)obj);
                }
                else if (obj.GetType() == typeof(ScaleformArgumentTXD))
                {
                    NativeFunction.CallByHash<uint>(PushScaleformMovieFunctionParameterStringHash, ((ScaleformArgumentTXD)obj).TXD);
                }
                else
                {
                    Game.LogTrivial(String.Format("Unknown argument type {0} passed to scaleform with handle {1}.", obj.GetType().Name, this.handle));
                }
            }

            NativeFunction.CallByHash<uint>(PopScaleformMovieFunctionVoidHash);
        }
    }

    internal class ScaleformArgumentTXD
    {
        private string _txd;
        public string TXD { get { return this._txd; } }

        public ScaleformArgumentTXD(string txd)
        {
            this._txd = txd;
        }
    }
}
