using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeViewSample.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using muxc = Microsoft.UI.Xaml.Controls;

namespace TreeViewSample
{
    public sealed partial class MainPage : Page
    {
        private List<Item> DataSource    = new List<Item>();
        private List<Item> SelectedNodes = new List<Item>();

        private enum SelectionMode
        {
            Select,
            UnSelect
        }





        public MainPage()
        {
            this.InitializeComponent();


            // Load some sample treeview data
            DataSource = GetDessertData();

            // When the page has finished loading, exercise the treeview control to ensure
            // all nodes are "realized" (see the "RealizeTreeViewNodes" method for more)
            this.Loaded += ( sender, e ) => { RealizeTreeViewNodes( DessertTree.RootNodes ); };
        }





        // Private methods


        /// <summary>
        /// The treeview control has NO "NodeSelected" event that fires as a node is checked or unchecked.
        /// However, the Tapped event can be co-opted to simulate a NodeSelected event.
        /// </summary>
        /// <remarks>
        /// Note that the Tapped event fires AFTER the ItemInvoked event.
        /// </remarks>
        private async void DessertTree_OnTapped( object sender, TappedRoutedEventArgs e )
        {
            // If the node's TEXT is tapped, ignore it...we'll let the Tapped even handle it
            if (! (e.OriginalSource is TextBlock))                     return;
            if (! (((TextBlock)e.OriginalSource).DataContext is Item)) return;


            // If a node item is tapped, grab its name
            string nodeName = (((TextBlock)e.OriginalSource).DataContext is Item)
                            ? ((Item)((TextBlock)e.OriginalSource).DataContext).Name
                            : string.Empty;


            // Loop through the treeview's SelectedNodes list. If the tapped node is
            // in the list, it was checked. If it's NOT in the list, it was UNchecked.
            bool foundIt = false;
            foreach (muxc.TreeViewNode node in DessertTree.SelectedNodes)
            {
                // If this node isn't the one we're looking for, continue the loop
                if (((Item)node?.Content)?.Name != nodeName) continue;


                // If we found the node we're looking for, so indicate and break out of the loop
                foundIt = true;
                break;
            }


            // Display a message indicating whether the node was checked or unchecked.
            // Note that you could fire an event here, adding the node and whether it was checked or unchecked as EventArgs.
            //ContentDialog treeViewItemInvoked = new ContentDialog()
            //                                    {
            //                                        Title           = "TreeView",
            //                                        Content         = foundIt ? $"Node '{nodeName}' was checked." : $"Node '{nodeName}' was UNchecked.",
            //                                        CloseButtonText = "OK"
            //                                    };

            //await treeViewItemInvoked.ShowAsync();


            // Update the master SelectedNodes list as well as the added/deleted items lists
            UpdateSelectedNodeLists();
        }


        /// <summary>
        /// Method to load sample treeview data.
        /// </summary>
        private List<Item> GetDessertData()
        {
            var list = new List<Item>();

            Item flavorsCategory = new Item()
                                   {
                                       Name = "Ice Cream Flavors",
                                       Children =
                                       {
                                           new Item() { Name = "Vanilla" },
                                           new Item() { Name = "Strawberry" },
                                           new Item() { Name = "Chocolate" }
                                       }
                                   };

            Item toppingsCategory = new Item()
                                    {
                                        Name = "Toppings",
                                        Children =
                                        {
                                            new Item()
                                            {
                                                Name = "Candy",
                                                Children =
                                                {
                                                    new Item() { Name = "Chocolate" },
                                                    new Item() { Name = "Mint" },
                                                    new Item() { Name = "Sprinkles" }
                                                }
                                            },
                                            new Item()
                                            {
                                                Name = "Fruits",
                                                Children =
                                                {
                                                    new Item() { Name = "Mango" },
                                                    new Item() { Name = "Peach" },
                                                    new Item() { Name = "Kiwi" }
                                                }
                                            },
                                            new Item()
                                            {
                                                Name = "Berries",
                                                Children =
                                                {
                                                    new Item() { Name = "Strawberry" },
                                                    new Item() { Name = "Blueberry" },
                                                    new Item() { Name = "Blackberry" }
                                                }
                                            }
                                        }
                                    };

            list.Add(flavorsCategory);
            list.Add(toppingsCategory);
            return list;
        }


