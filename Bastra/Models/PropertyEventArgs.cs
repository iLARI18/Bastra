namespace Bastra.Models
{
    public class PropertyEventArgs(string propertyName) : EventArgs
    {
        public string PropertyName { get; set; } = propertyName;
    }
}
