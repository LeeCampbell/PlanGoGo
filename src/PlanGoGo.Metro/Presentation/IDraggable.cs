namespace BlueCrest.BlueScreen.Presentation
{
    public interface IDraggable
    {
        bool CanDrag();
        bool IsDragging { get; set; }   //TODO: Unused currently. Implement or remove.

        //TODO: Add a GetData method. Should allow for us to have drag to NotePad features.
        //IDataObject GetData();
    }
}