        /// <summary>
        /// Method to convert a list of node items into a string in the form,  Item1, Item2, Item3.
        /// </summary>
        private string GetFormattedNodeNameList( List<Item> nodeNameList )
        {
            string nodeNameListString = string.Empty;


            // Return an empty string if the list passed in is null
            if (nodeNameList is null) return nodeNameListString;


            //  Loop through the list of nodes passed in to get the list of names as a string
            foreach (Item node in nodeNameList)
            {
                nodeNameListString += string.IsNullOrEmpty(nodeNameListString) ? node.Name : $", {node.Name}";
            }


            // Return the result
            return nodeNameListString;
        }


        /// <summary>
        /// Private method to "exercise" the treeview control after it has been loaded. Child nodes whose
        /// parent has not been expanded are "unrealized", and are not added in the treeview's SelectedNodes
        /// list until they become "realized". This is a problem if trying to keep track of selected nodes
        /// in real time -- particularly when a higher-level node is selected (implicitly selecting nodes
        /// below), but the nodes below have not yet been realized. This recursive method traverses the
        /// treeview ensuring all nodes are realized.
        /// </summary>
        /// <remarks>
        /// See the TreeView documentation section titled, "Selection and Realized/Unrealized Nodes".
        /// https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/tree-view#selection-and-realizedunrealized-nodes
        /// </remarks>
        private async void RealizeTreeViewNodes( IList<muxc.TreeViewNode> treeViewNodeList )
        {
            foreach (muxc.TreeViewNode node in treeViewNodeList)
            {
                if (node.HasChildren)
                {
                    node.IsExpanded = true;
                    await Task.Delay( 10 );  // Wait a moment to allow the UI to respond
                    RealizeTreeViewNodes(node.Children);
                    node.IsExpanded = false;
                    await Task.Delay( 10 );  // Wait a moment to allow the UI to respond
                }
            }
        }


        /// <summary>
        /// A recursive method to search for a specific node name within the treeview and if found, select it.
        /// </summary>
        private bool SelectNode( IList<muxc.TreeViewNode> treeViewNodeList, string nodeName, SelectionMode mode )
        {
            foreach (muxc.TreeViewNode node in treeViewNodeList)
            {
                // Search the children if the node has any
                if (node.HasChildren)
                {
                    bool foundIt = SelectNode( node.Children, nodeName, mode );
                    if (foundIt) return true;
                }

                // If no children, then check the node name
                else if (((Item)node.Content)?.Name == nodeName)
                {
                    if (mode == SelectionMode.Select) DessertTree.SelectedNodes.Add( node );
                    else                              DessertTree.SelectedNodes.Remove( node );

                    return true;
                }
            }


            // If we've made it this far we didn't find the node name
            return false;
        }


