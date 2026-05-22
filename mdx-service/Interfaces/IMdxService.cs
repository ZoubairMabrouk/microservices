namespace MdxServices.Interfaces
{
    public interface IMdxService
    {
        IEnumerable<Dictionary<string, object?>> Execute(string mdxQuery);
    }
}
