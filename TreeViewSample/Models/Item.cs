using System.Collections.ObjectModel;

namespace TreeViewSample.Models
{
    public class Item
    {
        // Properties
        public string                     Name     { get; set; }

        public ObservableCollection<Item> Children { get; set; } = new ObservableCollection<Item>();



        // Public methods
        public override string ToString()
        {
            return Name;
        }
    }
}