        /// <summary>
        /// Private method to update the master SelectedNodes list as well as the added/deleted items lists.
        /// </summary>
        private void UpdateSelectedNodeLists()
        {
            // Create a list to hold the new list of selected "leaf" nodes
            List<Item> flavorList   = new List<Item>();
            List<Item> toppingsList = new List<Item>();


            // Loop through the selected nodes to get the list of selected Ice Cream Flavors --
            // we're ONLY looking for "leaf" nodes whose parent is the "Ice Cream Flavors" node
            foreach (muxc.TreeViewNode node in DessertTree.SelectedNodes)
            {
                // If the node's parent is NOT the flavors node, it's a topping not a flavor...continue looping
                if (node.Parent.Content?.ToString() != "Ice Cream Flavors") continue;

                // If the parent node is the flavors node, add it to the list
                flavorList.Add(node.Content as Item);
            }


            //  Loop through the selected nodes to get the list of selected Toppings -- we're ONLY looking for "leaf" nodes
            foreach (muxc.TreeViewNode node in DessertTree.SelectedNodes)
            {
                // If the node has children, it's not a "leaf" node...continue looping
                if (node.HasChildren) continue;

                // If the node's parent IS the flavors node, it's a flavor not a topping...continue looping
                if (node.Parent.Content?.ToString() == "Ice Cream Flavors") continue;

                // If it's a "leaf" node, add it to the list
                toppingsList.Add(node.Content as Item);
            }


            // Show the selected flavors and toppings in the UI
            FlavorList.Text   = GetFormattedNodeNameList( flavorList.OrderBy( x => x.Name ).ToList() );
            ToppingsList.Text = GetFormattedNodeNameList( toppingsList.OrderBy( x => x.Name ).ToList() );


            // Copy the new nodes to a temp list so we can compare to the former selected node list
            List<Item> newNodeList = new List<Item>( flavorList );
            newNodeList.AddRange( toppingsList );



            // List the nodes that were added with this tap (i.e., in the new node list but not in the selected nodes list)
            AddedNodes.Text = GetFormattedNodeNameList( newNodeList.Except( SelectedNodes ).OrderBy( x => x.Name ).ToList() );

            // List the nodes that were deleted with this tap (i.e., in the selected nodes list but not in the new node list)
            DeletedNodes.Text = GetFormattedNodeNameList( SelectedNodes.Except( newNodeList ).OrderBy( x => x.Name ).ToList() );



            // Store the new selected "leaf" nodes
            SelectedNodes = new List<Item>( flavorList );
            SelectedNodes.AddRange( toppingsList );

            // Sort the list and store for the next evaluation
            SelectedNodes = SelectedNodes.OrderBy( x => x.Name ).ToList();
        }





        // Events

        /// <summary>
        /// The ItemInvoked event is ONLY fired when a treeview node's TEXT is tapped.
        /// It is NOT fired when the node's checkbox is checked or unchecked.
        /// It is NOT fired when the expander "handles" are tapped to expand or collapse a node's children.
        /// </summary>
        /// <remarks>
        /// Note that the ItemInvoked event fires BEFORE the Tapped event.
        /// </remarks>
        private async void DessertTree_OnItemInvoked( muxc.TreeView sender, muxc.TreeViewItemInvokedEventArgs e )
        {
            ContentDialog treeViewItemInvoked = new ContentDialog()
                                                {
                                                    Title           = "TreeView",
                                                    Content         = $"Node '{((Item)e.InvokedItem).Name}' was tapped.",
                                                    CloseButtonText = "OK"
                                                };

            await treeViewItemInvoked.ShowAsync();
        }


        /// <summary>
        /// Button click event that fires when the user wants to "check" all treeview nodes.
        /// </summary>
        private void SelectAllNodesButton_Click(object sender, RoutedEventArgs e)
        {
            if (DessertTree.SelectionMode == muxc.TreeViewSelectionMode.Multiple)
            {
// BUG: Problem area begins here!
// The TreeView.SelectAll method consistently generates a fatal exception:
//    "System.Exception: The operation attempted to access data outside the valid range"
#region PROBLEM AREA

                // "Check" all nodes in the treeview
                DessertTree.SelectAll();

#endregion
                // Note that there is no corresponding UNselectAll method.
            }
        }


        /// <summary>
        /// Event that fires when the "Select Node" button is clicked.
        /// </summary>
        private void SelectNodeButton_OnClick( object sender, RoutedEventArgs e )
        {
            // If nothing in the node name textbox, exit immediately
            if (string.IsNullOrWhiteSpace( NodeName.Text )) return;


            // Search for the node and select it when found
            bool updatedNode = SelectNode( DessertTree.RootNodes, NodeName.Text, SelectionMode.Select );

            // If we actually found the specified node name and updated its node, update the selected nodes lists
            if (updatedNode) UpdateSelectedNodeLists();
        }


        /// <summary>
        /// Event that fires when the "UnSelect Node" button is clicked.
        /// </summary>
        private void UnSelectNodeButton_OnClick( object sender, RoutedEventArgs e )
        {
            // If nothing in the node name textbox, exit immediately
            if (string.IsNullOrWhiteSpace( NodeName.Text )) return;


            // Search for the node and select it when found
            bool updatedNode = SelectNode( DessertTree.RootNodes, NodeName.Text, SelectionMode.UnSelect );

            // If we actually found the specified node name and updated its node, update the selected nodes lists
            if (updatedNode) UpdateSelectedNodeLists();
        }
    }
}
