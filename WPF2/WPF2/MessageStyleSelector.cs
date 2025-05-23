using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPF2
{

    public class MessageStyleSelector : StyleSelector
    {
        public Style? ClientMessageStyle { get; set; }
        public Style? SystemMessageStyle { get; set; }
        public override Style SelectStyle(object? item, DependencyObject container)
        {
            FrameworkElement? element = container as FrameworkElement;
            if (element != null)
            {
                if (item is Message message)
                {
                    //if (message.IsSystemMessage)
                    //    return (Style)element.FindResource("SystemMessageStyle");
                    //else
                    //    return (Style)element.FindResource("ClientMessageStyle");
                    if (message.IsSystemMessage)
                        return SystemMessageStyle ?? base.SelectStyle(item, container);
                    else
                        return ClientMessageStyle ?? base.SelectStyle(item, container);
                }
            }
            return base.SelectStyle(item, container);
        }
    }
}
