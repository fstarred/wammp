using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Yemp.Behavior
{
    class DraggableBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Window target;
                    if (this.AssociatedObject is Window)
                        target = (Window)this.AssociatedObject;
                    else
                        target = (Window)Window.GetWindow(this.AssociatedObject);

                    target.DragMove();
                }                                        
            };
        }

        protected override void OnDetaching()
        {

        }
    }
}
