using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPF2_Shared;

namespace WPF2
{

    public class MessageDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ClientMessageTemplate { get; set; }
        public DataTemplate? SystemMessageTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement? element = container as FrameworkElement;
            if (element != null)
            {
                if (item is Message message)
                {
                    //if (message.IsSystemMessage)
                    //    return (DataTemplate)element.FindResource("SystemMessageTemplate");
                    //else
                    //    return (DataTemplate)element.FindResource("ClientMessageTemplate");
                    if(message.IsSystemMessage)
                        return SystemMessageTemplate ?? base.SelectTemplate(item, container);
                    else
                        return ClientMessageTemplate ?? base.SelectTemplate(item, container);
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
