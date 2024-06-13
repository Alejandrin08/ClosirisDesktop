using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace ClosirisDesktop.Model.Utilities {
    public class LoggerManager {

        private static LoggerManager _instance;
        public static LoggerManager Instance {
            get {
                if (_instance == null) {
                    _instance = new LoggerManager(typeof(LoggerManager));
                }
                return _instance;
            }
        }

        public ILog Logger { get; set; }

        public LoggerManager(Type type) {
            Logger = LogManager.GetLogger(type);
        }

        public void LogError(string message, Exception ex) {
            Logger.Error(message, ex);
        }

        public void LogError(string message) {
            Logger.Error(message);
        }

        public void LogFatal(string message, Exception ex) {
            Logger.Fatal(message, ex);
        }

        public void LogInfo(string message) {
            Logger.Info(message);
        }
    }
}
