using System.Data;

namespace MdxServices.MDX
{
    public interface IMdxQuery
    {
        DataTable ExecuteMdxQuery(string mdxQuery);
    }
}