namespace OrleansPoc
{
    public interface ISomeService
    {
        string GetName();
    }

    public class ActualService : ISomeService
    {
        public string GetName()
        {
            return "I'm injected, yo!";
        }
    }
}