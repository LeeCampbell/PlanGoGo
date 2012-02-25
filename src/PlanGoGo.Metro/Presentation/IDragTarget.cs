namespace BlueCrest.BlueScreen.Presentation
{
    public interface IDragTarget
    {
        bool CanReceiveDrop(IDraggable dragSource);
        void ReceiveDrop(IDraggable dragSource);
    }
}