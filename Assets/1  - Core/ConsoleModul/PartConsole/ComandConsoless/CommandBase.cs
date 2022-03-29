namespace ConsoleModul.PartConsole.ComandConsoless
{
    public class CommandBase
    {
        public string Id => Format.Split(' ')[0];
        public string Description { get; }
        public string Format { get; }

        public CommandBase(string description, string format)
        {
            Description = description;
            Format = format;
        }
    }
}