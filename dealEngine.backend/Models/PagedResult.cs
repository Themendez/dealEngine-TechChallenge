﻿namespace dealEngine.AmadeusFlightApi.Models
{
    public class PagedResult<T>
    {
        public int Total { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public List<T> Data { get; set; } = new();
    }
}
