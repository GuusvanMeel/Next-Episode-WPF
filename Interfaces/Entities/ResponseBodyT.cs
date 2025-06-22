namespace Interfaces.Entities
{
    public class ResponseBody<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }


        public static ResponseBody<T> Fail(string message)
        {
            return new ResponseBody<T>
            {
                Success = false,
                Message = message
            };
        }
        public static ResponseBody<T> Ok(T data)
        {
            return new ResponseBody<T>
            {
                Success = true,
                Data = data
            };
        }

    }
}
