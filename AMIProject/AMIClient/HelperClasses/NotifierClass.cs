using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace AMIClient.HelperClasses
{
    public class NotifierClass
    {
        private static NotifierClass instance;
        public Notifier notifier;

        public NotifierClass()
        {
            if (notifier == null)
            {
                notifier = new Notifier(cfg =>
                {
                    cfg.PositionProvider = new WindowPositionProvider(
                        parentWindow: App.Current.MainWindow,
                        corner: Corner.TopRight,
                        offsetX: 10,
                        offsetY: 10);

                    cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                        notificationLifetime: TimeSpan.FromSeconds(5),
                        maximumNotificationCount: MaximumNotificationCount.FromCount(10));

                    cfg.Dispatcher = App.Current.Dispatcher;
                });
            }
        }

        public static NotifierClass Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                { 
                    instance = new NotifierClass();
                }

                return instance;
            }
        }
    }
}
