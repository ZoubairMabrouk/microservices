namespace MdxServices.Models
{
    public sealed class ApiResponse<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? Error { get; init; }
        public int RowCount { get; init; }

        public static ApiResponse<T> Ok(T data, int rowCount) =>
            new() { Success = true, Data = data, RowCount = rowCount };

        public static ApiResponse<T> Fail(string error) =>
            new() { Success = false, Error = error };
    }
}
