using ConsoleModul.Logger;

namespace ModelCore
{
    
    public class LoggerForModel : BaseLogger
    {
        public static LoggerForModel Instance => _instance == null ? _instance = new LoggerForModel() : _instance;
        private static LoggerForModel _instance;
        
        private LoggerForModel()
        {
            
        }
    }
}