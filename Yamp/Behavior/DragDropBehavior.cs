using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Yemp.Behavior
{
    class DragDropBehavior : Behavior<UIElement>
    {
        #region DependencyProperties

        public static readonly DependencyProperty DragEnterCommandProperty =
            DependencyProperty.RegisterAttached("DragEnterCommand", typeof(ICommand), typeof(DragDropBehavior));

        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.RegisterAttached("DropCommand", typeof(ICommand), typeof(DragDropBehavior));

        #endregion

        public ICommand DragEnterCommand
        {
            get { return (ICommand)GetValue(DragEnterCommandProperty); }
            set { SetValue(DragEnterCommandProperty, value); }
        }

        public ICommand DropCommand
        {
            get { return (ICommand)GetValue(DropCommandProperty); }
            set { SetValue(DropCommandProperty, value); }
        }

        protected override void OnAttached()
        {
            UIElement element = this.AssociatedObject;

            element.DragEnter -= Element_DragEnter;
            element.DragEnter += Element_DragEnter;

            element.Drop -= Element_Drop;
            element.Drop += Element_Drop;            
        }

        private void Element_Drop(object sender, DragEventArgs e)
        {
            DropCommand.Execute(e);
        }

        private void Element_DragEnter(object sender, DragEventArgs e)
        {
            DragEnterCommand.Execute(e);
        }

        protected override void OnDetaching()
        {

        }
    }
}
