namespace Common.Api.Responses;

public class PageResult<T>
{
      public int Page { get; set; }

      public int Size { get; set; }

      public int TotalElements { get; set; }

      public IReadOnlyList<T> Content { get; set; } = [];

      public int TotalPages =>
          Size > 0
              ? (int)Math.Ceiling(TotalElements / (double)Size)
              : 0;
  }