namespace SuperSight
{
    using System;

    internal interface ISight : IDisposable
    {
        bool IsActive { get; set; }
        bool MustBeActivated { get; }
        bool MustBeDeactivated { get; }
        void OnActivate();
        void OnDeactivate();
        void OnActiveUpdate();
    }
}
