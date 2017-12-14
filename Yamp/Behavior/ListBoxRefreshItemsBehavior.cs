using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Yemp.Behavior
{
    class ListBoxRefreshItemsBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            ((INotifyCollectionChanged)this.AssociatedObject.Items).CollectionChanged += (sender, e) => 
            {
                if (e.OldItems != null)
                    this.AssociatedObject.Items.Refresh();
            };
        }

        protected override void OnDetaching()
        {

        }
    }
}
