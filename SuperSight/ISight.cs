namespace SuperSight
{
    internal interface ISight
    {
        bool IsActive { get; set; }
        bool MustBeActivated { get; }
        bool MustBeDeactivated { get; }
        void OnActivate();
        void OnDeactivate();
        void OnActiveUpdate();
    }
}
